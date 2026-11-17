using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

/// <summary>
/// Worn gear that is nearly broken: its icon on the panel is tinted red, and a message warns once when an item drops below the
/// threshold. At a workbench the repair button repairs everything that can be repaired in one press.
/// </summary>
[HarmonyPatch]
internal static class Durability
{
	private static readonly HashSet<ItemDrop.ItemData> Warned = new HashSet<ItemDrop.ItemData>();

	private static float s_next;

	private static bool s_repairing;

	internal static float Threshold => Mathf.Clamp(MainConfig.durabilityWarn.Value, 0, 100) / 100f;

	// A worn item (not a broken one: that blinks red already) below the threshold
	internal static bool Low(ItemDrop.ItemData item)
	{
		float threshold = Threshold;
		return threshold > 0f && item?.m_shared != null && item.m_shared.m_useDurability && item.m_durability > 0f && item.GetDurabilityPercentage() <= threshold;
	}

	internal static void Tick()
	{
		if (Time.time < s_next)
		{
			return;
		}
		s_next = Time.time + 2f;
		Player player = Player.m_localPlayer;
		if (player == null || player.IsDead() || player.m_isLoading || Threshold <= 0f)
		{
			return;
		}
		foreach (ItemDrop.ItemData item in ((Humanoid)player).m_inventory.m_inventory)
		{
			if (!item.m_equipped || item.m_shared == null || !item.m_shared.m_useDurability)
			{
				continue;
			}
			if (Low(item))
			{
				if (Warned.Add(item))
				{
					player.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize(item.m_shared.m_name) + " is nearly broken (" + Mathf.RoundToInt(item.GetDurabilityPercentage() * 100f) + "%)");
				}
			}
			else if (item.GetDurabilityPercentage() > Threshold + 0.1f)
			{
				// Repaired: warn again next time
				Warned.Remove(item);
			}
		}
		if (Warned.Count > 64)
		{
			Warned.Clear();
		}
	}

	// The vanilla repair button repairs one item a press; this repairs all of them
	[HarmonyPatch(typeof(InventoryGui), "RepairOneItem")]
	[HarmonyPostfix]
	private static void RepairAll(InventoryGui __instance)
	{
		if (!Plugin.Active || s_repairing || !MainConfig.repairAll.Value)
		{
			return;
		}
		s_repairing = true;
		try
		{
			for (int i = 0; i < 64 && __instance.HaveRepairableItems(); i++)
			{
				__instance.RepairOneItem();
			}
		}
		finally
		{
			s_repairing = false;
		}
	}
}
