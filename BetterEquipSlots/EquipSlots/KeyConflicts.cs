using System;
using System.Collections.Generic;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

internal static class KeyConflicts
{
	private static readonly List<string> Lines = new List<string>();

	private static readonly HashSet<string> RawMouseButtons = new HashSet<string> { "MouseLeft", "MouseRight", "MouseMiddle", "MouseForward", "MouseBack" };

	private static float s_nextCheck;

	private static string s_shown;

	private static TextMeshProUGUI s_text;

	internal static void Recheck()
	{
		s_nextCheck = 0f;
	}

	internal static void Destroy()
	{
		if (s_text != null)
		{
			UnityEngine.Object.Destroy(s_text.gameObject);
		}
		s_text = null;
		s_shown = null;
	}

	internal static void Tick()
	{
		Hud instance = Hud.instance;
		Player localPlayer = Player.m_localPlayer;
		if (instance == null || localPlayer == null || ZInput.instance == null)
		{
			return;
		}
		if (Time.unscaledTime >= s_nextCheck)
		{
			s_nextCheck = Time.unscaledTime + 2f;
			try
			{
				Lines.Clear();
				if (ModConfig.ShowKeyConflicts.Value)
				{
					Check();
				}
			}
			catch (Exception ex)
			{
				Plugin.Log.LogWarning("Quick slot key check failed: " + ex.Message);
				Lines.Clear();
			}
		}
		if (Lines.Count <= 0 || InventoryGui.IsVisible() || localPlayer.IsDead())
		{
			if (s_text != null && s_text.gameObject.activeSelf)
			{
				s_text.gameObject.SetActive(value: false);
			}
		}
		else if (!(s_text == null) || Create(instance))
		{
			if (!s_text.gameObject.activeSelf)
			{
				s_text.gameObject.SetActive(value: true);
			}
			string text = string.Join("\n", Lines.ToArray());
			if (text != s_shown)
			{
				s_shown = text;
				s_text.text = text;
			}
			Place(instance);
		}
	}

	private static void Check()
	{
		bool flag = ModConfig.ShowOnKey.Value && ModConfig.PanelKey.Value.MainKey != KeyCode.None;
		KeyboardShortcut[] array = new KeyboardShortcut[6];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ModConfig.SlotKey(i);
		}
		for (int j = 0; j < array.Length; j++)
		{
			AddUnreadable(array[j], Localize("$cdev_es_keyconflict_slot", (j + 1).ToString()));
		}
		if (flag)
		{
			AddUnreadable(ModConfig.PanelKey.Value, Localize("$cdev_es_keyconflict_panelname"));
		}
		AddUnreadable(ModConfig.AltModifier.Value, "AltModifier");
		for (int k = 0; k < array.Length; k++)
		{
			KeyboardShortcut keyboardShortcut = array[k];
			if (keyboardShortcut.MainKey == KeyCode.None || QuickSlots.Unreadable(keyboardShortcut) != null)
			{
				continue;
			}
			List<string> list = new List<string>();
			for (int l = 0; l < array.Length; l++)
			{
				if (l != k && Same(keyboardShortcut, array[l]))
				{
					list.Add(Localize("$cdev_es_keyconflict_slot", (l + 1).ToString()));
				}
			}
			if (flag && Same(keyboardShortcut, ModConfig.PanelKey.Value))
			{
				list.Add(Localize("$cdev_es_keyconflict_panelname"));
			}
			if (IsEmpty(keyboardShortcut.Modifiers) && !ModConfig.KeysWinOverVanilla.Value)
			{
				AddGameActions(keyboardShortcut, list);
			}
			AddOtherPlugins(keyboardShortcut, list);
			if (list.Count > 0)
			{
				Lines.Add(Localize("$cdev_es_keyconflict", (k + 1).ToString(), QuickSlots.Label(k), string.Join(", ", list.ToArray())));
			}
		}
		if (!flag)
		{
			return;
		}
		KeyboardShortcut value = ModConfig.PanelKey.Value;
		List<string> list2 = new List<string>();
		for (int m = 0; m < array.Length; m++)
		{
			if (Same(value, array[m]))
			{
				list2.Add(Localize("$cdev_es_keyconflict_slot", (m + 1).ToString()));
			}
		}
		if (IsEmpty(value.Modifiers))
		{
			AddGameActions(value, list2);
		}
		AddOtherPlugins(value, list2);
		if (list2.Count > 0)
		{
			Lines.Add(Localize("$cdev_es_keyconflict_panel", QuickSlots.Label(value), string.Join(", ", list2.ToArray())));
		}
	}

	private static void AddUnreadable(KeyboardShortcut key, string setting)
	{
		string text = QuickSlots.Unreadable(key);
		if (text != null)
		{
			Lines.Add(Localize("$cdev_es_keyunreadable", setting, text));
		}
	}

	private static void AddGameActions(KeyboardShortcut key, List<string> with)
	{
		foreach (KeyValuePair<string, ZInput.ButtonDef> button in ZInput.instance.m_buttons)
		{
			if (!button.Key.StartsWith("Joy", StringComparison.Ordinal) && !RawMouseButtons.Contains(button.Key) && QuickSlots.ActionUses(button.Value?.ButtonAction, key.MainKey))
			{
				with.Add(button.Key);
			}
		}
	}

	private static void AddOtherPlugins(KeyboardShortcut key, List<string> with)
	{
		foreach (PluginInfo value in Chainloader.PluginInfos.Values)
		{
			if (value?.Instance == null || value.Metadata.GUID == BetterEquipSlotsPlugin.PluginGUID)
			{
				continue;
			}
			ConfigFile config = value.Instance.Config;
			foreach (ConfigDefinition key2 in config.Keys)
			{
				ConfigEntryBase configEntryBase = config[key2];
				bool num;
				if (!(configEntryBase.BoxedValue is KeyboardShortcut b))
				{
					if (!(configEntryBase.BoxedValue is KeyCode keyCode) || keyCode == KeyCode.None)
					{
						continue;
					}
					num = keyCode == key.MainKey;
				}
				else
				{
					num = Same(key, b);
				}
				if (num)
				{
					with.Add(value.Metadata.Name + " [" + key2.Section + "] " + key2.Key);
				}
			}
		}
	}

	private static bool Same(KeyboardShortcut a, KeyboardShortcut b)
	{
		if (a.MainKey == KeyCode.None || a.MainKey != b.MainKey)
		{
			return false;
		}
		HashSet<KeyCode> hashSet = new HashSet<KeyCode>(a.Modifiers);
		return hashSet.SetEquals(b.Modifiers);
	}

	private static bool IsEmpty(IEnumerable<KeyCode> keys)
	{
		using (IEnumerator<KeyCode> enumerator = keys.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				KeyCode current = enumerator.Current;
				return false;
			}
		}
		return true;
	}

	private static string Localize(string key, params string[] words)
	{
		StringBuilder stringBuilder = new StringBuilder((Localization.instance != null) ? Localization.instance.Localize(key) : key);
		for (int num = words.Length - 1; num >= 0; num--)
		{
			stringBuilder.Replace("$" + num, words[num]);
		}
		return stringBuilder.ToString();
	}

	private static bool Create(Hud hud)
	{
		HotkeyBar componentInChildren = hud.GetComponentInChildren<HotkeyBar>(includeInactive: true);
		TMP_Text gpName = hud.m_gpName;
		if (componentInChildren == null || gpName == null)
		{
			return false;
		}
		GameObject gameObject = new GameObject("CDEV_QuickSlotKeyConflicts", typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(componentInChildren.transform.parent, worldPositionStays: false);
		Vector2 anchorMin = (rectTransform.anchorMax = new Vector2(0f, 1f));
		rectTransform.anchorMin = anchorMin;
		rectTransform.pivot = new Vector2(0f, 1f);
		rectTransform.sizeDelta = new Vector2(520f, 200f);
		gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
		s_text = gameObject.AddComponent<TextMeshProUGUI>();
		s_text.font = gpName.font;
		s_text.fontSharedMaterial = gpName.fontSharedMaterial;
		s_text.fontSize = 15f;
		s_text.color = new Color(1f, 0.62f, 0.3f, 1f);
		s_text.alignment = TextAlignmentOptions.TopLeft;
		s_text.textWrappingMode = TextWrappingModes.Normal;
		s_text.overflowMode = TextOverflowModes.Overflow;
		s_text.raycastTarget = false;
		s_text.outlineWidth = 0.25f;
		s_text.outlineColor = new Color32(0, 0, 0, 220);
		return true;
	}

	private static void Place(Hud hud)
	{
		HotkeyBar componentInChildren = hud.GetComponentInChildren<HotkeyBar>(includeInactive: true);
		RectTransform rectTransform = (RectTransform)s_text.transform;
		if (!(componentInChildren == null) && rectTransform.parent is RectTransform rectTransform2)
		{
			Vector3[] array = new Vector3[4];
			float num = float.MaxValue;
			float num2 = float.MaxValue;
			RectTransform[] componentsInChildren = componentInChildren.GetComponentsInChildren<RectTransform>(includeInactive: false);
			foreach (RectTransform rectTransform3 in componentsInChildren)
			{
				rectTransform3.GetWorldCorners(array);
				num2 = Mathf.Min(num2, array[0].x);
				num = Mathf.Min(num, array[0].y);
			}
			if (num != float.MaxValue)
			{
				Vector3 vector = rectTransform2.InverseTransformPoint(new Vector3(num2, num, 0f));
				Rect rect = rectTransform2.rect;
				rectTransform.anchoredPosition = new Vector2(vector.x - rect.xMin, vector.y - rect.yMax - 6f);
			}
		}
	}
}
