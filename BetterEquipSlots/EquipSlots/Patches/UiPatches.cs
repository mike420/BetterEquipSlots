using System;
using HarmonyLib;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots.Patches;

[HarmonyPatch]
internal static class UiPatches
{
	private static bool s_ammoFailed;

	[HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
	[HarmonyPrefix]
	private static bool UpdateGui(InventoryGrid __instance, Player player, ItemDrop.ItemData dragItem)
	{
		if (!SlotPanel.IsPanelGrid(__instance))
		{
			return true;
		}
		try
		{
			SlotPanel.Render(__instance, player, dragItem);
		}
		catch (Exception arg)
		{
			Plugin.Log.LogError($"Drawing the equipment panel failed: {arg}");
		}
		return false;
	}

	[HarmonyPatch(typeof(InventoryGrid), "UpdateGui")]
	[HarmonyPostfix]
	private static void UpdateGuiDone(InventoryGrid __instance, Player player)
	{
		if (!Plugin.Active || player == null)
		{
			return;
		}
		try
		{
			EquipProgress.Draw(__instance, player);
		}
		catch (Exception arg)
		{
			Plugin.Log.LogError($"Drawing the equip progress failed: {arg}");
		}
		try
		{
			if (SlotPanel.IsPanelGrid(__instance))
			{
				CellLocks.Refresh(__instance, player);
			}
		}
		catch (Exception arg2)
		{
			Plugin.Log.LogError($"Drawing the cell locks failed: {arg2}");
		}
	}

	[HarmonyPatch(typeof(InventoryGrid), "GetPositionFromIndex")]
	[HarmonyPrefix]
	private static bool GetPositionFromIndex(InventoryGrid __instance, int i, ref Vector2i __result)
	{
		if (!SlotPanel.IsPanelGrid(__instance))
		{
			return true;
		}
		__result = SlotPanel.PositionOf(i);
		return false;
	}

	[HarmonyPatch(typeof(InventoryGrid), "GetElement")]
	[HarmonyPrefix]
	private static bool GetElement(InventoryGrid __instance, int x, int y, ref InventoryElement __result)
	{
		if (!SlotPanel.IsPanelGrid(__instance))
		{
			return true;
		}
		__result = SlotPanel.ElementAt(__instance, x, y);
		return false;
	}

	[HarmonyPatch(typeof(InventoryGrid), "DropItem")]
	[HarmonyPrefix]
	private static bool DropItem(InventoryGrid __instance, Inventory fromInventory, ItemDrop.ItemData item, int amount, Vector2i pos, ref bool __result)
	{
		Inventory inventory = __instance.m_inventory;
		SlotCell cell = (SlotInventory.IsSlotInventory(inventory) ? SlotLayout.At(pos) : null);
		SlotCell slotCell = (SlotInventory.IsSlotInventory(fromInventory) ? SlotLayout.At(item.m_gridPos) : null);
		bool flag = SlotInventory.IsSlotInventory(inventory) && SlotLayout.InRegion(pos);
		if (!flag && slotCell == null)
		{
			return true;
		}
		ItemDrop.ItemData itemAt = inventory.GetItemAt(pos.x, pos.y);
		if (itemAt == item)
		{
			return true;
		}
		bool flag2 = !flag || SlotLayout.Accepts(cell, item);
		bool flag3 = itemAt != null && (itemAt.m_shared.m_name != item.m_shared.m_name || (item.m_shared.m_maxQuality > 1 && itemAt.m_quality != item.m_quality) || itemAt.m_shared.m_maxStackSize == 1) && item.m_stack == amount;
		if ((flag2 & flag3) && slotCell != null)
		{
			flag2 = SlotLayout.Accepts(slotCell, itemAt);
		}
		if (flag2)
		{
			return true;
		}
		__result = false;
		return false;
	}

	[HarmonyPatch(typeof(UITooltip), "AnchorTooltip")]
	[HarmonyPostfix]
	private static void TooltipAbovePanel()
	{
		GameObject tooltip = UITooltip.m_tooltip;
		InventoryGui instance = InventoryGui.instance;
		if (Plugin.Active && !(tooltip == null) && !(instance == null) && !(tooltip.transform.parent == null) && tooltip.transform.parent.IsChildOf(instance.transform))
		{
			Canvas componentInParent = tooltip.transform.parent.GetComponentInParent<Canvas>();
			int num = ((componentInParent != null) ? (Math.Max(componentInParent.sortingOrder, componentInParent.rootCanvas.sortingOrder) + 50) : 50);
			Canvas canvas = tooltip.GetComponent<Canvas>();
			if (canvas == null)
			{
				canvas = tooltip.AddComponent<Canvas>();
			}
			canvas.overrideSorting = true;
			canvas.sortingLayerID = ((componentInParent != null) ? componentInParent.sortingLayerID : 0);
			if (canvas.sortingOrder != num)
			{
				canvas.sortingOrder = num;
			}
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "Awake")]
	[HarmonyPostfix]
	private static void GuiAwake(InventoryGui __instance)
	{
		if (Plugin.Active)
		{
			SlotPanel.Create(__instance);
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnRightClickItem")]
	[HarmonyPrefix]
	private static bool OnRightClickItem(InventoryGui __instance, InventoryGrid grid, ItemDrop.ItemData item)
	{
		if (!Plugin.Active || !SlotPanel.LookMode || item == null || grid != __instance.m_playerGrid || __instance.m_dragGo != null || Player.m_localPlayer == null || grid.m_inventory != ((Humanoid)Player.m_localPlayer).m_inventory)
		{
			return true;
		}
		try
		{
			return !Appearance.TakeLookOf(Player.m_localPlayer, item);
		}
		catch (Exception arg)
		{
			Plugin.Log.LogError($"Taking the look of an item failed: {arg}");
			return true;
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
	[HarmonyPrefix]
	private static void OnSelectedItemPrefix(InventoryGui __instance, InventoryGrid grid, Vector2i pos, out EquipSync.DragState __state)
	{
		__state = new EquipSync.DragState
		{
			Dropping = (__instance.m_dragGo != null && __instance.m_dragItem != null),
			TargetInventory = ((grid != null) ? grid.GetInventory() : null),
			TargetPos = pos,
			SourceInventory = __instance.m_dragInventory,
			SourcePos = ((__instance.m_dragItem != null) ? __instance.m_dragItem.m_gridPos : new Vector2i(-1, -1))
		};
		EquipSync.Enter();
	}

	[HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
	[HarmonyFinalizer]
	private static void OnSelectedItemDone(EquipSync.DragState __state)
	{
		EquipSync.Exit();
		if (!EquipSync.Busy && Plugin.Active)
		{
			try
			{
				EquipSync.AfterDrop(__state);
			}
			catch (Exception arg)
			{
				Plugin.Log.LogError($"Equip after drop failed: {arg}");
			}
		}
	}

	[HarmonyPatch(typeof(InventoryGui), "SetActiveGroup", new Type[]
	{
		typeof(UIGroupHandler),
		typeof(bool)
	})]
	[HarmonyPrefix]
	private static bool SetActiveGroup(InventoryGui __instance, ref UIGroupHandler group)
	{
		if (SlotPanel.Grid != null && group == SlotPanel.Grid.m_uiGroup && __instance.m_playerGrid != null)
		{
			if (GamepadPanel.Active)
			{
				return false;
			}
			group = __instance.m_playerGrid.m_uiGroup;
		}
		return true;
	}

	[HarmonyPatch(typeof(HotkeyBar), "UpdateIcons")]
	[HarmonyPostfix]
	private static void HotbarIcons(HotkeyBar __instance, Player player)
	{
		if (!Plugin.Active)
		{
			return;
		}
		try
		{
			HotbarAmmo.Update(__instance, player);
		}
		catch (Exception arg)
		{
			if (!s_ammoFailed)
			{
				s_ammoFailed = true;
				Plugin.Log.LogError($"Drawing the ammo on the hotbar failed: {arg}");
			}
		}
	}

	[HarmonyPatch(typeof(Hud), "Awake")]
	[HarmonyPostfix]
	private static void HudAwake(Hud __instance)
	{
		if (Plugin.Active)
		{
			QuickBar.Create(__instance);
		}
	}

	[HarmonyPatch(typeof(ZInput), "GetButtonDown")]
	[HarmonyPostfix]
	[HarmonyPriority(0)]
	[HarmonyAfter(new string[] { "com.jotunn.jotunn" })]
	private static void GetButtonDown(string name, ref bool __result)
	{
		if (__result && Plugin.Active && (QuickSlots.TakesButton(name) || GamepadQuickSlots.Takes(name) || GamepadPanel.Takes(name)))
		{
			__result = false;
		}
	}
}
