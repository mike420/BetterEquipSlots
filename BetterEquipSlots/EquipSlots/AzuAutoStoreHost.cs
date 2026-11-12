using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace BetterEquipSlots.EquipSlots;

/// <summary>
/// AzuAutoStore draws its favourite borders for every item in the player's inventory, looking each one's cell up by
/// row * width + column. The slot items sit far below the grid (from row 20), so the lookup runs off the end of the
/// cell list and throws on every frame the inventory is open, which stops the inventory window updating: items and
/// extra rows are missing and chests do not open. Like the QuickStackStore guard, the slot items are taken out of the
/// list while the borders are drawn and put back after, and the slot panel's own grid is left out.
/// </summary>
internal static class AzuAutoStoreHost
{
	internal const string Guid = "Azumatt.AzuAutoStore";

	private static bool s_borderErrorLogged;

	internal static void Setup(Harmony harmony)
	{
		if (harmony == null || !Chainloader.PluginInfos.TryGetValue(Guid, out PluginInfo info) || info.Instance == null)
		{
			return;
		}
		try
		{
			Assembly assembly = info.Instance.GetType().Assembly;
			MethodInfo updateGui = assembly.GetType("AzuAutoStore.Patches.Favoriting.BorderRenderer")?.GetMethod("UpdateGui", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (updateGui == null || updateGui.GetParameters().Length != 4)
			{
				Plugin.Log.LogWarning($"AzuAutoStore {info.Metadata.Version}: its border renderer was not found; " + "if its favourite borders throw, the inventory window stops updating.");
				return;
			}
			harmony.Patch(updateGui, new HarmonyMethod(typeof(AzuAutoStoreHost), nameof(BordersBefore)), null, null, new HarmonyMethod(typeof(AzuAutoStoreHost), nameof(BordersAfter)), null);
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not guard AzuAutoStore's favourite borders: " + ex.Message);
		}
	}

	// __0 grid, __1 player, __2 inventory, __3 cells
	private static bool BordersBefore(Player __1, Inventory __2, List<InventoryElement> __3, out List<KeyValuePair<int, ItemDrop.ItemData>> __state)
	{
		__state = null;
		if (!Plugin.Active || __1 == null || __2 == null || __1.m_inventory != __2)
		{
			return true;
		}
		if (SlotPanel.Grid != null && __3 == SlotPanel.Grid.m_elements)
		{
			return false;
		}
		int height = __2.GetHeight();
		List<ItemDrop.ItemData> items = __2.m_inventory;
		for (int i = items.Count - 1; i >= 0; i--)
		{
			if (items[i] != null && items[i].m_gridPos.y >= height)
			{
				(__state ??= new List<KeyValuePair<int, ItemDrop.ItemData>>()).Add(new KeyValuePair<int, ItemDrop.ItemData>(i, items[i]));
				items.RemoveAt(i);
			}
		}
		return true;
	}

	// A finalizer, so the items are always put back, and anything AzuAutoStore still throws does not stop the window
	private static Exception BordersAfter(Exception __exception, Inventory __2, List<KeyValuePair<int, ItemDrop.ItemData>> __state)
	{
		if (__state != null && __2 != null)
		{
			List<ItemDrop.ItemData> items = __2.m_inventory;
			for (int i = __state.Count - 1; i >= 0; i--)
			{
				items.Insert(Math.Min(__state[i].Key, items.Count), __state[i].Value);
			}
		}
		if (__exception != null && !s_borderErrorLogged)
		{
			s_borderErrorLogged = true;
			Plugin.Log.LogWarning("AzuAutoStore's favourite borders failed (ignored, so the inventory window keeps updating; logged once): " + __exception);
		}
		return null;
	}
}
