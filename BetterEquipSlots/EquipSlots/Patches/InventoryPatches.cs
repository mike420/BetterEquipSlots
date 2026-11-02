using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots.Patches;

[HarmonyPatch]
internal static class InventoryPatches
{
	[HarmonyPatch(typeof(Inventory), "AddItem", new Type[]
	{
		typeof(ItemDrop.ItemData),
		typeof(int),
		typeof(int),
		typeof(int),
		typeof(bool)
	})]
	[HarmonyPrefix]
	[HarmonyPriority(600)]
	private static bool AddItemAt(Inventory __instance, ItemDrop.ItemData item, int x, int y, ref bool skipValidPositionCheck, ref bool __result)
	{
		if (skipValidPositionCheck || !SlotInventory.IsSlotInventory(__instance) || !SlotLayout.InRegion(new Vector2i(x, y)))
		{
			return true;
		}
		SlotCell slotCell = SlotLayout.At(x, y);
		if (slotCell == null || !SlotLayout.Accepts(slotCell, item))
		{
			__result = false;
			return false;
		}
		skipValidPositionCheck = true;
		return true;
	}

	[HarmonyPatch(typeof(Inventory), "TopFirst")]
	[HarmonyPrefix]
	private static bool TopFirst(ItemDrop.ItemData item, ref bool __result)
	{
		if (!Plugin.Active || !Jewelry.IsJewelry(item))
		{
			return true;
		}
		__result = false;
		return false;
	}

	[HarmonyPatch(typeof(Inventory), "AddItem", new Type[] { typeof(ItemDrop.ItemData) })]
	[HarmonyPrefix]
	private static void AddLooseItem(Inventory __instance, ItemDrop.ItemData item, out bool __state)
	{
		__state = item != null && SlotInventory.IsSlotInventory(__instance) && !__instance.ContainsItem(item);
	}

	[HarmonyPatch(typeof(Inventory), "AddItem", new Type[] { typeof(ItemDrop.ItemData) })]
	[HarmonyPostfix]
	private static void AddLooseItemDone(Inventory __instance, ItemDrop.ItemData item, bool __state, ref bool __result)
	{
		if (!__state || !__result || !__instance.ContainsItem(item))
		{
			return;
		}
		SlotCell slotCell = SlotLayout.At(item.m_gridPos);
		if (slotCell != null && (slotCell.Kind == SlotKind.Quick || slotCell.Kind == SlotKind.Stash))
		{
			Vector2i gridPos = SlotInventory.FreeGridCell(__instance, __instance.TopFirst(item));
			if (gridPos.x >= 0)
			{
				item.m_gridPos = gridPos;
			}
			else
			{
				__instance.m_inventory.Remove(item);
				__result = false;
			}
			__instance.Changed(false, false);
		}
	}

	[HarmonyPatch(typeof(Inventory), "GetEmptySlots")]
	[HarmonyPrefix]
	private static bool GetEmptySlots(Inventory __instance, ref int __result)
	{
		if (!SlotInventory.IsSlotInventory(__instance))
		{
			return true;
		}
		__result = __instance.m_width * __instance.m_height - SlotInventory.GridItemCount(__instance);
		return false;
	}

	[HarmonyPatch(typeof(Inventory), "HaveEmptySlot")]
	[HarmonyPrefix]
	private static bool HaveEmptySlot(Inventory __instance, ref bool __result)
	{
		if (!SlotInventory.IsSlotInventory(__instance))
		{
			return true;
		}
		__result = SlotInventory.GridItemCount(__instance) < __instance.m_width * __instance.m_height;
		return false;
	}

	[HarmonyPatch(typeof(Inventory), "CanAddItem", new Type[]
	{
		typeof(ItemDrop.ItemData),
		typeof(int)
	})]
	[HarmonyPrefix]
	private static bool CanAddItem(Inventory __instance, ItemDrop.ItemData item, int stack, ref bool __result)
	{
		if (!SlotInventory.IsSlotInventory(__instance))
		{
			return true;
		}
		if (stack <= 0)
		{
			stack = item.m_stack;
		}
		int num = __instance.m_width * __instance.m_height - SlotInventory.GridItemCount(__instance);
		__result = __instance.FindFreeStackSpace(item.m_shared.m_name, item.m_worldLevel) + num * item.m_shared.m_maxStackSize >= stack;
		return false;
	}

	[HarmonyPatch(typeof(Inventory), "AddItem", new Type[]
	{
		typeof(string),
		typeof(int),
		typeof(int),
		typeof(int),
		typeof(long),
		typeof(string),
		typeof(Vector2i),
		typeof(bool),
		typeof(bool),
		typeof(bool)
	})]
	[HarmonyPrefix]
	private static void AddNamedItemAt(Inventory __instance, string name, Vector2i position, out int __state)
	{
		__state = -1;
		if (!SlotInventory.IsSlotInventory(__instance) || !SlotLayout.InRegion(position) || __instance.GetItemAt(position.x, position.y) != null)
		{
			return;
		}
		SlotCell slotCell = SlotLayout.At(position);
		GameObject gameObject = ((ObjectDB.instance != null) ? ObjectDB.instance.GetItemPrefab(name) : null);
		ItemDrop itemDrop = ((gameObject != null) ? gameObject.GetComponent<ItemDrop>() : null);
		if (slotCell != null && !(itemDrop == null))
		{
			ItemDrop.ItemData itemData = itemDrop.m_itemData.Clone();
			itemData.m_dropPrefab = gameObject;
			if (SlotLayout.Accepts(slotCell, itemData))
			{
				__state = __instance.m_height;
				__instance.m_height = 32;
			}
		}
	}

	[HarmonyPatch(typeof(Inventory), "AddItem", new Type[]
	{
		typeof(string),
		typeof(int),
		typeof(int),
		typeof(int),
		typeof(long),
		typeof(string),
		typeof(Vector2i),
		typeof(bool),
		typeof(bool),
		typeof(bool)
	})]
	[HarmonyFinalizer]
	private static void AddNamedItemAtDone(Inventory __instance, int __state)
	{
		if (__state >= 0)
		{
			__instance.m_height = __state;
		}
	}

	[HarmonyPatch(typeof(Inventory), "MoveInventoryToGrave")]
	[HarmonyPostfix]
	private static void MoveInventoryToGrave(Inventory __instance, Inventory original)
	{
		if (!SlotInventory.IsSlotInventory(original))
		{
			return;
		}
		try
		{
			long playerId = ((Game.instance != null) ? Game.instance.GetPlayerProfile().GetPlayerID() : 0);
			SlotMemory.CompactGrave(__instance, playerId);
		}
		catch (Exception arg)
		{
			Plugin.Log.LogError($"Laying out the grave failed: {arg}");
		}
	}

	[HarmonyPatch(typeof(Inventory), "MoveAll")]
	[HarmonyPrefix]
	private static void MoveAll(Inventory __instance, Inventory fromInventory)
	{
		if (!SlotInventory.IsSlotInventory(__instance))
		{
			return;
		}
		try
		{
			SlotMemory.TakeBack(__instance, fromInventory);
		}
		catch (Exception arg)
		{
			Plugin.Log.LogError($"Taking the grave back failed: {arg}");
		}
	}

	[HarmonyPatch(typeof(Inventory), "StackAll")]
	[HarmonyPrefix]
	private static void StackAll(Inventory fromInventory, out List<ItemDrop.ItemData> __state)
	{
		__state = null;
		if (!SlotInventory.IsSlotInventory(fromInventory))
		{
			return;
		}
		List<ItemDrop.ItemData> inventory = fromInventory.m_inventory;
		for (int num = inventory.Count - 1; num >= 0; num--)
		{
			if (SlotLayout.InRegion(inventory[num].m_gridPos) || Locks.IsLocked(inventory[num].m_gridPos))
			{
				if (__state == null)
				{
					__state = new List<ItemDrop.ItemData>();
				}
				__state.Add(inventory[num]);
				inventory.RemoveAt(num);
			}
		}
	}

	[HarmonyPatch(typeof(Inventory), "StackAll")]
	[HarmonyFinalizer]
	private static void StackAllDone(Inventory fromInventory, List<ItemDrop.ItemData> __state)
	{
		if (__state != null)
		{
			fromInventory.m_inventory.AddRange(__state);
			fromInventory.Changed(false, false);
		}
	}

	[HarmonyPatch(typeof(Inventory), "Changed")]
	[HarmonyPostfix]
	private static void Changed(Inventory __instance)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer != null && ((Humanoid)localPlayer).m_inventory == __instance)
		{
			EquipSync.MarkDirty();
		}
	}
}
