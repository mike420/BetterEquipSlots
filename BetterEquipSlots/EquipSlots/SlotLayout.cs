using System.Collections.Generic;

namespace BetterEquipSlots.EquipSlots;

internal static class SlotLayout
{
	internal const int Width = 8;

	internal const int FirstRow = 20;

	internal const int QuickRow = 22;

	internal const int QuiverRow = 23;

	internal const int FirstStashRow = 24;

	internal const int LastRow = 31;

	internal const int QuickCount = 6;

	internal const int QuiverCount = 3;

	internal const int ExtraBelts = 4;

	internal const int LookCount = 4;

	internal static readonly List<SlotCell> All;

	internal static readonly List<SlotCell> Equipment;

	internal static readonly List<SlotCell> Quick;

	internal static readonly List<SlotCell> Quiver;

	internal static readonly List<SlotCell> Stash;

	internal static readonly List<SlotCell> ExtraBelt;

	internal static readonly List<SlotCell> Look;

	private static readonly SlotCell[,] ByPos;

	static SlotLayout()
	{
		All = new List<SlotCell>();
		Equipment = new List<SlotCell>();
		Quick = new List<SlotCell>();
		Quiver = new List<SlotCell>();
		Stash = new List<SlotCell>();
		ExtraBelt = new List<SlotCell>();
		Look = new List<SlotCell>();
		ByPos = new SlotCell[8, 12];
		Add(SlotKind.Helmet, 0, 0, 20);
		Add(SlotKind.Chest, 0, 1, 20);
		Add(SlotKind.Legs, 0, 2, 20);
		Add(SlotKind.Cape, 0, 3, 20);
		Add(SlotKind.Belt, 0, 4, 20);
		Add(SlotKind.Trinket, 0, 5, 20);
		Add(SlotKind.Amulet, 0, 6, 20);
		Add(SlotKind.Ring, 0, 7, 20);
		Add(SlotKind.Ring, 1, 0, 21);
		Add(SlotKind.Earring, 0, 1, 21);
		Add(SlotKind.Earring, 1, 2, 21);
		Add(SlotKind.Ammo, 0, 3, 21);
		for (int i = 1; i <= 4; i++)
		{
			ExtraBelt.Add(Add(SlotKind.Belt, i, 3 + i, 21));
		}
		for (int j = 0; j < 6; j++)
		{
			Add(SlotKind.Quick, j, j, 22);
		}
		for (int k = 0; k < 3; k++)
		{
			Add(SlotKind.Quiver, k, k, 23);
		}
		int num = 0;
		for (int l = 24; l <= 31; l++)
		{
			for (int m = 0; m < 8; m++)
			{
				Add(SlotKind.Stash, num++, m, l);
			}
		}
		for (int n = 0; n < 4; n++)
		{
			Add(SlotKind.Look, n, 4 + n, 23);
		}
	}

	private static SlotCell Add(SlotKind kind, int ordinal, int x, int y)
	{
		SlotCell slotCell = new SlotCell(All.Count, kind, ordinal, new Vector2i(x, y));
		All.Add(slotCell);
		ByPos[x, y - 20] = slotCell;
		if (IsEquipmentKind(kind))
		{
			Equipment.Add(slotCell);
		}
		else
		{
			switch (kind)
			{
			case SlotKind.Quick:
				Quick.Add(slotCell);
				break;
			case SlotKind.Quiver:
				Quiver.Add(slotCell);
				break;
			case SlotKind.Look:
				Look.Add(slotCell);
				break;
			default:
				Stash.Add(slotCell);
				break;
			}
		}
		return slotCell;
	}

	internal static bool IsOpen(SlotCell cell)
	{
		if (cell.Kind == SlotKind.Belt && cell.Ordinal != 0)
		{
			return cell.Ordinal <= MultiUtilityHost.ExtraCells;
		}
		return true;
	}

	internal static bool InRegion(Vector2i pos)
	{
		if (pos.x >= 0 && pos.x < 8 && pos.y >= 20)
		{
			return pos.y <= 31;
		}
		return false;
	}

	internal static SlotCell At(Vector2i pos)
	{
		if (!InRegion(pos))
		{
			return null;
		}
		return ByPos[pos.x, pos.y - 20];
	}

	internal static SlotCell At(int x, int y)
	{
		return At(new Vector2i(x, y));
	}

	internal static bool IsEquipmentKind(SlotKind kind)
	{
		if (kind >= SlotKind.Helmet)
		{
			return kind <= SlotKind.Ammo;
		}
		return false;
	}

	internal static bool IsJewelryKind(SlotKind kind)
	{
		if (kind != SlotKind.Ring && kind != SlotKind.Earring)
		{
			return kind == SlotKind.Amulet;
		}
		return true;
	}

	internal static SlotKind HomeKind(ItemDrop.ItemData item)
	{
		if (item?.m_shared == null)
		{
			return SlotKind.None;
		}
		switch (item.m_shared.m_itemType)
		{
		case ItemDrop.ItemData.ItemType.Helmet:
			return SlotKind.Helmet;
		case ItemDrop.ItemData.ItemType.Chest:
			return SlotKind.Chest;
		case ItemDrop.ItemData.ItemType.Legs:
			return SlotKind.Legs;
		case ItemDrop.ItemData.ItemType.Shoulder:
			return SlotKind.Cape;
		case ItemDrop.ItemData.ItemType.Utility:
			return SlotKind.Belt;
		case ItemDrop.ItemData.ItemType.Trinket:
			return SlotKind.Trinket;
		case ItemDrop.ItemData.ItemType.Ammo:
		case ItemDrop.ItemData.ItemType.AmmoNonEquipable:
			return SlotKind.Ammo;
		default:
		{
			SlotKind slotKind = Jewelry.KindOf(item);
			if (slotKind == SlotKind.None)
			{
				if (!BetterArcheryHost.IsQuiver(item))
				{
					return SlotKind.None;
				}
				return SlotKind.Ammo;
			}
			return slotKind;
		}
		}
	}

	internal static bool Accepts(SlotCell cell, ItemDrop.ItemData item)
	{
		if (cell == null || item?.m_shared == null)
		{
			return false;
		}
		switch (cell.Kind)
		{
		case SlotKind.Quick:
			return true;
		case SlotKind.Quiver:
			if (BetterArcheryHost.QuiverActive && BetterArcheryHost.IsQuiverEquipped())
			{
				return item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo;
			}
			return false;
		case SlotKind.Stash:
			return false;
		case SlotKind.Look:
			return false;
		case SlotKind.Ammo:
			return HomeKind(item) == SlotKind.Ammo;
		default:
			if (HomeKind(item) == cell.Kind)
			{
				return IsOpen(cell);
			}
			return false;
		}
	}

	internal static SlotKind LookKind(SlotCell cell)
	{
		if (cell == null || cell.Kind != SlotKind.Look)
		{
			return cell?.Kind ?? SlotKind.None;
		}
		return cell.Ordinal switch
		{
			0 => SlotKind.Helmet, 
			1 => SlotKind.Chest, 
			2 => SlotKind.Legs, 
			_ => SlotKind.Cape, 
		};
	}

	internal static SlotCell LookCellFor(SlotKind kind)
	{
		foreach (SlotCell item in Look)
		{
			if (LookKind(item) == kind)
			{
				return item;
			}
		}
		return null;
	}

	internal static IEnumerable<SlotCell> CellsOf(SlotKind kind)
	{
		foreach (SlotCell item in Equipment)
		{
			if (item.Kind == kind && IsOpen(item))
			{
				yield return item;
			}
		}
	}

	internal static SlotCell QuickCell(int ordinal)
	{
		return Quick[ordinal];
	}
}
