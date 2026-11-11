using System;
using System.Reflection;
using BepInEx.Bootstrap;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

// Einherjar Bodies already swaps the body on the character preview, as it does on any player VisEquipment. Its look (eyes,
// underwear, tattoos) it reads from the VisEquipment's own Player or network data, and the preview copy has neither, so it
// came out with the default look. Its look API is asked to show the player's look on the preview instead.
internal static class EinherjarBodiesHost
{
	internal const string Guid = "MagicMike.EinherjarBodies";

	private const int ApiVersion = 1;

	private static Func<GameObject, bool> s_drawn;

	private static Action<VisEquipment, VisEquipment> s_useLookOf;

	private static bool s_looked;

	private static void Find()
	{
		if (s_looked)
		{
			return;
		}
		s_looked = true;
		try
		{
			if (!Chainloader.PluginInfos.TryGetValue(Guid, out var info) || info.Instance == null)
			{
				return;
			}
			Type api = info.Instance.GetType().Assembly.GetType("EinherjarBodies.EinherjarLookApi");
			FieldInfo version = api?.GetField("Version", BindingFlags.Public | BindingFlags.Static);
			if (version == null || (int)version.GetValue(null) != ApiVersion)
			{
				Plugin.Log.LogWarning($"Einherjar Bodies {info.Metadata.Version} has no look API this version understands, so the character preview shows its default look.");
				return;
			}
			s_drawn = (Func<GameObject, bool>)Delegate.CreateDelegate(typeof(Func<GameObject, bool>), api.GetMethod("Drawn", BindingFlags.Public | BindingFlags.Static));
			s_useLookOf = (Action<VisEquipment, VisEquipment>)Delegate.CreateDelegate(typeof(Action<VisEquipment, VisEquipment>), api.GetMethod("UseLookOf", BindingFlags.Public | BindingFlags.Static));
		}
		catch (Exception ex)
		{
			s_drawn = null;
			s_useLookOf = null;
			Plugin.Log.LogWarning("Could not show Einherjar Bodies' look on the character preview: " + ex.Message);
		}
	}

	// The Einherjar body draws without writing alpha, so on a see-through background it doesn't show (VillagePlebs' looks
	// preview hit the same and draws on a solid background while it's there)
	internal static bool Drawn(GameObject character)
	{
		Find();
		return s_drawn != null && character != null && s_drawn(character);
	}

	// The preview copy shows the look of the player it stands for
	internal static void ShowLookOf(VisEquipment preview, VisEquipment source)
	{
		Find();
		s_useLookOf?.Invoke(preview, source);
	}
}
