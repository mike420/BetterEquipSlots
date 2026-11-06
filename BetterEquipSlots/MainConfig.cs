using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using Jotunn.Extensions;
using UnityEngine;

namespace BetterEquipSlots;

internal static class MainConfig
{
	public static ConfigEntry<bool> modEnabled;

	public static ConfigEntry<KeyboardShortcut> quick1;
	public static ConfigEntry<KeyboardShortcut> quick2;
	public static ConfigEntry<KeyboardShortcut> quick3;
	public static ConfigEntry<KeyboardShortcut> quick4;
	public static ConfigEntry<KeyboardShortcut> quick5;
	public static ConfigEntry<KeyboardShortcut> quick6;
	public static ConfigEntry<KeyboardShortcut> altModifier;
	public static ConfigEntry<bool> keysWinOverVanilla;
	public static ConfigEntry<bool> showKeyConflicts;
	public static ConfigEntry<bool> gamepadHotbar;

	public static ConfigEntry<bool> hudEnabled;
	public static ConfigEntry<Vector2> hudOffset;
	public static ConfigEntry<float> hudSpacing;

	public static ConfigEntry<bool> ammoOnHotbar;
	public static ConfigEntry<bool> autoEquipAmmo;

	public static ConfigEntry<bool> cellLockIcons;

	public static ConfigEntry<Vector2> panelOffset;
	public static ConfigEntry<bool> panelCharacter;
	public static ConfigEntry<bool> keepWornInInventory;
	public static ConfigEntry<bool> showOnKey;
	public static ConfigEntry<KeyboardShortcut> panelKey;

	public static ConfigEntry<bool> multiUtility;
	public static ConfigEntry<int> maxUtilityItems;

	public static ConfigEntry<string> rings;
	public static ConfigEntry<string> earrings;
	public static ConfigEntry<string> amulets;

	private const string KeyHelp = " Any key, or a key with modifiers such as \"O + LeftAlt\"; empty: no key.";

	public static void InitConfigs(ConfigFile config)
	{
		// (Read before binding: once bound, the file exists)
		bool newFile = !File.Exists(config.ConfigFilePath);
		bool newUtility = newFile || !File.ReadAllText(config.ConfigFilePath).Contains("[Utility]");

		modEnabled = config.BindConfigInOrder("General Settings", "Enable Plugin", defaultValue: true, "Enable plugin: the equipment panel, quick slots and the rest. Off: the game as without the mod.\n(Restart required. It is read before connecting to a server, so it is a local setting).\n", synced: false, sectionOrder: true, settingOrder: true);

		quick1 = config.BindConfigInOrder("Quick Slots", "Key 1", defaultValue: new KeyboardShortcut(KeyCode.Z), "Uses quick slot 1." + KeyHelp, synced: false, sectionOrder: true, settingOrder: true);
		quick2 = config.BindConfigInOrder("Quick Slots", "Key 2", defaultValue: new KeyboardShortcut(KeyCode.V), "Uses quick slot 2." + KeyHelp, synced: false, sectionOrder: true, settingOrder: true);
		quick3 = config.BindConfigInOrder("Quick Slots", "Key 3", defaultValue: new KeyboardShortcut(KeyCode.B), "Uses quick slot 3." + KeyHelp, synced: false, sectionOrder: true, settingOrder: true);
		quick4 = config.BindConfigInOrder("Quick Slots", "Key 4", defaultValue: new KeyboardShortcut(KeyCode.Z, KeyCode.LeftAlt), "Uses quick slot 4." + KeyHelp, synced: false, sectionOrder: true, settingOrder: true);
		quick5 = config.BindConfigInOrder("Quick Slots", "Key 5", defaultValue: new KeyboardShortcut(KeyCode.V, KeyCode.LeftAlt), "Uses quick slot 5." + KeyHelp, synced: false, sectionOrder: true, settingOrder: true);
		quick6 = config.BindConfigInOrder("Quick Slots", "Key 6", defaultValue: new KeyboardShortcut(KeyCode.B, KeyCode.LeftAlt), "Uses quick slot 6." + KeyHelp, synced: false, sectionOrder: true, settingOrder: true);
		altModifier = config.BindConfigInOrder("Quick Slots", "Alt Modifier", defaultValue: new KeyboardShortcut(KeyCode.LeftAlt), "While it is held the HUD shows quick slots 4-6 instead of 1-3. Empty or None: always 1-3.", synced: false, sectionOrder: true, settingOrder: true);
		keysWinOverVanilla = config.BindConfigInOrder("Quick Slots", "Keys Win Over Vanilla", defaultValue: false, "A quick slot key without modifiers that is also bound to a game action (vanilla: V auto-pickup, 1-8 the hotbar): off - the key does both, and the clash is shown under the hotbar; on - it does only the quick slot. A key with modifiers (Alt+1) never also does the game action.", synced: false, sectionOrder: true, settingOrder: true);
		showKeyConflicts = config.BindConfigInOrder("Quick Slots", "Show Key Conflicts", defaultValue: true, "Show a note under the hotbar (while the inventory is closed) when a quick slot key or the equipment panel key is also used by something else.", synced: false, sectionOrder: true, settingOrder: true);
		gamepadHotbar = config.BindConfigInOrder("Quick Slots", "Gamepad Hotbar", defaultValue: true, "With a controller: the hotbar's left / right buttons step on past slot 8 onto the six quick slots, and its use button uses the one selected there.", synced: false, sectionOrder: true, settingOrder: true);

		hudEnabled = config.BindConfigInOrder("HUD", "Show Quick Slots", defaultValue: true, "Show the quick slots next to the forsaken power.", synced: false, sectionOrder: true, settingOrder: true);
		hudOffset = config.BindConfigInOrder("HUD", "Offset", defaultValue: Vector2.zero, "Moves the HUD quick slots from their place right of the forsaken power (pixels, x right, y up).", synced: false, sectionOrder: true, settingOrder: true);
		hudSpacing = config.BindConfigInOrder("HUD", "Spacing", defaultValue: 8f, "Gap between the HUD quick slots.", synced: false, sectionOrder: true, settingOrder: true, acceptableValues: new AcceptableValueRange<float>(0f, 64f));

		ammoOnHotbar = config.BindConfigInOrder("Ammo", "Show On Hotbar", defaultValue: true, "Above a bow, crossbow or fishing rod on the hotbar (1-8): the ammo it shoots next and how many of it are left.", synced: false, sectionOrder: true, settingOrder: true);
		autoEquipAmmo = config.BindConfigInOrder("Ammo", "Auto Equip", defaultValue: true, "When the worn arrows (bolts, bait) run out, put on the next of the same kind, else any other that fits the weapon.", synced: false, sectionOrder: true, settingOrder: true);

		cellLockIcons = config.BindConfigInOrder("Inventory", "Cell Locks", defaultValue: true, "Show a padlock in the bottom left corner of every inventory cell. Click it to lock the cell: its item is then never moved into a chest by a button that empties the inventory - vanilla's \"stack similar\" and QuickStackStore's quick stack, store and sort.", synced: false, sectionOrder: true, settingOrder: true);

		panelOffset = config.BindConfigInOrder("Panel", "Offset", defaultValue: Vector2.zero, "Moves the equipment panel from its place between the inventory and the crafting panel (pixels, x right, y up). Dragging the panel by its frame sets it too.", synced: false, sectionOrder: true, settingOrder: true);
		panelCharacter = config.BindConfigInOrder("Panel", "Show Character", defaultValue: true, "Draw your character, as it is dressed right now, behind the equipment slots.", synced: false, sectionOrder: true, settingOrder: true);
		keepWornInInventory = config.BindConfigInOrder("Panel", "Keep Worn In Inventory", defaultValue: false, "Where worn armour, cape, belt, trinket and arrows go.\nOff (default): a worn item moves out of the inventory into its slot on the panel, freeing its inventory cell.\nOn: a worn item stays in its inventory cell, as in the game without this mod, and the panel only shows it in its slot as well - one item in two places, still taking up the inventory cell.\nTo have worn items leave your inventory, keep this off.", synced: false, sectionOrder: true, settingOrder: true);
		showOnKey = config.BindConfigInOrder("Panel", "Show On Key", defaultValue: false, "The equipment panel shows only after pressing Key: in the open inventory it shows / hides the panel, with the inventory closed it opens the inventory with the panel. Off: the panel shows whenever the inventory is open.", synced: false, sectionOrder: true, settingOrder: true);
		panelKey = config.BindConfigInOrder("Panel", "Key", defaultValue: new KeyboardShortcut(KeyCode.U), "The key of the equipment panel when Show On Key is on. Any key, or a key with modifiers such as \"E + LeftAlt\".", synced: false, sectionOrder: true, settingOrder: true);

		multiUtility = config.BindConfigInOrder("Utility", "Multiple Utility Items", defaultValue: true, "Wear several utility items (belts and the like) at once, their stats and effects all counted. The panel shows a window with a cell for each next to the belt slot. Off: one utility item, as in the game, and no window.", synced: true, sectionOrder: true, settingOrder: true);
		maxUtilityItems = config.BindConfigInOrder("Utility", "Max Utility Items", defaultValue: 5, "How many utility items can be worn at once with Multiple Utility Items on. Never two of the same kind.", synced: true, sectionOrder: true, settingOrder: true, acceptableValues: new AcceptableValueRange<int>(2, 5));

		rings = config.BindConfigInOrder("Jewelry", "Rings", defaultValue: "", "Item prefab names worn in the ring slots, comma-separated.", synced: true, sectionOrder: true, settingOrder: true);
		earrings = config.BindConfigInOrder("Jewelry", "Earrings", defaultValue: "", "Item prefab names worn in the earring slots, comma-separated.", synced: true, sectionOrder: true, settingOrder: true);
		amulets = config.BindConfigInOrder("Jewelry", "Amulets", defaultValue: "", "Item prefab names worn in the amulet slot, comma-separated.", synced: true, sectionOrder: true, settingOrder: true);

		if (newFile)
		{
			ImportCdevConfig(config);
		}
		if (newUtility)
		{
			ImportMultiUtilityConfig(config);
		}
	}

	// CDEV.EquipmentSlots' settings ([section] and key in its file): the setting each became here
	private static Dictionary<(string, string), ConfigEntryBase> CdevNames() => new Dictionary<(string, string), ConfigEntryBase>
	{
		[("QuickSlots", "Key1")] = quick1,
		[("QuickSlots", "Key2")] = quick2,
		[("QuickSlots", "Key3")] = quick3,
		[("QuickSlots", "Key4")] = quick4,
		[("QuickSlots", "Key5")] = quick5,
		[("QuickSlots", "Key6")] = quick6,
		[("QuickSlots", "AltModifier")] = altModifier,
		[("QuickSlots", "KeysWinOverVanilla")] = keysWinOverVanilla,
		[("QuickSlots", "ShowKeyConflicts")] = showKeyConflicts,
		[("QuickSlots", "GamepadHotbar")] = gamepadHotbar,
		[("Hud", "Enabled")] = hudEnabled,
		[("Hud", "Offset")] = hudOffset,
		[("Hud", "Spacing")] = hudSpacing,
		[("Ammo", "ShowOnHotbar")] = ammoOnHotbar,
		[("Ammo", "AutoEquip")] = autoEquipAmmo,
		[("Inventory", "CellLocks")] = cellLockIcons,
		[("Panel", "Offset")] = panelOffset,
		[("Panel", "ShowCharacter")] = panelCharacter,
		[("Panel", "KeepWornInInventory")] = keepWornInInventory,
		[("Panel", "ShowOnKey")] = showOnKey,
		[("Panel", "Key")] = panelKey,
		[("Jewelry", "Rings")] = rings,
		[("Jewelry", "Earrings")] = earrings,
		[("Jewelry", "Amulets")] = amulets,
	};

	/// <summary>
	/// The first time the Utility settings are made, takes Equip Multiple Utility Items' (now built in) from its file, if
	/// there is one: Enabled and MaxEquippedItems.
	/// </summary>
	private static void ImportMultiUtilityConfig(ConfigFile config)
	{
		string path = Path.Combine(Paths.ConfigPath, "aedenthorn.EquipMultipleUtilityItems.cfg");
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			foreach (string raw in File.ReadAllLines(path))
			{
				string line = raw.Trim();
				int equals = line.IndexOf('=');
				if (line.StartsWith("#") || equals <= 0)
				{
					continue;
				}
				string key = line.Substring(0, equals).Trim();
				string value = line.Substring(equals + 1).Trim();
				if (key == "Enabled")
				{
					multiUtility.SetSerializedValue(value);
				}
				else if (key == "MaxEquippedItems")
				{
					maxUtilityItems.SetSerializedValue(value);
				}
			}
			config.Save();
			Jotunn.Logger.LogInfo("[BetterEquipSlots] Took the Utility settings from aedenthorn.EquipMultipleUtilityItems.cfg.");
		}
		catch (Exception e)
		{
			Jotunn.Logger.LogWarning("[BetterEquipSlots] Could not read aedenthorn.EquipMultipleUtilityItems.cfg: " + e.Message);
		}
	}

	/// <summary>
	/// The first time the mod runs (no config file yet), takes the settings from CDEV.EquipmentSlots' file
	/// (cdev.equipmentslots.cfg), if there is one, so a player moving over keeps their keys and layout.
	/// </summary>
	private static void ImportCdevConfig(ConfigFile config)
	{
		string path = Path.Combine(Paths.ConfigPath, "cdev.equipmentslots.cfg");
		if (!File.Exists(path))
		{
			return;
		}
		try
		{
			Dictionary<(string, string), ConfigEntryBase> names = CdevNames();
			string section = "";
			int taken = 0;
			foreach (string raw in File.ReadAllLines(path))
			{
				string line = raw.Trim();
				if (line.Length == 0 || line.StartsWith("#"))
				{
					continue;
				}
				if (line.StartsWith("[") && line.EndsWith("]"))
				{
					section = line.Substring(1, line.Length - 2).Trim();
					continue;
				}
				int equals = line.IndexOf('=');
				if (equals <= 0 || !names.TryGetValue((section, line.Substring(0, equals).Trim()), out ConfigEntryBase entry))
				{
					continue;
				}
				try
				{
					entry.SetSerializedValue(line.Substring(equals + 1).Trim());
					taken++;
				}
				catch (Exception e)
				{
					Jotunn.Logger.LogWarning("[BetterEquipSlots] Could not take " + section + " / " + line.Substring(0, equals).Trim() + " from cdev.equipmentslots.cfg: " + e.Message);
				}
			}
			config.Save();
			Jotunn.Logger.LogInfo("[BetterEquipSlots] Took " + taken + " setting(s) from cdev.equipmentslots.cfg.");
		}
		catch (Exception e)
		{
			Jotunn.Logger.LogWarning("[BetterEquipSlots] Could not read cdev.equipmentslots.cfg: " + e.Message);
		}
	}
}
