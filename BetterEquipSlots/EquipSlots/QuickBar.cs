using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

internal static class QuickBar
{
	private sealed class Widget
	{
		internal RectTransform Root;

		internal Image Icon;

		internal TMP_Text Name;

		internal TMP_Text Amount;

		internal GameObject Selected;

		internal GameObject TimeBar;

		internal ItemDrop.ItemData ShownItem;

		internal int ShownStack = -1;

		internal string ShownLabel;

		internal bool ShownWorn;

		internal bool ShownAlt;

		internal Image[] FrameImages;

		internal Color[] FrameColors;

		internal Image Food;

		internal GameObject PadCursor;
	}

	private static readonly Color AltFrame = new Color(0.35f, 0.65f, 1f, 1f);

	private static readonly Widget[] Widgets = new Widget[3];

	private static RectTransform s_power;

	private static Hud s_hud;

	internal const string HandleName = "CDEV_QuickSlots";

	private static RectTransform s_handle;

	private static Vector2 s_placedOffset = new Vector2(float.NaN, float.NaN);

	private static float s_placedSpacing = float.NaN;

	private static Vector2 s_powerCorner;

	internal static void Destroy()
	{
		for (int i = 0; i < Widgets.Length; i++)
		{
			if (Widgets[i]?.Root != null)
			{
				GameObject gameObject = Widgets[i].Root.gameObject;
				gameObject.SetActive(value: false);
				gameObject.name = "CDEV_QuickSlot_old";
				UnityEngine.Object.Destroy(gameObject);
			}
			Widgets[i] = null;
		}
		if (s_handle != null)
		{
			s_handle.name = "CDEV_QuickSlots_old";
			UnityEngine.Object.Destroy(s_handle.gameObject);
		}
		s_handle = null;
		s_placedSpacing = float.NaN;
		s_hud = null;
		s_power = null;
	}

	internal static void Create(Hud hud)
	{
		try
		{
			Build(hud);
		}
		catch (Exception arg)
		{
			Plugin.Log.LogError($"Could not build the HUD quick slots: {arg}");
		}
	}

	private static void Build(Hud hud)
	{
		s_hud = hud;
		s_power = hud.m_gpRoot;
		if (s_power == null)
		{
			return;
		}
		for (int i = 0; i < Widgets.Length; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(s_power.gameObject, s_power.parent, worldPositionStays: false);
			gameObject.name = "CDEV_QuickSlot" + (i + 1);
			gameObject.SetActive(value: false);
			Widget widget = new Widget
			{
				Root = (RectTransform)gameObject.transform,
				Icon = Child<Image>(gameObject, "Icon"),
				Name = Child<TMP_Text>(gameObject, "Name"),
				Amount = Child<TMP_Text>(gameObject, "TimeText"),
				Selected = gameObject.transform.Find("selected")?.gameObject,
				TimeBar = gameObject.transform.Find("TimeBar")?.gameObject
			};
			UIInputHandler[] componentsInChildren = gameObject.GetComponentsInChildren<UIInputHandler>(includeInactive: true);
			foreach (UIInputHandler uIInputHandler in componentsInChildren)
			{
				uIInputHandler.enabled = false;
			}
			StripForeign(gameObject);
			widget.TimeBar?.SetActive(value: false);
			if (widget.Selected != null)
			{
				widget.FrameImages = widget.Selected.GetComponentsInChildren<Image>(includeInactive: true);
				widget.FrameColors = new Color[widget.FrameImages.Length];
				for (int k = 0; k < widget.FrameImages.Length; k++)
				{
					widget.FrameColors[k] = widget.FrameImages[k].color;
				}
			}
			Corner(widget.Name, new Vector2(1f, 0f), new Vector2(-3f, 2f), TextAlignmentOptions.BottomRight);
			Corner(widget.Amount, new Vector2(0f, 0f), new Vector2(3f, 2f), TextAlignmentOptions.BottomLeft);
			Widgets[i] = widget;
		}
		GameObject gameObject2 = new GameObject("CDEV_QuickSlots", typeof(RectTransform));
		s_handle = (RectTransform)gameObject2.transform;
		s_handle.SetParent(s_power.parent, worldPositionStays: false);
		s_handle.anchorMin = s_power.anchorMin;
		s_handle.anchorMax = s_power.anchorMax;
		s_handle.pivot = Vector2.zero;
		gameObject2.AddComponent<LayoutElement>().ignoreLayout = true;
		Rect rect = s_power.rect;
		s_powerCorner = s_power.anchoredPosition + new Vector2((1f - s_power.pivot.x) * rect.width, (0f - s_power.pivot.y) * rect.height);
		PlaceHandle();
	}

	private static void PlaceHandle()
	{
		Vector2 value = ModConfig.HudOffset.Value;
		float value2 = ModConfig.HudSpacing.Value;
		Rect rect = s_power.rect;
		s_handle.sizeDelta = new Vector2(3f * rect.width + 2f * value2, rect.height);
		s_handle.anchoredPosition = s_powerCorner + new Vector2(value2, 0f) + value;
		s_placedOffset = value;
		s_placedSpacing = value2;
		BetterUiHost.Rebase("CDEV_QuickSlots", s_handle.anchoredPosition);
	}

	private static void StripForeign(GameObject go)
	{
		MonoBehaviour[] componentsInChildren = go.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			if (monoBehaviour != null && !IsGameOrUnity(monoBehaviour.GetType().Assembly.GetName().Name))
			{
				UnityEngine.Object.Destroy(monoBehaviour);
			}
		}
		LayoutElement layoutElement = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
		layoutElement.ignoreLayout = true;
	}

	private static bool IsGameOrUnity(string assembly)
	{
		if (!assembly.StartsWith("UnityEngine", StringComparison.Ordinal) && !assembly.StartsWith("Unity.", StringComparison.Ordinal))
		{
			switch (assembly)
			{
			default:
				return assembly == "gui_framework";
			case "assembly_valheim":
			case "assembly_guiutils":
			case "assembly_utils":
				break;
			}
		}
		return true;
	}

	private static void Corner(TMP_Text text, Vector2 corner, Vector2 offset, TextAlignmentOptions alignment)
	{
		if (!(text == null))
		{
			text.enableAutoSizing = false;
			text.fontSize = Mathf.Min(text.fontSize, 14f);
			text.alignment = alignment;
			text.overflowMode = TextOverflowModes.Overflow;
			text.textWrappingMode = TextWrappingModes.NoWrap;
			RectTransform rectTransform = text.rectTransform;
			Vector2 vector = (rectTransform.pivot = corner);
			Vector2 anchorMin = (rectTransform.anchorMax = vector);
			rectTransform.anchorMin = anchorMin;
			rectTransform.sizeDelta = new Vector2(60f, 18f);
			rectTransform.anchoredPosition = offset;
		}
	}

	private static T Child<T>(GameObject root, string name) where T : Component
	{
		Transform transform = root.transform.Find(name);
		if (!(transform != null))
		{
			return null;
		}
		return transform.GetComponent<T>();
	}

	internal static void Tick()
	{
		if (s_hud == null || Widgets[0] == null || Widgets[0].Root == null)
		{
			return;
		}
		Player localPlayer = Player.m_localPlayer;
		bool flag = ModConfig.HudEnabled.Value && localPlayer != null && !localPlayer.IsDead();
		int selected = GamepadQuickSlots.Selected;
		bool flag2 = flag && (QuickSlots.AltHeld() || selected >= 3);
		float value = ModConfig.HudSpacing.Value;
		if (s_handle == null)
		{
			return;
		}
		if (ModConfig.HudOffset.Value != s_placedOffset || value != s_placedSpacing)
		{
			PlaceHandle();
		}
		Vector3 localScale = s_handle.localScale;
		for (int i = 0; i < Widgets.Length; i++)
		{
			Widget widget = Widgets[i];
			if (widget.Root.gameObject.activeSelf != flag)
			{
				widget.Root.gameObject.SetActive(flag);
			}
			if (flag)
			{
				Rect rect = widget.Root.rect;
				Vector2 pivot = widget.Root.pivot;
				Vector2 vector = s_handle.anchoredPosition + new Vector2(((float)i * (rect.width + value) + pivot.x * rect.width) * localScale.x, pivot.y * rect.height * localScale.y);
				if (widget.Root.anchoredPosition != vector)
				{
					widget.Root.anchoredPosition = vector;
				}
				if (widget.Root.localScale != localScale)
				{
					widget.Root.localScale = localScale;
				}
				int num = (flag2 ? (i + 3) : i);
				SlotCell slotCell = SlotLayout.QuickCell(num);
				ItemDrop.ItemData itemAt = ((Humanoid)localPlayer).m_inventory.GetItemAt(slotCell.Pos.x, slotCell.Pos.y);
				Draw(widget, localPlayer, itemAt, QuickSlots.Label(num), flag2);
				PadCursor(widget, selected == num);
			}
		}
	}

	private static void PadCursor(Widget widget, bool on)
	{
		if (widget.PadCursor == null)
		{
			if (!on)
			{
				return;
			}
			HotkeyBar hotkeyBar = ((s_hud != null) ? s_hud.GetComponentInChildren<HotkeyBar>(includeInactive: true) : null);
			GameObject gameObject = ((hotkeyBar != null && hotkeyBar.m_elements.Count > 0) ? hotkeyBar.m_elements[0].m_selection : null);
			if (gameObject == null)
			{
				return;
			}
			widget.PadCursor = UnityEngine.Object.Instantiate(gameObject, widget.Root, worldPositionStays: false);
			widget.PadCursor.name = "CDEV_PadCursor";
			RectTransform rectTransform = (RectTransform)widget.PadCursor.transform;
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.offsetMin = new Vector2(-4f, -4f);
			rectTransform.offsetMax = new Vector2(4f, 4f);
			Graphic[] componentsInChildren = widget.PadCursor.GetComponentsInChildren<Graphic>(includeInactive: true);
			foreach (Graphic graphic in componentsInChildren)
			{
				graphic.raycastTarget = false;
			}
		}
		widget.PadCursor.transform.SetAsLastSibling();
		if (widget.PadCursor.activeSelf != on)
		{
			widget.PadCursor.SetActive(on);
		}
	}

	private static void Draw(Widget widget, Player player, ItemDrop.ItemData item, string label, bool alt)
	{
		bool flag = item != null && (item.m_equipped || player.IsItemEquiped(item));
		int num = ((item != null && item.m_shared.m_maxStackSize > 1) ? item.m_stack : (-1));
		if (widget.ShownItem == item && widget.ShownStack == num && widget.ShownLabel == label && widget.ShownWorn == flag && widget.ShownAlt == alt && (item == null || widget.Icon == null || widget.Icon.sprite == item.GetIcon()))
		{
			return;
		}
		widget.ShownItem = item;
		widget.ShownStack = num;
		widget.ShownLabel = label;
		widget.ShownWorn = flag;
		widget.ShownAlt = alt;
		if (widget.FrameImages != null)
		{
			for (int i = 0; i < widget.FrameImages.Length; i++)
			{
				Color color = widget.FrameColors[i];
				widget.FrameImages[i].color = (alt ? new Color(AltFrame.r, AltFrame.g, AltFrame.b, color.a) : color);
			}
		}
		if (widget.Icon != null)
		{
			widget.Icon.enabled = item != null;
			widget.Icon.sprite = item?.GetIcon();
			widget.Icon.color = Color.white;
			EpicLootHost.Icon(widget.Icon, item);
		}
		if (widget.Name != null)
		{
			widget.Name.text = label;
		}
		if (widget.Amount != null)
		{
			widget.Amount.text = ((num >= 0) ? num.ToString() : "");
		}
		widget.Selected?.SetActive(flag);
		bool flag2 = item != null && item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable && (item.m_shared.m_food > 0f || item.m_shared.m_foodStamina > 0f || item.m_shared.m_foodEitr > 0f);
		Image image = (flag2 ? Fork(widget) : widget.Food);
		if (flag2 && image == null)
		{
			widget.ShownItem = null;
		}
		if (image != null)
		{
			image.enabled = flag2;
			if (flag2)
			{
				image.color = SlotPanel.FoodColor(item.m_shared);
			}
		}
	}

	private static Image Fork(Widget widget)
	{
		if (widget.Food != null)
		{
			return widget.Food;
		}
		InventoryElement inventoryElement = InventoryGui.instance?.m_playerGrid?.m_elementPrefab?.GetComponent<InventoryElement>();
		if (inventoryElement == null || inventoryElement.m_food == null)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(inventoryElement.m_food.gameObject, widget.Root, worldPositionStays: false);
		gameObject.name = "Food";
		MonoBehaviour[] componentsInChildren = gameObject.GetComponentsInChildren<MonoBehaviour>(includeInactive: true);
		foreach (MonoBehaviour monoBehaviour in componentsInChildren)
		{
			if (!(monoBehaviour is Graphic))
			{
				UnityEngine.Object.Destroy(monoBehaviour);
			}
		}
		gameObject.transform.SetAsLastSibling();
		widget.Food = gameObject.GetComponent<Image>();
		if (widget.Food != null)
		{
			widget.Food.raycastTarget = false;
		}
		return widget.Food;
	}
}
