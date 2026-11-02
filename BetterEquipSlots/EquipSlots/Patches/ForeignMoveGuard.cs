using System;
using HarmonyLib;

namespace BetterEquipSlots.EquipSlots.Patches;

[HarmonyPatch]
internal static class ForeignMoveGuard
{
	private struct Hand
	{
		internal ItemDrop.ItemData Item;

		internal ItemDrop.ItemData Swap;
	}

	private static ItemDrop.ItemData s_handItem;

	private static ItemDrop.ItemData s_swapItem;

	private static int s_handDepth;

	[HarmonyPatch(typeof(InventoryGrid), "DropItem")]
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void DropItemStart(InventoryGrid __instance, ItemDrop.ItemData item, Vector2i pos, out Hand __state)
	{
		__state = new Hand
		{
			Item = s_handItem,
			Swap = s_swapItem
		};
		s_handItem = item;
		s_swapItem = __instance?.m_inventory?.GetItemAt(pos.x, pos.y);
		s_handDepth++;
	}

	[HarmonyPatch(typeof(InventoryGrid), "DropItem")]
	[HarmonyFinalizer]
	private static void DropItemEnd(Hand __state)
	{
		s_handItem = __state.Item;
		s_swapItem = __state.Swap;
		s_handDepth--;
	}

	[HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static void SelectStart(InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos, out Hand __state)
	{
		__state = new Hand
		{
			Item = s_handItem,
			Swap = s_swapItem
		};
		s_handItem = item ?? grid?.GetInventory()?.GetItemAt(pos.x, pos.y);
		s_swapItem = null;
		s_handDepth++;
	}

	[HarmonyPatch(typeof(InventoryGui), "OnSelectedItem")]
	[HarmonyFinalizer]
	private static void SelectEnd(Hand __state)
	{
		s_handItem = __state.Item;
		s_swapItem = __state.Swap;
		s_handDepth--;
	}

	[HarmonyPatch(typeof(Inventory), "MoveItemToThis", new Type[]
	{
		typeof(Inventory),
		typeof(ItemDrop.ItemData),
		typeof(int),
		typeof(int),
		typeof(int)
	})]
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static bool MoveAt(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item, ref bool __result)
	{
		if (!Kept(__instance, fromInventory, item))
		{
			return true;
		}
		__result = false;
		return false;
	}

	[HarmonyPatch(typeof(Inventory), "MoveItemToThis", new Type[]
	{
		typeof(Inventory),
		typeof(ItemDrop.ItemData)
	})]
	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static bool Move(Inventory __instance, Inventory fromInventory, ItemDrop.ItemData item)
	{
		return !Kept(__instance, fromInventory, item);
	}

	private static bool Kept(Inventory to, Inventory from, ItemDrop.ItemData item)
	{
		if (!Plugin.Active || item == null || from == null || from == to || !SlotInventory.IsSlotInventory(from) || !from.ContainsItem(item))
		{
			return false;
		}
		Vector2i gridPos = item.m_gridPos;
		if ((gridPos.y < 20 || gridPos.y > 31) && !Locks.IsLocked(gridPos))
		{
			return false;
		}
		InventoryGui instance = InventoryGui.instance;
		bool flag = s_handDepth > 0 && (item == s_handItem || item == s_swapItem || (instance != null && item == instance.m_dragItem));
		return !flag;
	}
}
