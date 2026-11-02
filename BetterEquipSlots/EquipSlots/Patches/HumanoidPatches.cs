using System;
using System.Collections.Generic;
using HarmonyLib;

namespace BetterEquipSlots.EquipSlots.Patches;

[HarmonyPatch]
internal static class HumanoidPatches
{
	private static readonly List<ItemDrop.ItemData> Worn = new List<ItemDrop.ItemData>();

	[HarmonyPatch(typeof(Humanoid), "EquipItem")]
	[HarmonyPrefix]
	private static bool EquipItemPrefix(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects, ref bool __result)
	{
		EquipSync.EquipDepth++;
		if (!Plugin.Active || item == null || !(__instance is Player player) || !Jewelry.IsJewelry(item))
		{
			return true;
		}
		__result = EquipJewelry(player, item, triggerEquipEffects);
		return false;
	}

	[HarmonyPatch(typeof(Humanoid), "EquipItem")]
	[HarmonyPostfix]
	private static void EquipItemPostfix(Humanoid __instance, ItemDrop.ItemData item, bool __result)
	{
		if (__result && EquipSync.EquipDepth == 1 && EquipSync.IsLocal(__instance))
		{
			EquipSync.OnEquipped((Player)__instance, item);
		}
	}

	[HarmonyPatch(typeof(Humanoid), "EquipItem")]
	[HarmonyFinalizer]
	private static void EquipItemDone()
	{
		EquipSync.EquipDepth = Math.Max(0, EquipSync.EquipDepth - 1);
	}

	private static bool EquipJewelry(Player player, ItemDrop.ItemData item, bool triggerEquipEffects)
	{
		Inventory inventory = ((Humanoid)player).m_inventory;
		if (!inventory.ContainsItem(item))
		{
			return false;
		}
		if (item.m_equipped && SlotInventory.InOwnCell(item))
		{
			((Humanoid)player).SetupEquipment();
			return true;
		}
		SlotKind kind = Jewelry.KindOf(item);
		if (EquipSync.Busy && !SlotInventory.InOwnCell(item))
		{
			item.m_equipped = true;
			return true;
		}
		if (!SlotInventory.InOwnCell(item))
		{
			using (EquipSync.BusyScope.Enter())
			{
				if (!EquipSync.PlaceIntoSlot(inventory, item, kind))
				{
					return false;
				}
			}
		}
		item.m_equipped = true;
		((Humanoid)player).SetupEquipment();
		if (triggerEquipEffects)
		{
			((Humanoid)player).TriggerEquipEffect(item);
		}
		inventory.Changed(false, false);
		return true;
	}

	[HarmonyPatch(typeof(Humanoid), "UnequipItem")]
	[HarmonyPrefix]
	private static bool UnequipItemPrefix(Humanoid __instance, ItemDrop.ItemData item)
	{
		if (!Plugin.Active || item == null || !(__instance is Player player) || !Jewelry.IsJewelry(item))
		{
			return true;
		}
		if (item.m_equipped)
		{
			item.m_equipped = false;
			((Humanoid)player).SetupEquipment();
		}
		return false;
	}

	[HarmonyPatch(typeof(Humanoid), "UnequipItem")]
	[HarmonyPostfix]
	private static void UnequipItemPostfix(Humanoid __instance, ItemDrop.ItemData item)
	{
		if (Plugin.Active && EquipSync.IsLocal(__instance))
		{
			EquipSync.OnUnequipped((Player)__instance, item);
		}
	}

	[HarmonyPatch(typeof(Humanoid), "UnequipAllItems")]
	[HarmonyPrefix]
	private static void UnequipAllItemsPrefix()
	{
		EquipSync.Enter();
	}

	[HarmonyPatch(typeof(Humanoid), "UnequipAllItems")]
	[HarmonyPostfix]
	private static void UnequipAllItemsPostfix(Humanoid __instance)
	{
		if (!Plugin.Active || !(__instance is Player { m_inventory: not null } player))
		{
			return;
		}
		Jewelry.CollectWorn(player, Worn);
		foreach (ItemDrop.ItemData item in Worn)
		{
			player.UnequipItem(item, triggerEquipEffects: false);
		}
		Worn.Clear();
	}

	[HarmonyPatch(typeof(Humanoid), "UnequipAllItems")]
	[HarmonyFinalizer]
	private static void UnequipAllItemsDone()
	{
		EquipSync.Exit();
	}

	[HarmonyPatch(typeof(Player), "UnequipDeathDropItems")]
	[HarmonyPrefix]
	private static void UnequipDeathDropItemsPrefix()
	{
		EquipSync.Enter();
	}

	[HarmonyPatch(typeof(Player), "UnequipDeathDropItems")]
	[HarmonyFinalizer]
	private static void UnequipDeathDropItemsDone()
	{
		EquipSync.Exit();
	}

	[HarmonyPatch(typeof(Humanoid), "IsItemEquiped")]
	[HarmonyPostfix]
	private static void IsItemEquiped(Humanoid __instance, ItemDrop.ItemData item, ref bool __result)
	{
		if (!__result && item != null && item.m_equipped && Plugin.Active && __instance is Player && Jewelry.IsWorn(__instance, item))
		{
			__result = true;
		}
	}

	[HarmonyPatch(typeof(ItemDrop.ItemData), "IsEquipable")]
	[HarmonyPostfix]
	private static void IsEquipable(ItemDrop.ItemData __instance, ref bool __result)
	{
		if (!__result && Plugin.Active && Jewelry.IsJewelry(__instance))
		{
			__result = true;
		}
	}

	[HarmonyPatch(typeof(Humanoid), "UpdateEquipmentStatusEffects")]
	[HarmonyPostfix]
	private static void UpdateEquipmentStatusEffects(Humanoid __instance)
	{
		if (Plugin.Active && __instance is Player)
		{
			Jewelry.SyncStatusEffects(__instance);
		}
	}

	[HarmonyPatch(typeof(Player), "GetBodyArmor")]
	[HarmonyPostfix]
	private static void GetBodyArmor(Player __instance, ref float __result)
	{
		if (Plugin.Active)
		{
			__result += Jewelry.Armor(__instance);
		}
	}

	[HarmonyPatch(typeof(Player), "ToggleEquipped")]
	[HarmonyPrefix]
	private static bool ToggleEquipped(Player __instance, ItemDrop.ItemData item, ref bool __result)
	{
		if (!Plugin.Active || EquipSync.AllowToggle(__instance, item))
		{
			return true;
		}
		__result = true;
		return false;
	}

	[HarmonyPatch(typeof(Humanoid), "DropInvalidItems")]
	[HarmonyPrefix]
	private static bool DropInvalidItems(Humanoid __instance)
	{
		Inventory inventory = __instance.m_inventory;
		if (!(__instance is Player) || !SlotInventory.IsSlotInventory(inventory))
		{
			return true;
		}
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>();
		HashSet<Vector2i> hashSet = new HashSet<Vector2i>();
		foreach (ItemDrop.ItemData item in inventory.m_inventory)
		{
			if ((!SlotInventory.InGrid(inventory, item.m_gridPos) && SlotLayout.At(item.m_gridPos) == null) || !hashSet.Add(item.m_gridPos))
			{
				list.Add(item);
			}
		}
		ItemDrop.ItemData[] array = list.ToArray();
		foreach (ItemDrop.ItemData itemData in array)
		{
			Vector2i gridPos = itemData.m_gridPos;
			itemData.m_gridPos = new Vector2i(-1000, -1000);
			if (SlotInventory.MoveToGridOrStash(inventory, itemData))
			{
				list.Remove(itemData);
			}
			else
			{
				itemData.m_gridPos = gridPos;
			}
		}
		if (list.Count > 0)
		{
			Plugin.Log.LogWarning($"Dropping {list.Count} item(s) that fit nowhere in the inventory.");
			foreach (ItemDrop.ItemData item2 in list)
			{
				__instance.DropItem(inventory, item2, item2.m_stack);
			}
		}
		return false;
	}
}
