using System;
using System.Collections.Generic;

namespace BetterEquipSlots.EquipSlots;

internal enum PanelPreset
{
	Custom,
	Full,
	Compact,
	Minimal
}

/// <summary>
/// Which slots of the panel are left out when they are empty (Panel → Hidden Slots), and the presets that fill that in. A slot that
/// holds an item is always shown. Jewelcrafting alone has one ring and one necklace slot, so the rest of the jewelry slots are
/// left out then.
/// </summary>
internal static class SlotVisibility
{
	private static readonly HashSet<SlotKind> Hidden = new HashSet<SlotKind>();

	private static bool s_applying;

	internal static void Setup()
	{
		Reload();
		MainConfig.hiddenSlots.SettingChanged += (_, _) =>
		{
			Reload();
			SlotPanel.Invalidate();
		};
		MainConfig.panelPreset.SettingChanged += (_, _) => ApplyPreset();
	}

	private static void Reload()
	{
		Hidden.Clear();
		foreach (string word in (MainConfig.hiddenSlots.Value ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
		{
			string name = word.Trim();
			if (name.Equals("Necklace", StringComparison.OrdinalIgnoreCase))
			{
				name = "Amulet";
			}
			else if (name.Equals("Cloak", StringComparison.OrdinalIgnoreCase))
			{
				name = "Cape";
			}
			if (Enum.TryParse(name, ignoreCase: true, out SlotKind kind) && SlotLayout.IsEquipmentKind(kind))
			{
				Hidden.Add(kind);
			}
		}
	}

	// A preset is a button: it sets the settings it stands for, then goes back to Custom
	private static void ApplyPreset()
	{
		if (s_applying || MainConfig.panelPreset.Value == PanelPreset.Custom)
		{
			return;
		}
		s_applying = true;
		try
		{
			switch (MainConfig.panelPreset.Value)
			{
			case PanelPreset.Full:
				MainConfig.hiddenSlots.Value = "";
				MainConfig.panelCharacter.Value = true;
				break;
			case PanelPreset.Compact:
				MainConfig.hiddenSlots.Value = "Trinket, Ammo, Backpack, Earring, Ring, Amulet";
				MainConfig.panelCharacter.Value = true;
				break;
			case PanelPreset.Minimal:
				MainConfig.hiddenSlots.Value = "Cape, Belt, Trinket, Ammo, Backpack, Earring, Ring, Amulet";
				MainConfig.panelCharacter.Value = false;
				break;
			}
			MainConfig.panelPreset.Value = PanelPreset.Custom;
		}
		finally
		{
			s_applying = false;
		}
		Reload();
		SlotPanel.Invalidate();
	}

	internal static bool IsHidden(SlotKind kind, int ordinal, Inventory inventory)
	{
		SlotCell cell = null;
		foreach (SlotCell candidate in SlotLayout.Equipment)
		{
			if (candidate.Kind == kind && candidate.Ordinal == ordinal)
			{
				cell = candidate;
				break;
			}
		}
		if (cell != null && inventory != null && SlotInventory.ItemIn(inventory, cell) != null)
		{
			return false;
		}
		if (Hidden.Contains(kind))
		{
			return true;
		}
		// Jewelcrafting alone: one finger and one neck slot
		if (JewelcraftingHost.Active && !Jewelry.AnyKnown)
		{
			return kind == SlotKind.Earring || (kind == SlotKind.Ring && ordinal > 0);
		}
		return false;
	}

	internal static (SlotKind, int)[] Filter((SlotKind, int)[] column, Inventory inventory)
	{
		List<(SlotKind, int)> kept = new List<(SlotKind, int)>();
		foreach ((SlotKind, int) entry in column)
		{
			if (!IsHidden(entry.Item1, entry.Item2, inventory))
			{
				kept.Add(entry);
			}
		}
		return kept.ToArray();
	}
}
