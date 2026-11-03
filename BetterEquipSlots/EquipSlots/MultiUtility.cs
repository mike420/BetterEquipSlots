using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

// Several utility items (belts and the like) worn at once, each in its own belt cell of the panel. Built in from aedenthorn's
// Equip Multiple Utility Items 0.7.2 (public domain): the game knows one utility item (m_utilityItem), the others are worn
// items it is taught about here - their stats, eitr regeneration, status effects, and being taken off on death.
[HarmonyPatch]
internal static class MultiUtility
{
	internal const string OldGuid = "aedenthorn.EquipMultipleUtilityItems";

	// The belt cells the panel has room for: the first and four more
	private const int MostItems = 5;

	private static readonly List<ItemDrop.ItemData> Scratch = new List<ItemDrop.ItemData>();

	internal static bool Enabled
	{
		get
		{
			if (Plugin.Active && MainConfig.multiUtility.Value)
			{
				return MaxItems > 1;
			}
			return false;
		}
	}

	internal static int MaxItems => Mathf.Clamp(MainConfig.maxUtilityItems.Value, 1, MostItems);

	// Belt cells past the first, for the panel
	internal static int ExtraCells
	{
		get
		{
			if (!Enabled)
			{
				return 0;
			}
			return MaxItems - 1;
		}
	}

	internal static bool Active => ExtraCells > 0;

	internal static void Setup()
	{
		MainConfig.multiUtility.SettingChanged += (_, _) => Changed();
		MainConfig.maxUtilityItems.SettingChanged += (_, _) => Changed();
	}

	// After every plugin's Awake: the separate mod, if it is still installed, would do all of this a second time
	internal static void SwitchOffOldMod()
	{
		if (!Chainloader.PluginInfos.ContainsKey(OldGuid))
		{
			return;
		}
		try
		{
			new Harmony(OldGuid).UnpatchSelf();
			Plugin.Log.LogWarning("Equip Multiple Utility Items is installed: BetterEquipSlots has it built in (Utility settings), so its patches were switched off. You can remove it.");
		}
		catch (Exception ex)
		{
			Plugin.Log.LogError("Equip Multiple Utility Items is installed and could not be switched off; remove it, as BetterEquipSlots has it built in: " + ex.Message);
		}
	}

	private static void Changed()
	{
		SlotPanel.Invalidate();
		Player player = Player.m_localPlayer;
		if (player != null && Plugin.Active)
		{
			TakeOffOverLimit(player);
		}
	}

	// Switched off, or fewer allowed: the worn ones past the limit come off
	private static void TakeOffOverLimit(Player player)
	{
		int keep = (Enabled ? MaxItems : 1) - (((Humanoid)player).m_utilityItem != null ? 1 : 0);
		Extras(player, Scratch);
		for (int i = Scratch.Count - 1; i >= keep && i >= 0; i--)
		{
			((Humanoid)player).UnequipItem(Scratch[i], false);
		}
		Scratch.Clear();
	}

	private static bool IsUtility(ItemDrop.ItemData item)
	{
		return item != null && item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Utility && !Jewelry.IsJewelry(item) && !BackpackHost.IsAdvanced(item);
	}

	// The worn utility items besides the game's own one
	private static void Extras(Humanoid humanoid, List<ItemDrop.ItemData> into)
	{
		into.Clear();
		Inventory inventory = humanoid.m_inventory;
		if (inventory == null)
		{
			return;
		}
		ItemDrop.ItemData utility = humanoid.m_utilityItem;
		foreach (ItemDrop.ItemData item in inventory.GetAllItems())
		{
			if (item.m_equipped && item != utility && IsUtility(item))
			{
				into.Add(item);
			}
		}
	}

	// Fewer worn than allowed, and none of the same kind
	private static bool HasRoomFor(Humanoid humanoid, ItemDrop.ItemData item)
	{
		Extras(humanoid, Scratch);
		ItemDrop.ItemData utility = humanoid.m_utilityItem;
		if (utility != null)
		{
			Scratch.Add(utility);
		}
		bool result = Scratch.Count < MaxItems && !Scratch.Exists((ItemDrop.ItemData i) => i.m_shared.m_name == item.m_shared.m_name);
		Scratch.Clear();
		return result;
	}

	[HarmonyPatch(typeof(Player), "UpdateModifiers")]
	[HarmonyPostfix]
	private static void AddModifiers(Player __instance)
	{
		if (!Enabled || Player.s_equipmentModifierSourceFields == null)
		{
			return;
		}
		Extras(__instance, Scratch);
		float[] values = __instance.m_equipmentModifierValues;
		for (int i = 0; i < values.Length; i++)
		{
			foreach (ItemDrop.ItemData item in Scratch)
			{
				values[i] += (float)Player.s_equipmentModifierSourceFields[i].GetValue(item.m_shared);
			}
		}
		Scratch.Clear();
	}

	[HarmonyPatch(typeof(Player), "GetEquipmentEitrRegenModifier")]
	[HarmonyPostfix]
	private static void AddEitrRegen(Player __instance, ref float __result)
	{
		if (!Enabled)
		{
			return;
		}
		Extras(__instance, Scratch);
		foreach (ItemDrop.ItemData item in Scratch)
		{
			__result += item.m_shared.m_eitrRegenModifier;
		}
		Scratch.Clear();
	}

	[HarmonyPatch(typeof(Humanoid), "IsItemEquiped")]
	[HarmonyPostfix]
	private static void ExtraIsWorn(Humanoid __instance, ItemDrop.ItemData item, ref bool __result)
	{
		if (!__result && item != null && item.m_equipped && __instance is Player && Enabled && item != __instance.m_utilityItem && IsUtility(item))
		{
			__result = true;
		}
	}

	// Equipping takes a moment (the game queues it): one of the same kind as a worn one, or past the limit, is not queued
	[HarmonyPatch(typeof(Player), "QueueEquipAction")]
	[HarmonyPrefix]
	private static bool QueueOnlyWhenRoom(Player __instance, ItemDrop.ItemData item)
	{
		if (!Enabled || !IsUtility(item) || item.m_equipped || __instance.IsEquipActionQueued(item))
		{
			return true;
		}
		return HasRoomFor(__instance, item);
	}

	// The game swaps the worn utility item for the new one; with room, the new one is put on beside it instead. After the
	// slots' own prefix (jewelry), which counts every equip
	[HarmonyPatch(typeof(Humanoid), "EquipItem")]
	[HarmonyPrefix]
	[HarmonyPriority(Priority.Low)]
	private static bool EquipBeside(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects, ref bool __result)
	{
		if (!Enabled || !IsUtility(item) || !(__instance is Player) || item.m_equipped)
		{
			return true;
		}
		if (__instance.m_utilityItem == null)
		{
			// The game puts it on as its own utility item, if there's room beside the others
			if (HasRoomFor(__instance, item))
			{
				return true;
			}
			__result = false;
			return false;
		}
		if (!__instance.m_inventory.ContainsItem(item) || __instance.InAttack() || __instance.InDodge() || (!__instance.IsDead() && __instance.IsSwimming() && !__instance.IsOnGround()) || (item.m_shared.m_useDurability && item.m_durability <= 0f) || (item.m_shared.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(item.m_shared.m_dlc)))
		{
			return true;
		}
		if (!HasRoomFor(__instance, item))
		{
			__result = false;
			return false;
		}
		item.m_equipped = true;
		__instance.SetupEquipment();
		if (triggerEquipEffects)
		{
			__instance.TriggerEquipEffect(item);
		}
		__result = true;
		return false;
	}

	[HarmonyPatch(typeof(Humanoid), "UpdateEquipmentStatusEffects")]
	[HarmonyPrefix]
	private static void DropStaleEffects(Humanoid __instance)
	{
		if (!Enabled || !(__instance is Player))
		{
			return;
		}
		ItemDrop.ItemData utility = __instance.m_utilityItem;
		Extras(__instance, Scratch);
		SEMan seman = __instance.m_seman;
		foreach (ItemDrop.ItemData item in __instance.m_inventory.GetAllItems())
		{
			StatusEffect effect = item.m_shared.m_equipStatusEffect;
			if (item.m_equipped || item == utility || !IsUtility(item) || effect == null)
			{
				continue;
			}
			bool stillGiven = (utility != null && utility.m_shared.m_equipStatusEffect != null && utility.m_shared.m_equipStatusEffect.name == effect.name) || Scratch.Exists((ItemDrop.ItemData i) => i.m_shared.m_equipStatusEffect != null && i.m_shared.m_equipStatusEffect.name == effect.name);
			if (!stillGiven)
			{
				seman.RemoveStatusEffect(effect.NameHash(), false);
			}
		}
		Scratch.Clear();
	}

	[HarmonyPatch(typeof(Humanoid), "UpdateEquipmentStatusEffects")]
	[HarmonyPostfix]
	private static void AddExtraEffects(Humanoid __instance)
	{
		if (!Enabled || !(__instance is Player))
		{
			return;
		}
		Extras(__instance, Scratch);
		foreach (ItemDrop.ItemData item in Scratch)
		{
			if (item.m_shared.m_equipStatusEffect != null)
			{
				__instance.m_seman.AddStatusEffect(item.m_shared.m_equipStatusEffect, false);
			}
		}
		Scratch.Clear();
	}

	[HarmonyPatch(typeof(Humanoid), "UnequipAllItems")]
	[HarmonyPostfix]
	private static void UnequipAllExtras(Humanoid __instance)
	{
		if (Enabled && __instance is Player)
		{
			UnequipExtras(__instance);
		}
	}

	[HarmonyPatch(typeof(Player), "UnequipDeathDropItems")]
	[HarmonyPostfix]
	private static void UnequipExtrasOnDeath(Player __instance)
	{
		if (Enabled)
		{
			UnequipExtras(__instance);
		}
	}

	private static void UnequipExtras(Humanoid humanoid)
	{
		Extras(humanoid, Scratch);
		ItemDrop.ItemData[] items = Scratch.ToArray();
		Scratch.Clear();
		foreach (ItemDrop.ItemData item in items)
		{
			humanoid.UnequipItem(item, false);
		}
	}
}
