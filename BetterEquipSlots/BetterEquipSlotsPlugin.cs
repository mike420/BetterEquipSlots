using System;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BetterEquipSlots.EquipSlots;
using HarmonyLib;
using Jotunn.Managers;
using Jotunn.Utils;

namespace BetterEquipSlots;

[BepInPlugin(PluginGUID, PluginName, PluginVersion)]
[BepInDependency("com.jotunn.jotunn", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("ishid4.mods.betterarchery", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("goldenrevolver.quick_stack_store", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("Azumatt.AzuAutoStore", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("LuminairPrime.valheim.localcraftfromstorageprime", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("zenox.betterui", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("randyknapp.mods.epicloot", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("cdev.sleepingplayers", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("MagicMike.EinherjarBodies", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("org.bepinex.plugins.jewelcrafting", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("nick008.extrafood", BepInDependency.DependencyFlags.SoftDependency)]
[NetworkCompatibility(CompatibilityLevel.ClientMustHaveMod, VersionStrictness.Minor)]
public class BetterEquipSlotsPlugin : BaseUnityPlugin
{
	public const string PluginName = "BetterEquipSlots";
	public const string PluginVersion = "1.1.1";
	public const string PluginAuthor = "MagicMike";
	internal const string PluginGUID = "MagicMike.BetterEquipSlots";
	internal const string PluginCopy = "Copyright © MagicMike 2026";
	public const string PluginLink = "https://discord.gg/mnr36G6J4x";

	public static BetterEquipSlotsPlugin plugininstance;

	internal static ConfigFile ConfigFile;
	internal static ConfigFileWatcher ConfigFileWatcher;

	private Harmony harmony;

	public void Awake()
	{
		plugininstance = this;
		Plugin.Log = Logger;
		ConfigFile = Config;
		ConfigFileWatcher = new ConfigFileWatcher(Config, 1000L);
		Config.SaveOnConfigSet = false;
		MainConfig.InitConfigs(Config);

		MagicMike.ModSettings.ModSettings.Init(this);

		UpdatePlugin();

		if (!MainConfig.modEnabled.Value)
		{
			return;
		}
		if (Chainloader.PluginInfos.ContainsKey(Plugin.EaqsGuid))
		{
			Logger.LogError("EquipmentAndQuickSlots is installed: BetterEquipSlots replaces it and stays switched off. Remove EquipmentAndQuickSlots; its slot items are converted on the next load.");
			return;
		}
		if (Chainloader.PluginInfos.ContainsKey(Plugin.CdevGuid))
		{
			Logger.LogError("CDEV.EquipmentSlots is installed: BetterEquipSlots is a port of it and stays switched off. Remove CDEV.EquipmentSlots; its slots, styles and locks carry over as they are.");
			return;
		}

		Translations.Register();
		Localization.OnLanguageChange = (Action)Delegate.Combine(Localization.OnLanguageChange, new Action(SlotPanel.Invalidate));
		Plugin.Active = true;
		BetterArcheryHost.Setup();
		MultiUtility.Setup();
		harmony = new Harmony(PluginGUID);
		harmony.PatchAll(Assembly.GetExecutingAssembly());
		QuickStackStoreHost.Setup(harmony);
		AzuAutoStoreHost.Setup(harmony);
		JewelcraftingHost.Guard(harmony);
		LocalCraftFromStorageHost.Setup(harmony);
		BetterUiHost.Setup();
		EpicLootHost.Setup();

		// Keys: the panel's labels and the quick slots are redrawn
		foreach (ConfigEntry<KeyboardShortcut> key in new[] { MainConfig.quick1, MainConfig.quick2, MainConfig.quick3, MainConfig.quick4, MainConfig.quick5, MainConfig.quick6, MainConfig.altModifier })
		{
			key.SettingChanged += (_, _) =>
			{
				SlotPanel.Invalidate();
				QuickSlots.Invalidate();
			};
		}
		MainConfig.keepWornInInventory.SettingChanged += (_, _) => EquipSync.MarkDirty();
		SlotVisibility.Setup();
		FoodBag.Setup();
		MainConfig.rings.SettingChanged += (_, _) => Jewelry.ReloadConfig();
		MainConfig.earrings.SettingChanged += (_, _) => Jewelry.ReloadConfig();
		MainConfig.amulets.SettingChanged += (_, _) => Jewelry.ReloadConfig();
		Jewelry.ReloadConfig();

		ConfigFileWatcher.OnConfigFileReloaded += delegate
		{
			UpdatePlugin(saveConfig: false);
		};
		SynchronizationManager.OnConfigurationWindowClosed += delegate
		{
			UpdatePlugin();
		};

		Game.isModded = true;
		StartLate();
	}

	// Every plugin's Awake has run by now
	private void Start()
	{
		if (Plugin.Active)
		{
			MultiUtility.SwitchOffOldMod();
			BackpackHost.Setup();
			JewelcraftingHost.Setup();
		}
	}

	/// <summary>When the plugin is (re)loaded with a world already running, the slots are set up on what is already there.</summary>
	private static void StartLate()
	{
		if (Player.m_localPlayer != null)
		{
			SlotInventory.Register(Player.m_localPlayer.m_inventory);
		}
		if (Hud.instance != null)
		{
			QuickBar.Create(Hud.instance);
		}
		if (InventoryGui.instance != null)
		{
			SlotPanel.Create(InventoryGui.instance);
		}
	}

	private void Update()
	{
		if (Plugin.Active)
		{
			QuickSlots.Tick();
			Loadouts.Tick();
			FoodBag.Tick();
			SlotPanel.TickKey();
			AmmoRefill.Tick();
			QuickRefill.Tick();
			Durability.Tick();
			EquipSync.Tick();
			QuickBar.Tick();
			PlayerPreview.Tick();
		}
	}

	private void LateUpdate()
	{
		InventoryGui instance = InventoryGui.instance;
		if (Plugin.Active && instance != null && InventoryGui.IsVisible() && Player.m_localPlayer != null && SlotPanel.ApplyShown())
		{
			SlotPanel.Refresh(instance, Player.m_localPlayer);
		}
	}

	private void OnDestroy()
	{
		if (!Plugin.Active)
		{
			return;
		}
		Localization.OnLanguageChange = (Action)Delegate.Remove(Localization.OnLanguageChange, new Action(SlotPanel.Invalidate));
		harmony?.UnpatchSelf();
		SlotPanel.Destroy();
		QuickBar.Destroy();
		HotbarAmmo.Destroy();
		PlayerPreview.Destroy();
	}

	private void UpdatePlugin(bool saveConfig = true)
	{
		if (saveConfig)
		{
			base.Config.Save();
		}
	}
}
