using BepInEx.Logging;

namespace BetterEquipSlots.EquipSlots;

/// <summary>
/// What the slots' code reads from the plugin (<see cref="BetterEquipSlotsPlugin"/>): whether it is running, the log, and
/// the GUIDs of the mods it stands in for.
/// </summary>
internal static class Plugin
{
	/// <summary>EquipmentAndQuickSlots: its slot items are converted (<see cref="Migration"/>); with it installed this mod stays off.</summary>
	internal const string EaqsGuid = "randyknapp.mods.equipmentandquickslots";

	/// <summary>CDEV.EquipmentSlots, which this mod is a port of: the two would both take the same slots, so with it installed this mod stays off.</summary>
	internal const string CdevGuid = "cdev.equipmentslots";

	internal static ManualLogSource Log;

	internal static bool Active { get; set; }
}
