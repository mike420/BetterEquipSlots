namespace BetterEquipSlots.EquipSlots;

internal sealed class SlotCell
{
	internal readonly int Index;

	internal readonly SlotKind Kind;

	internal readonly int Ordinal;

	internal readonly Vector2i Pos;

	internal bool IsEquipment => SlotLayout.IsEquipmentKind(Kind);

	internal SlotCell(int index, SlotKind kind, int ordinal, Vector2i pos)
	{
		Index = index;
		Kind = kind;
		Ordinal = ordinal;
		Pos = pos;
	}
}
