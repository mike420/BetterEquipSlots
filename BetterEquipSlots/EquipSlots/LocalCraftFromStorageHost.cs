using System;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;

namespace BetterEquipSlots.EquipSlots;

internal static class LocalCraftFromStorageHost
{
	internal const string Guid = "LuminairPrime.valheim.localcraftfromstorageprime";

	internal static void Setup(Harmony harmony)
	{
		if (harmony == null || !Chainloader.PluginInfos.TryGetValue("LuminairPrime.valheim.localcraftfromstorageprime", out var value) || value.Instance == null)
		{
			return;
		}
		try
		{
			MethodInfo methodInfo = value.Instance.GetType().Assembly.GetType("LocalCraftFromStoragePrime.DepositOperation")?.GetMethod("Eligible", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(ItemDrop.ItemData) }, null);
			if (methodInfo == null || methodInfo.ReturnType != typeof(bool))
			{
				Plugin.Log.LogWarning($"Local Craft From Storage Prime {value.Metadata.Version}: its deposit check was not " + "found, so \"Store Nearby\" may move items out of the slots.");
			}
			else
			{
				harmony.Patch(methodInfo, null, new HarmonyMethod(typeof(LocalCraftFromStorageHost), "LeaveSlotsAlone"));
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not keep Local Craft From Storage Prime out of the slots: " + ex.Message);
		}
	}

	private static void LeaveSlotsAlone(ItemDrop.ItemData __0, ref bool __result)
	{
		if (__result && Plugin.Active && __0 != null)
		{
			Vector2i gridPos = __0.m_gridPos;
			if ((gridPos.y >= 20 && gridPos.y <= 31) || Locks.IsLocked(gridPos))
			{
				__result = false;
			}
		}
	}
}
