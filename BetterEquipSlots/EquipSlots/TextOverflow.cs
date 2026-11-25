using TMPro;

namespace BetterEquipSlots.EquipSlots;

internal static class TextOverflow
{
	/// <summary>
	/// Cuts overflowing text off with "…" when the font can draw it, and plainly otherwise. The game's font has no "…",
	/// and asking TextMeshPro for an ellipsis anyway logs a warning for every text object before it truncates instead.
	/// </summary>
	public static void SetEllipsisOverflow(this TMP_Text text)
	{
		bool hasEllipsis = text.font && text.font.HasCharacter((char)0x2026, searchFallbacks: true);
		text.overflowMode = hasEllipsis ? TextOverflowModes.Ellipsis : TextOverflowModes.Truncate;
	}
}
