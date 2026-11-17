using System;
using System.Reflection;
using BepInEx.Bootstrap;

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
