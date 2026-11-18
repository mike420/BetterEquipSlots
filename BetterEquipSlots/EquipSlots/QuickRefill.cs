using System.Collections.Generic;
using HarmonyLib;

namespace BetterEquipSlots.EquipSlots;

/// <summary>
/// When the last of a stack in a quick slot is used up (a potion, a food, ...), puts the next stack of the same item from the
/// inventory into that slot, so its key keeps working.
/// </summary>
[HarmonyPatch]
internal static class QuickRefill
{
	private struct Pending
	{
		internal SlotCell Cell;

		internal string Name;

		internal int Quality;
	}

	private static readonly List<Pending> Waiting = new List<Pending>();

	[HarmonyPatch(typeof(Player), "ConsumeItem")]
	[HarmonyPostfix]
	private static void ConsumeItem(Player __instance, Inventory inventory, ItemDrop.ItemData item, bool __result)
	{
		if (!Plugin.Active || !__result || item?.m_shared == null || !MainConfig.quickRefill.Value || __instance != Player.m_localPlayer || inventory != ((Humanoid)__instance).m_inventory)
		{
			return;
		}
		SlotCell cell = SlotLayout.At(item.m_gridPos);
		if (cell != null && (cell.Kind == SlotKind.Quick || cell.Kind == SlotKind.Food) && !inventory.ContainsItem(item))
		{
			Waiting.Add(new Pending { Cell = cell, Name = item.m_shared.m_name, Quality = item.m_quality });
		}
	}

	internal static void Tick()
	{
		if (Waiting.Count == 0)
		{
			return;
		}
		Player player = Player.m_localPlayer;
		if (player == null || player.IsDead() || player.m_isLoading)
		{
			Waiting.Clear();
			return;
		}
		Inventory inventory = ((Humanoid)player).m_inventory;
		bool changed = false;
		foreach (Pending pending in Waiting)
		{
			if (SlotInventory.ItemIn(inventory, pending.Cell) != null)
			{
				continue;
			}
			ItemDrop.ItemData best = null;
			foreach (ItemDrop.ItemData item in inventory.m_inventory)
			{
				// Only from the bag (or the "did not fit" rows), never from another slot
				SlotCell at = SlotLayout.At(item.m_gridPos);
				if (item.m_shared.m_name != pending.Name || (at != null && at.Kind != SlotKind.Stash))
				{
					continue;
				}
				if (best == null || (item.m_quality == pending.Quality && best.m_quality != pending.Quality))
				{
					best = item;
				}
			}
			if (best != null)
			{
				best.m_gridPos = pending.Cell.Pos;
				changed = true;
			}
		}
		Waiting.Clear();
		if (changed)
		{
			inventory.Changed(false, false);
			EquipSync.MarkDirty();
		}
	}
}
