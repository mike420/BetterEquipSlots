using System;
using System.Reflection;
using BepInEx.Bootstrap;
using HarmonyLib;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

// BodyReplacer already swaps the body on the character preview, as it does on any player VisEquipment. Its looks (colours,
// muscle, body shape, tattoos, beard, hair) it reads from the VisEquipment's Player or its ZDO, and the preview copy has
// neither, so it came out with the default look. Its reads are pointed at the player the preview shows instead.
internal static class BodyReplacerHost
{
	internal const string Guid = "MagicMike.BodyReplacer";

	private static Type s_visualType;

	internal static void Setup(Harmony harmony)
	{
		if (harmony == null || !Chainloader.PluginInfos.TryGetValue(Guid, out var info) || info.Instance == null)
		{
			return;
		}
		try
		{
			Assembly assembly = info.Instance.GetType().Assembly;
			s_visualType = assembly.GetType("BodyReplacer.EinherjarVisual");
			Type type = assembly.GetType("BodyReplacer.Appearance");
			Type[] extraArgs = new Type[2]
			{
				typeof(VisEquipment),
				typeof(string)
			};
			MethodInfo extra = type?.GetMethod("Extra", BindingFlags.Static | BindingFlags.Public, null, extraArgs, null);
			MethodInfo forVis = type?.GetMethod("For", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(VisEquipment) }, null);
			if (extra == null)
			{
				Plugin.Log.LogWarning($"BodyReplacer {info.Metadata.Version}: its Appearance.Extra was not found, so the character preview shows its default look.");
				return;
			}
			HarmonyMethod prefix = new HarmonyMethod(typeof(BodyReplacerHost), nameof(ReadFromPlayer));
			harmony.Patch(extra, prefix);
			if (forVis != null)
			{
				harmony.Patch(forVis, prefix);
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not show BodyReplacer's look on the character preview: " + ex.Message);
		}
	}

	// The Einherjar body draws without writing alpha, so on a see-through background it doesn't show (VillagePlebs' looks
	// preview hit the same and draws on a solid background while it's there)
	internal static bool Drawn(GameObject character)
	{
		if (s_visualType == null || character == null)
		{
			return false;
		}
		return character.GetComponent(s_visualType) is Behaviour behaviour && behaviour.enabled;
	}

	private static void ReadFromPlayer(ref VisEquipment vis)
	{
		VisEquipment source = PlayerPreview.SourceOf(vis);
		if (source != null)
		{
			vis = source;
		}
	}
}
