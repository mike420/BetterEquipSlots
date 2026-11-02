using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Configuration;

namespace BetterEquipSlots.EquipSlots;

internal static class BetterArcheryHost
{
	internal const string Guid = "ishid4.mods.betterarchery";

	private static FieldInfo s_rowIndex;

	private static Func<bool> s_isQuiverEquipped;

	private static Func<ItemDrop.ItemData, string> s_customSlotName;

	private static ConfigEntry<bool> s_enabled;

	private static ConfigEntry<bool> s_hudEnabled;

	private static readonly Dictionary<ItemDrop.ItemData.SharedData, bool> Quivers = new Dictionary<ItemDrop.ItemData.SharedData, bool>();

	internal static bool Hosted { get; private set; }

	internal static bool QuiverActive
	{
		get
		{
			if (Hosted && s_enabled != null)
			{
				return s_enabled.Value;
			}
			return false;
		}
	}

	internal static bool QuiverHudShown
	{
		get
		{
			if (QuiverActive && s_hudEnabled != null && s_hudEnabled.Value)
			{
				return IsQuiverEquipped();
			}
			return false;
		}
	}

	internal static void Setup()
	{
		if (!Chainloader.PluginInfos.TryGetValue("ishid4.mods.betterarchery", out var value) || value.Instance == null)
		{
			return;
		}
		try
		{
			Type type = value.Instance.GetType();
			FieldInfo field = type.GetField("eaqsPresent", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			s_rowIndex = type.GetField("QuiverRowIndex", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			MethodInfo method = type.GetMethod("IsQuiverEquipped", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
			s_enabled = type.GetField("ConfigQuiverEnabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as ConfigEntry<bool>;
			s_hudEnabled = type.GetField("QuiverHudEnabled", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as ConfigEntry<bool>;
			if (field == null || field.FieldType != typeof(int) || s_rowIndex == null || s_rowIndex.FieldType != typeof(int) || method == null || s_enabled == null)
			{
				Plugin.Log.LogWarning($"Better Archery {value.Metadata.Version}: its quiver members were not found; the quiver is not hosted " + "(written for 2.0.0). Keep Better Archery's 'Enable Quiver' off, or arrows in the quiver may be lost.");
				return;
			}
			MethodInfo methodInfo = type.Assembly.GetType("Common.CustomSlotCreator")?.GetMethod("GetCustomSlotName", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(ItemDrop.ItemData) }, null);
			s_customSlotName = ((methodInfo != null && methodInfo.ReturnType == typeof(string)) ? ((Func<ItemDrop.ItemData, string>)Delegate.CreateDelegate(typeof(Func<ItemDrop.ItemData, string>), methodInfo)) : null);
			if (s_customSlotName == null)
			{
				Plugin.Log.LogWarning("Better Archery: Common.CustomSlotCreator.GetCustomSlotName was not found; the ammo slot cannot tell its quiver from arrows.");
			}
			field.SetValue(null, 1);
			s_rowIndex.SetValue(null, 23);
			s_isQuiverEquipped = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), method);
			Hosted = true;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Better Archery quiver hosting failed: " + ex.Message);
		}
	}

	internal static void KeepRow()
	{
		if (Hosted && (int)s_rowIndex.GetValue(null) != 23)
		{
			s_rowIndex.SetValue(null, 23);
		}
	}

	internal static bool IsQuiver(ItemDrop.ItemData item)
	{
		if (!Hosted || s_customSlotName == null || item?.m_shared == null || item.m_dropPrefab == null)
		{
			return false;
		}
		if (Quivers.TryGetValue(item.m_shared, out var value))
		{
			return value;
		}
		bool flag;
		try
		{
			flag = s_customSlotName(item) == "quiver";
		}
		catch
		{
			flag = false;
		}
		Quivers[item.m_shared] = flag;
		return flag;
	}

	internal static bool IsQuiverEquipped()
	{
		if (!QuiverActive)
		{
			return false;
		}
		try
		{
			return s_isQuiverEquipped();
		}
		catch
		{
			return false;
		}
	}
}
