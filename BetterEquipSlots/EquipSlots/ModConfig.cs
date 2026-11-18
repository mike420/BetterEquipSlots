using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

/// <summary>
/// What the slots' code reads from the plugin's settings (bound in <see cref="MainConfig"/>), under the names it had in
/// CDEV.EquipmentSlots.
/// </summary>
internal static class ModConfig
{
	internal static ConfigEntry<KeyboardShortcut> Quick1 => MainConfig.quick1;
	internal static ConfigEntry<KeyboardShortcut> Quick2 => MainConfig.quick2;
	internal static ConfigEntry<KeyboardShortcut> Quick3 => MainConfig.quick3;
	internal static ConfigEntry<KeyboardShortcut> Quick4 => MainConfig.quick4;
	internal static ConfigEntry<KeyboardShortcut> Quick5 => MainConfig.quick5;
	internal static ConfigEntry<KeyboardShortcut> Quick6 => MainConfig.quick6;
	internal static ConfigEntry<KeyboardShortcut> AltModifier => MainConfig.altModifier;
	internal static ConfigEntry<bool> KeysWinOverVanilla => MainConfig.keysWinOverVanilla;
	internal static ConfigEntry<bool> GamepadHotbar => MainConfig.gamepadHotbar;
	internal static ConfigEntry<bool> HudEnabled => MainConfig.hudEnabled;
	internal static ConfigEntry<Vector2> HudOffset => MainConfig.hudOffset;
	internal static ConfigEntry<float> HudSpacing => MainConfig.hudSpacing;
	internal static ConfigEntry<bool> CellLockIcons => MainConfig.cellLockIcons;
	internal static ConfigEntry<bool> AmmoOnHotbar => MainConfig.ammoOnHotbar;
	internal static ConfigEntry<bool> AutoEquipAmmo => MainConfig.autoEquipAmmo;
	internal static ConfigEntry<Vector2> PanelOffset => MainConfig.panelOffset;
	internal static ConfigEntry<bool> PanelCharacter => MainConfig.panelCharacter;
	internal static ConfigEntry<bool> KeepWornInInventory => MainConfig.keepWornInInventory;
	internal static ConfigEntry<bool> ShowOnKey => MainConfig.showOnKey;
	internal static ConfigEntry<KeyboardShortcut> PanelKey => MainConfig.panelKey;
	internal static ConfigEntry<string> Rings => MainConfig.rings;
	internal static ConfigEntry<string> Earrings => MainConfig.earrings;
	internal static ConfigEntry<string> Amulets => MainConfig.amulets;

	internal static KeyboardShortcut SlotKey(int ordinal)
	{
		return ordinal switch
		{
			0 => Quick1.Value,
			1 => Quick2.Value,
			2 => Quick3.Value,
			3 => Quick4.Value,
			4 => Quick5.Value,
			5 => Quick6.Value,
			_ => KeyboardShortcut.Empty,
		};
	}

	internal static KeyCode[] AltKeys()
	{
		List<KeyCode> list = new List<KeyCode>();
		KeyboardShortcut value = AltModifier.Value;
		if (value.MainKey != KeyCode.None)
		{
			list.Add(value.MainKey);
			list.AddRange(value.Modifiers);
		}
		return list.ToArray();
	}
}
