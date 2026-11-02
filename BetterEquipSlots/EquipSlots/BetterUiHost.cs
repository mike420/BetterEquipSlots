using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

internal static class BetterUiHost
{
	internal const string Guid = "zenox.betterui";

	private static readonly string[] Names = new string[3] { "CDEV_QuickSlot1", "CDEV_QuickSlot2", "CDEV_QuickSlot3" };

	private static object s_plugin;

	private static FieldInfo s_offsets;

	private static ConfigEntry<bool> s_starsOn;

	private static ConfigEntry<bool> s_durabilityOn;

	private static ConfigEntry<int> s_starSize;

	private static Func<int, string> s_starText;

	private static Color s_starColor;

	private static Action<GuiBar, float> s_durabilityColor;

	private static bool s_lookFailed;

	private static readonly Dictionary<InventoryElement, (Color color, Vector2 size)> QualityDefaults = new Dictionary<InventoryElement, (Color, Vector2)>();

	internal static void Setup()
	{
		if (!Chainloader.PluginInfos.TryGetValue("zenox.betterui", out var value) || value.Instance == null)
		{
			return;
		}
		BindItemLook(value);
		try
		{
			Type type = value.Instance.GetType();
			if (!(type.GetField("AutoDetectExcludedNames", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) is HashSet<string> hashSet))
			{
				Plugin.Log.LogWarning($"BetterUI {value.Metadata.Version}: its list of HUD elements to leave alone was not " + "found, so its HUD editor may pin the quick slots onto the forsaken power.");
				return;
			}
			string[] names = Names;
			foreach (string item in names)
			{
				hashSet.Add(item);
			}
			s_plugin = value.Instance;
			s_offsets = type.GetField("barOffsets", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (type.GetField("barOffsets", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(value.Instance) is IDictionary dictionary)
			{
				string[] names2 = Names;
				foreach (string text in names2)
				{
					dictionary.Remove("Custom:" + text);
				}
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not keep BetterUI's HUD editor off the quick slots: " + ex.Message);
		}
	}

	private static void BindItemLook(PluginInfo info)
	{
		try
		{
			Type type = info.Instance.GetType();
			s_starsOn = type.GetField("ItemLevelStarsEnabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as ConfigEntry<bool>;
			s_starSize = type.GetField("InventoryStarFontSize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as ConfigEntry<int>;
			MethodInfo method = type.GetMethod("StarText", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(int) }, null);
			if (type.GetField("StarColor", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) is Color color)
			{
				s_starColor = color;
			}
			if (s_starsOn != null && s_starSize != null && method != null)
			{
				s_starText = (Func<int, string>)Delegate.CreateDelegate(typeof(Func<int, string>), method);
			}
			s_durabilityOn = type.GetField("DurabilityColorEnabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as ConfigEntry<bool>;
			MethodInfo method2 = type.GetMethod("ApplyDurabilityColor", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[2]
			{
				typeof(GuiBar),
				typeof(float)
			}, null);
			if (s_durabilityOn != null && method2 != null)
			{
				s_durabilityColor = (Action<GuiBar, float>)Delegate.CreateDelegate(typeof(Action<GuiBar, float>), method2);
			}
			if (s_starText == null && s_durabilityColor == null)
			{
				Plugin.Log.LogWarning($"BetterUI {info.Metadata.Version}: its quality stars and durability colours were not " + "found, so the equipment panel shows vanilla's.");
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not read BetterUI's quality stars: " + ex.Message);
			s_starText = null;
			s_durabilityColor = null;
		}
	}

	internal static string QualityText(ItemDrop.ItemData item)
	{
		if (s_starText == null || s_lookFailed || !s_starsOn.Value || item.m_shared.m_maxQuality <= 1)
		{
			return null;
		}
		return $"<size={s_starSize.Value}>{s_starText(item.m_quality)}</size>";
	}

	internal static void Cell(InventoryElement element, ItemDrop.ItemData item)
	{
		if (s_lookFailed || element == null || item == null || (s_starText == null && s_durabilityColor == null))
		{
			return;
		}
		try
		{
			TMP_Text quality = element.m_quality;
			if (s_starText != null && quality != null)
			{
				if (!QualityDefaults.ContainsKey(element))
				{
					QualityDefaults[element] = (quality.color, quality.rectTransform.sizeDelta);
				}
				if (s_starsOn.Value && quality.enabled && item.m_shared.m_maxQuality > 1)
				{
					quality.color = s_starColor;
					quality.textWrappingMode = TextWrappingModes.NoWrap;
					quality.rectTransform.sizeDelta = new Vector2((float)item.m_quality * ((float)s_starSize.Value * 0.85f), 20f);
				}
				else
				{
					(Color, Vector2) tuple = QualityDefaults[element];
					(quality.color, _) = tuple;
					quality.rectTransform.sizeDelta = tuple.Item2;
				}
			}
			if (s_durabilityColor != null && s_durabilityOn.Value && element.m_durability != null && element.m_durability.gameObject.activeSelf && item.m_shared.m_useDurability && item.m_durability > 0f)
			{
				s_durabilityColor(element.m_durability, item.GetDurabilityPercentage());
			}
		}
		catch (Exception ex)
		{
			s_lookFailed = true;
			Plugin.Log.LogWarning("BetterUI's quality stars failed in the equipment panel, switched off for this session: " + ex.Message);
		}
	}

	internal static void Rebase(string name, Vector2 position)
	{
		if (s_plugin == null || s_offsets == null)
		{
			return;
		}
		try
		{
			if (s_offsets.GetValue(s_plugin) is IDictionary dictionary && dictionary.Contains("Custom:" + name))
			{
				object obj = dictionary["Custom:" + name];
				FieldInfo field = obj.GetType().GetField("Captured");
				FieldInfo field2 = obj.GetType().GetField("BaseAnchoredPositions");
				if (field != null && (bool)field.GetValue(obj) && field2?.GetValue(obj) is Vector2[] array && array.Length != 0)
				{
					array[0] = position;
				}
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not tell BetterUI the quick slots' new place: " + ex.Message);
			s_plugin = null;
		}
	}
}
