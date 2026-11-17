using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace BetterEquipSlots.EquipSlots;

/// <summary>
/// Jewelcrafting's rings and necklaces are utility items that it puts on in its own finger and neck slots (when its Ring Slot and
/// Necklace Slot settings are on). Here a ring goes in the first Ring cell and a necklace in the Amulet cell; Jewelcrafting still
/// does the wearing, and they take no place among the utility items. It has one finger and one neck slot, so only one of each.
/// </summary>
internal static class JewelcraftingHost
{
	internal const string Guid = "org.bepinex.plugins.jewelcrafting";

	private const string RingPrefix = "$jc_ring_";

	private const string NecklacePrefix = "$jc_necklace_";

	private static bool s_loaded;

	private static bool s_backgroundErrorLogged;

	private static BepInEx.Configuration.ConfigEntryBase s_ringSlot;

	private static BepInEx.Configuration.ConfigEntryBase s_necklaceSlot;

	internal static bool Loaded => s_loaded;

	// Its own jewelry is worn in the slots, so the panel shows the jewelry cells even with no other jewelry mod
	internal static bool Active => s_loaded && (RingSlotOn || NecklaceSlotOn);

	private static bool RingSlotOn => IsOn(s_ringSlot);

	private static bool NecklaceSlotOn => IsOn(s_necklaceSlot);

	private static bool IsOn(BepInEx.Configuration.ConfigEntryBase entry)
	{
		// Its toggle is an enum, On / Off; without the setting, as with its default, on
		return entry == null || string.Equals(entry.BoxedValue?.ToString(), "On", StringComparison.OrdinalIgnoreCase);
	}

	// After every plugin's Awake, so it is loaded whatever the order
	internal static void Setup()
	{
		if (!Chainloader.PluginInfos.TryGetValue(Guid, out var info) || info.Instance == null)
		{
			return;
		}
		s_loaded = true;
		try
		{
			Type type = info.Instance.GetType();
			s_ringSlot = type.GetField("ringSlot", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as BepInEx.Configuration.ConfigEntryBase;
			s_necklaceSlot = type.GetField("necklaceSlot", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as BepInEx.Configuration.ConfigEntryBase;
			if (s_ringSlot != null)
			{
				s_ringSlot.ConfigFile.SettingChanged += (_, _) => SlotPanel.Invalidate();
			}
			Plugin.Log.LogInfo($"Jewelcrafting {info.Metadata.Version}: its rings go in the Ring slot, its necklaces in the Amulet slot.");
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not read Jewelcrafting's ring and necklace settings (they are taken as on): " + ex.Message);
		}
	}

	/// <summary>
	/// Jewelcrafting colours the cell of every item in the player's inventory by that item's grid position (row * width +
	/// column). The slot items sit far below the grid (from row 20), so the lookup finds no cell and throws on every frame the
	/// inventory is open, which stops the inventory window updating. As with AzuAutoStore, the slot items are taken out of the
	/// list while it colours the cells and put back after, and the slot panel's own grid is left out.
	/// </summary>
	internal static void Guard(Harmony harmony)
	{
		if (harmony == null || !Chainloader.PluginInfos.TryGetValue(Guid, out var info) || info.Instance == null)
		{
			return;
		}
		try
		{
			Assembly assembly = info.Instance.GetType().Assembly;
			MethodInfo postfix = assembly.GetType("Jewelcrafting.SocketsBackground+ColorBackgroundInventory")?.GetMethod("Postfix", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (postfix == null || postfix.GetParameters().Length != 1)
			{
				Plugin.Log.LogWarning($"Jewelcrafting {info.Metadata.Version}: its cell colouring was not found; if it throws, the inventory window stops updating.");
				return;
			}
			harmony.Patch(postfix, new HarmonyMethod(typeof(JewelcraftingHost), nameof(BackgroundsBefore)), null, null, new HarmonyMethod(typeof(JewelcraftingHost), nameof(BackgroundsAfter)), null);
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not guard Jewelcrafting's cell colouring: " + ex.Message);
		}
	}

	// __0 the grid
	private static bool BackgroundsBefore(InventoryGrid __0, out List<KeyValuePair<int, ItemDrop.ItemData>> __state)
	{
		__state = null;
		if (!Plugin.Active || __0 == null || __0.m_inventory == null)
		{
			return true;
		}
		if (SlotPanel.IsPanelGrid(__0))
		{
			return false;
		}
		int height = __0.m_inventory.GetHeight();
		List<ItemDrop.ItemData> items = __0.m_inventory.m_inventory;
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

	// A finalizer, so the items are always put back, and anything it still throws does not stop the window
	private static Exception BackgroundsAfter(Exception __exception, InventoryGrid __0, List<KeyValuePair<int, ItemDrop.ItemData>> __state)
	{
		if (__state != null && __0?.m_inventory != null)
		{
			List<ItemDrop.ItemData> items = __0.m_inventory.m_inventory;
			for (int i = __state.Count - 1; i >= 0; i--)
			{
				items.Insert(Math.Min(__state[i].Key, items.Count), __state[i].Value);
			}
		}
		if (__exception != null && !s_backgroundErrorLogged)
		{
			s_backgroundErrorLogged = true;
			Plugin.Log.LogWarning("Jewelcrafting's cell colouring failed (ignored, so the inventory window keeps updating; logged once): " + __exception);
		}
		return null;
	}

	internal static bool IsRing(ItemDrop.ItemData item)
	{
		return s_loaded && RingSlotOn && StartsWith(item, RingPrefix);
	}

	internal static bool IsNecklace(ItemDrop.ItemData item)
	{
		return s_loaded && NecklaceSlotOn && StartsWith(item, NecklacePrefix);
	}

	// Any of its rings and necklaces (a ring is worn by Jewelcrafting, not as a utility item)
	internal static bool IsWornByIt(ItemDrop.ItemData item)
	{
		return IsRing(item) || IsNecklace(item);
	}

	private static bool StartsWith(ItemDrop.ItemData item, string prefix)
	{
		string name = item?.m_shared?.m_name;
		return name != null && name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
	}

	internal static SlotKind KindOf(ItemDrop.ItemData item)
	{
		if (IsRing(item))
		{
			return SlotKind.Ring;
		}
		return IsNecklace(item) ? SlotKind.Amulet : SlotKind.None;
	}
}
