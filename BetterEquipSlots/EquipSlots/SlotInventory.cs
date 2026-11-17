using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BetterEquipSlots.EquipSlots;

internal static class SlotInventory
{
	private static readonly ConditionalWeakTable<Inventory, object> Registered = new ConditionalWeakTable<Inventory, object>();

	private static readonly object Marker = new object();

	internal static void Register(Inventory inventory)
	{
		if (inventory != null && !Registered.TryGetValue(inventory, out var _))
		{
			Registered.Add(inventory, Marker);
		}
	}

	internal static void Unregister(Inventory inventory)
	{
		if (inventory != null)
		{
			Registered.Remove(inventory);
		}
	}

	internal static bool IsSlotInventory(Inventory inventory)
	{
		object value;
		if (inventory != null && Plugin.Active)
		{
			return Registered.TryGetValue(inventory, out value);
		}
		return false;
	}

	internal static bool InGrid(Inventory inventory, Vector2i pos)
	{
		if (pos.x >= 0 && pos.y >= 0 && pos.x < inventory.m_width)
		{
			return pos.y < inventory.m_height;
		}
		return false;
	}

	internal static int GridItemCount(Inventory inventory)
	{
		int num = 0;
		List<ItemDrop.ItemData> inventory2 = inventory.m_inventory;
		for (int i = 0; i < inventory2.Count; i++)
		{
			if (InGrid(inventory, inventory2[i].m_gridPos))
			{
				num++;
			}
		}
		return num;
	}

	internal static Vector2i FreeGridCell(Inventory inventory)
	{
		for (int i = 0; i < inventory.m_height; i++)
		{
			for (int j = 0; j < inventory.m_width; j++)
			{
				if (inventory.GetItemAt(j, i) == null)
				{
					return new Vector2i(j, i);
				}
			}
		}
		return new Vector2i(-1, -1);
	}

	internal static Vector2i FreeGridCell(Inventory inventory, bool topFirst)
	{
		for (int i = 0; i < inventory.m_height; i++)
		{
			int y = (topFirst ? i : (inventory.m_height - 1 - i));
			for (int j = 0; j < inventory.m_width; j++)
			{
				if (inventory.GetItemAt(j, y) == null)
				{
					return new Vector2i(j, y);
				}
			}
		}
		return new Vector2i(-1, -1);
	}

	internal static Vector2i FreeGridCellPreferBag(Inventory inventory)
	{
		for (int i = 1; i < inventory.m_height; i++)
		{
			for (int j = 0; j < inventory.m_width; j++)
			{
				if (inventory.GetItemAt(j, i) == null)
				{
					return new Vector2i(j, i);
				}
			}
		}
		for (int k = 0; k < inventory.m_width; k++)
		{
			if (inventory.GetItemAt(k, 0) == null)
			{
				return new Vector2i(k, 0);
			}
		}
		return new Vector2i(-1, -1);
	}

	internal static SlotCell FreeStashCell(Inventory inventory)
	{
		foreach (SlotCell item in SlotLayout.Stash)
		{
			if (inventory.GetItemAt(item.Pos.x, item.Pos.y) == null)
			{
				return item;
			}
		}
		return null;
	}

	internal static bool MoveToGridOrStash(Inventory inventory, ItemDrop.ItemData item)
	{
		Vector2i gridPos = FreeGridCellPreferBag(inventory);
		if (gridPos.x >= 0)
		{
			item.m_gridPos = gridPos;
			return true;
		}
		SlotCell slotCell = FreeStashCell(inventory);
		if (slotCell != null)
		{
			item.m_gridPos = slotCell.Pos;
			return true;
		}
		return false;
	}

	internal static ItemDrop.ItemData ItemIn(Inventory inventory, SlotCell cell)
	{
		return inventory.GetItemAt(cell.Pos.x, cell.Pos.y);
	}

	internal static SlotCell TargetCell(Inventory inventory, SlotKind kind, ItemDrop.ItemData item)
	{
		SlotCell slotCell = null;
		foreach (SlotCell item2 in SlotLayout.CellsOf(kind))
		{
			if (JewelcraftingHost.IsWornByIt(item) && item2.Ordinal > 0)
			{
				continue;
			}
			ItemDrop.ItemData itemData = ItemIn(inventory, item2);
			if (itemData == item || itemData == null)
			{
				return item2;
			}
			if (slotCell == null)
			{
				slotCell = item2;
			}
		}
		return slotCell;
	}

	internal static bool InGridOrSlot(ItemDrop.ItemData item)
	{
		if (SlotLayout.At(item.m_gridPos) == null)
		{
			return item.m_gridPos.y < 20;
		}
		return true;
	}

	internal static bool InOwnCell(ItemDrop.ItemData item)
	{
		SlotCell slotCell = SlotLayout.At(item.m_gridPos);
		if (slotCell == null)
		{
			return false;
		}
		SlotKind slotKind = SlotLayout.HomeKind(item);
		if (!slotCell.IsEquipment || slotCell.Kind != slotKind || !SlotLayout.IsOpen(slotCell))
		{
			if (slotCell.Kind == SlotKind.Quiver)
			{
				return slotKind == SlotKind.Ammo;
			}
			return false;
		}
		return true;
	}
}
