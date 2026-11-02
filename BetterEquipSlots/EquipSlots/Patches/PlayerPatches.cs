using System;
using HarmonyLib;

namespace BetterEquipSlots.EquipSlots.Patches;

[HarmonyPatch]
internal static class PlayerPatches
{
	[HarmonyPatch(typeof(Player), "Awake")]
	[HarmonyPostfix]
	private static void Awake(Player __instance)
	{
		SlotInventory.Register(((Humanoid)__instance).m_inventory);
	}

	[HarmonyPatch(typeof(Player), "Load")]
	[HarmonyPrefix]
	private static void LoadPrefix()
	{
		EquipSync.Enter();
	}

	[HarmonyPatch(typeof(Player), "Load")]
	[HarmonyPostfix]
	[HarmonyPriority(200)]
	[HarmonyAfter(new string[] { "cdev.sleepingplayers" })]
	private static void LoadPostfix(Player __instance)
	{
		if (!Plugin.Active || ((Humanoid)__instance).m_inventory == null)
		{
			return;
		}
		try
		{
			if (Migration.Normalize(((Humanoid)__instance).m_inventory, Migration.VisibleRows(__instance)) > 0)
			{
				((Humanoid)__instance).m_inventory.Changed(false, false);
			}
		}
		catch (Exception arg)
		{
			Plugin.Log.LogError($"Placing items after load failed: {arg}");
		}
		EquipSync.MarkDirty();
	}

	[HarmonyPatch(typeof(Player), "Load")]
	[HarmonyFinalizer]
	private static void LoadDone()
	{
		EquipSync.Exit();
	}

	[HarmonyPatch(typeof(Player), "OnSpawned")]
	[HarmonyPostfix]
	private static void OnSpawned()
	{
		BetterArcheryHost.KeepRow();
		EquipSync.MarkDirty();
	}
}
