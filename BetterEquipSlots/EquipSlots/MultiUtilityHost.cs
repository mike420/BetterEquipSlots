using System;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

internal static class MultiUtilityHost
{
	internal const string Guid = "aedenthorn.EquipMultipleUtilityItems";

	private static ConfigEntry<bool> s_enabled;

	private static ConfigEntry<int> s_max;

	internal static int ExtraCells
	{
		get
		{
			if (s_max != null && (s_enabled == null || s_enabled.Value))
			{
				return Mathf.Clamp(s_max.Value - 1, 0, 4);
			}
			return 0;
		}
	}

	internal static bool Active => ExtraCells > 0;

	internal static void Setup()
	{
		if (!Chainloader.PluginInfos.TryGetValue("aedenthorn.EquipMultipleUtilityItems", out var value) || value.Instance == null)
		{
			return;
		}
		try
		{
			Type type = value.Instance.GetType();
			s_enabled = type.GetField("modEnabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as ConfigEntry<bool>;
			s_max = type.GetField("maxEquippedItems", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as ConfigEntry<int>;
			if (s_max == null)
			{
				Plugin.Log.LogWarning($"Equip Multiple Utility Items {value.Metadata.Version}: its MaxEquippedItems setting was not " + "found, so the panel has one belt cell and extra utility items stay worn in the inventory.");
				return;
			}
			s_max.SettingChanged += (object _, EventArgs _) =>
			{
				SlotPanel.Invalidate();
			};
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not read Equip Multiple Utility Items' settings: " + ex.Message);
			s_max = null;
		}
	}
}
