using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

// The Backpack cell, for the backpacks of two mods:
// - Backpacks (Smoothbrain): a backpack is any item carrying its ItemContainer data; the mod equips it itself, through the
//   game's EquipItem, and tracks it apart from the game's slots. Its own check (validateBackpack, what it hands
//   AzuExtendedPlayerInventory for its backpack slot) tells which items go here.
// - Advanced Backpacks: a backpack is a utility item, so the game would put it on in place of the belt. Here it is worn
//   beside the belt instead, as its own piece (the way Advanced Equipment does it), and it takes no place among the utility
//   items.
[HarmonyPatch]
internal static class BackpackHost
{
	internal const string BackpacksGuid = "org.bepinex.plugins.backpacks";

	internal const string AdvancedGuid = "p0kahontazz.advancedbackpacks";

	private static Func<ItemDrop.ItemData, bool> s_isBackpacksItem;

	private static Func<ItemDrop.ItemData, bool> s_isAdvancedItem;

	private static IList s_advancedPrefabs;

	private static readonly List<ItemDrop.ItemData> Scratch = new List<ItemDrop.ItemData>();

	// Either mod is there: the panel shows the Backpack cell
	internal static bool Active
	{
		get
		{
			if (s_isBackpacksItem == null)
			{
				return s_isAdvancedItem != null;
			}
			return true;
		}
	}

	// After every plugin's Awake, so both mods are loaded whatever the order
	internal static void Setup()
	{
		if (Chainloader.PluginInfos.TryGetValue(BackpacksGuid, out var backpacks) && backpacks.Instance != null)
		{
			try
			{
				Type type = backpacks.Instance.GetType();
				MethodInfo method = AccessTools.Method(type, "validateBackpack", new Type[1] { typeof(ItemDrop.ItemData) });
				if (method != null && method.ReturnType == typeof(bool))
				{
					s_isBackpacksItem = (Func<ItemDrop.ItemData, bool>)Delegate.CreateDelegate(typeof(Func<ItemDrop.ItemData, bool>), method);
					Plugin.Log.LogInfo($"Backpacks {backpacks.Metadata.Version}: its backpacks go in the Backpack slot.");
				}
				else
				{
					Plugin.Log.LogWarning($"Backpacks {backpacks.Metadata.Version}: its backpack check was not found, so its backpacks have no slot here.");
				}
			}
			catch (Exception ex)
			{
				Plugin.Log.LogWarning("Backpacks: its backpack check could not be read (" + ex.Message + "), so its backpacks have no slot here.");
			}
		}
		if (Chainloader.PluginInfos.TryGetValue(AdvancedGuid, out var advanced) && advanced.Instance != null)
		{
			try
			{
				Type type2 = advanced.Instance.GetType().Assembly.GetType("AdvancedBackpacks.Backpacks");
				MethodInfo method2 = ((type2 != null) ? AccessTools.Method(type2, "IsBackpack", new Type[1] { typeof(ItemDrop.ItemData) }) : null);
				if (method2 != null && method2.ReturnType == typeof(bool))
				{
					s_isAdvancedItem = (Func<ItemDrop.ItemData, bool>)Delegate.CreateDelegate(typeof(Func<ItemDrop.ItemData, bool>), method2);
					s_advancedPrefabs = AccessTools.Field(type2, "Prefabs")?.GetValue(null) as IList;
					Plugin.Log.LogInfo($"Advanced Backpacks {advanced.Metadata.Version}: its backpacks go in the Backpack slot, beside the belt.");
				}
				else
				{
					Plugin.Log.LogWarning($"Advanced Backpacks {advanced.Metadata.Version}: its backpack check was not found, so its backpacks have no slot here.");
				}
			}
			catch (Exception ex2)
			{
				Plugin.Log.LogWarning("Advanced Backpacks: its backpack check could not be read (" + ex2.Message + "), so its backpacks have no slot here.");
			}
		}
		if (Active)
		{
			SlotPanel.Invalidate();
			EquipSync.MarkDirty();
		}
	}

	internal static bool IsBackpack(ItemDrop.ItemData item)
	{
		if (item?.m_shared == null)
		{
			return false;
		}
		if (IsAdvanced(item))
		{
			return true;
		}
		if (s_isBackpacksItem == null)
		{
			return false;
		}
		try
		{
			return s_isBackpacksItem(item);
		}
		catch (Exception ex)
		{
			s_isBackpacksItem = null;
			Plugin.Log.LogWarning("Backpacks: its backpack check failed (" + ex.Message + "); its backpacks have no slot here until a restart.");
			return false;
		}
	}

	// An Advanced Backpacks backpack: a utility item the game must not take for the belt
	internal static bool IsAdvanced(ItemDrop.ItemData item)
	{
		if (s_isAdvancedItem == null || item == null)
		{
			return false;
		}
		try
		{
			return s_isAdvancedItem(item);
		}
		catch (Exception ex)
		{
			s_isAdvancedItem = null;
			Plugin.Log.LogWarning("Advanced Backpacks: its backpack check failed (" + ex.Message + "); its backpacks have no slot here until a restart.");
			return false;
		}
	}

	// The empty cell's faded icon: a backpack of the installed mod
	internal static string PlaceholderPrefab()
	{
		if (s_isBackpacksItem != null)
		{
			return "bp_explorer";
		}
		if (s_advancedPrefabs != null && s_advancedPrefabs.Count > 0 && s_advancedPrefabs[0] is GameObject prefab && prefab != null)
		{
			return prefab.name;
		}
		return null;
	}

	private static bool WornAdvanced(Humanoid humanoid, ItemDrop.ItemData item)
	{
		if (item != null && item.m_equipped && humanoid is Player && Plugin.Active && item != humanoid.m_utilityItem && IsAdvanced(item))
		{
			return humanoid.m_inventory != null && humanoid.m_inventory.ContainsItem(item);
		}
		return false;
	}

	// Put on as its own piece, never as the game's utility item. Late, after Advanced Backpacks' own prefix (one backpack at a
	// time, not from its own rows), which can refuse it
	[HarmonyPatch(typeof(Humanoid), "EquipItem")]
	[HarmonyPrefix]
	[HarmonyPriority(Priority.Low)]
	private static bool EquipBesideBelt(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects, ref bool __result, bool __runOriginal)
	{
		if (!__runOriginal || !Plugin.Active || item == null || !(__instance is Player player) || !IsAdvanced(item))
		{
			return __runOriginal;
		}
		Inventory inventory = ((Humanoid)player).m_inventory;
		if (!inventory.ContainsItem(item))
		{
			__result = false;
			return false;
		}
		if (item.m_equipped)
		{
			((Humanoid)player).SetupEquipment();
			__result = true;
			return false;
		}
		if (__instance.InAttack() || __instance.InDodge() || (!__instance.IsDead() && __instance.IsSwimming() && !__instance.IsOnGround()) || (item.m_shared.m_useDurability && item.m_durability <= 0f))
		{
			__result = false;
			return false;
		}
		if (__instance.m_utilityItem == item)
		{
			__instance.m_utilityItem = null;
		}
		item.m_equipped = true;
		((Humanoid)player).SetupEquipment();
		if (triggerEquipEffects)
		{
			((Humanoid)player).TriggerEquipEffect(item);
		}
		__result = true;
		return false;
	}

	[HarmonyPatch(typeof(Humanoid), "IsItemEquiped")]
	[HarmonyPostfix]
	private static void IsWorn(Humanoid __instance, ItemDrop.ItemData item, ref bool __result)
	{
		if (!__result && WornAdvanced(__instance, item))
		{
			__result = true;
		}
	}

	// Not on the game's own list of worn pieces: taken off with the rest, and on death
	[HarmonyPatch(typeof(Humanoid), "UnequipAllItems")]
	[HarmonyPostfix]
	private static void UnequipAll(Humanoid __instance)
	{
		TakeOff(__instance);
	}

	[HarmonyPatch(typeof(Player), "UnequipDeathDropItems")]
	[HarmonyPostfix]
	private static void UnequipOnDeath(Player __instance)
	{
		TakeOff(__instance);
	}

	private static void TakeOff(Humanoid humanoid)
	{
		if (s_isAdvancedItem == null || !Plugin.Active || !(humanoid is Player) || humanoid.m_inventory == null)
		{
			return;
		}
		Scratch.Clear();
		foreach (ItemDrop.ItemData item in humanoid.m_inventory.GetAllItems())
		{
			if (WornAdvanced(humanoid, item))
			{
				Scratch.Add(item);
			}
		}
		ItemDrop.ItemData[] items = Scratch.ToArray();
		Scratch.Clear();
		foreach (ItemDrop.ItemData item2 in items)
		{
			humanoid.UnequipItem(item2, false);
		}
	}
}
