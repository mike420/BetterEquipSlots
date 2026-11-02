using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace BetterEquipSlots.EquipSlots;

internal static class QuickStackStoreHost
{
	internal const string Guid = "goldenrevolver.quick_stack_store";

	private static bool s_borderErrorLogged;

	internal static bool Hooked { get; private set; }

	internal static void Setup(Harmony harmony)
	{
		if (harmony != null && Chainloader.PluginInfos.TryGetValue("goldenrevolver.quick_stack_store", out var value) && !(value.Instance == null))
		{
			Assembly assembly = value.Instance.GetType().Assembly;
			HookSlotChecks(harmony, assembly, value);
			HookBorders(harmony, assembly, value);
		}
	}

	private static void HookSlotChecks(Harmony harmony, Assembly assembly, PluginInfo info)
	{
		try
		{
			Type type = assembly.GetType("QuickStackStore.CompatibilitySupport");
			Type[] types = new Type[3]
			{
				typeof(int),
				typeof(int),
				typeof(Vector2i)
			};
			MethodInfo methodInfo = type?.GetMethod("IsEquipOrQuickSlot", BindingFlags.Static | BindingFlags.Public, null, types, null);
			MethodInfo methodInfo2 = type?.GetMethod("IsEquipSlot", BindingFlags.Static | BindingFlags.Public, null, types, null);
			if (methodInfo != null)
			{
				harmony.Patch(methodInfo, null, new HarmonyMethod(typeof(QuickStackStoreHost), "LeaveSlotsAlone"));
			}
			if (methodInfo2 != null)
			{
				harmony.Patch(methodInfo2, null, new HarmonyMethod(typeof(QuickStackStoreHost), "LeaveSlotsAloneButRestock"));
			}
			if (methodInfo == null || methodInfo2 == null)
			{
				Plugin.Log.LogWarning($"QuickStackStore {info.Metadata.Version}: its IsEquipOrQuickSlot / IsEquipSlot check " + "was not found, so its sort, store, quick stack or restock may move items out of the slots.");
			}
			else
			{
				Hooked = true;
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not teach QuickStackStore about the slots: " + ex.Message);
		}
	}

	private static void HookBorders(Harmony harmony, Assembly assembly, PluginInfo info)
	{
		try
		{
			MethodInfo methodInfo = assembly.GetType("QuickStackStore.BorderRenderer")?.GetMethod("UpdateGui", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[3]
			{
				typeof(Player),
				typeof(Inventory),
				typeof(List<InventoryElement>)
			}, null);
			if (methodInfo == null)
			{
				Plugin.Log.LogWarning($"QuickStackStore {info.Metadata.Version}: its border renderer was not found; " + "if its favourite borders throw, the inventory window stops updating.");
			}
			else
			{
				harmony.Patch(methodInfo, new HarmonyMethod(typeof(QuickStackStoreHost), "BordersBefore"), null, null, new HarmonyMethod(typeof(QuickStackStoreHost), "BordersAfter"), null);
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not guard QuickStackStore's favourite borders: " + ex.Message);
		}
	}

	private static bool IsSlotRow(Vector2i pos)
	{
		if (pos.y >= 20)
		{
			return pos.y <= 31;
		}
		return false;
	}

	private static void LeaveSlotsAlone(Vector2i itemPos, ref bool __result)
	{
		if (!__result && Plugin.Active && (IsSlotRow(itemPos) || Locks.IsLocked(itemPos)))
		{
			__result = true;
		}
	}

	private static void LeaveSlotsAloneButRestock(Vector2i itemPos, ref bool __result)
	{
		if (!__result && Plugin.Active)
		{
			bool flag = itemPos.y == 22 && itemPos.x >= 0 && itemPos.x < 6;
			if ((IsSlotRow(itemPos) && !flag) || Locks.IsLocked(itemPos))
			{
				__result = true;
			}
		}
	}

	private static bool BordersBefore(Player __0, Inventory __1, List<InventoryElement> __2, out List<KeyValuePair<int, ItemDrop.ItemData>> __state)
	{
		__state = null;
		if (!Plugin.Active || __0 == null || __1 == null || ((Humanoid)__0).m_inventory != __1)
		{
			return true;
		}
		if ((object)SlotPanel.Grid != null && __2 == SlotPanel.Grid.m_elements)
		{
			return false;
		}
		int height = __1.GetHeight();
		List<ItemDrop.ItemData> inventory = __1.m_inventory;
		for (int num = inventory.Count - 1; num >= 0; num--)
		{
			if (inventory[num] != null && inventory[num].m_gridPos.y >= height)
			{
				(__state ?? (__state = new List<KeyValuePair<int, ItemDrop.ItemData>>())).Add(new KeyValuePair<int, ItemDrop.ItemData>(num, inventory[num]));
				inventory.RemoveAt(num);
			}
		}
		return true;
	}

	private static Exception BordersAfter(Exception __exception, Inventory __1, List<KeyValuePair<int, ItemDrop.ItemData>> __state)
	{
		if (__state != null && __1 != null)
		{
			List<ItemDrop.ItemData> inventory = __1.m_inventory;
			for (int num = __state.Count - 1; num >= 0; num--)
			{
				inventory.Insert(Math.Min(__state[num].Key, inventory.Count), __state[num].Value);
			}
		}
		if (__exception != null && !s_borderErrorLogged)
		{
			s_borderErrorLogged = true;
			Plugin.Log.LogWarning("QuickStackStore's favourite borders failed (ignored, so the inventory window keeps updating; logged once): " + __exception);
		}
		return null;
	}
}
