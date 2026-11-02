using System;
using System.Collections.Generic;

namespace BetterEquipSlots.EquipSlots;

internal static class SlotMemory
{
	internal const string Key = "CDEV_EquipmentSlots_slot";

	internal const string CellKey = "CDEV_EquipmentSlots_cell";

	private static readonly List<KeyValuePair<ItemDrop.ItemData, Vector2i>> Pending = new List<KeyValuePair<ItemDrop.ItemData, Vector2i>>();

	internal static void Stamp(ItemDrop.ItemData item, SlotCell cell, long playerId)
	{
		if (item.m_customData == null)
		{
			item.m_customData = new Dictionary<string, string>();
		}
		Dictionary<string, string> customData = item.m_customData;
		int index = cell.Index;
		customData["CDEV_EquipmentSlots_slot"] = index + "|" + playerId;
	}

	private static void StampCell(ItemDrop.ItemData item, Vector2i pos, long playerId)
	{
		if (item.m_customData == null)
		{
			item.m_customData = new Dictionary<string, string>();
		}
		item.m_customData["CDEV_EquipmentSlots_cell"] = pos.x + "," + pos.y + "|" + playerId;
	}

	internal static bool Restore(Player player, Inventory inventory, ItemDrop.ItemData item)
	{
		if (item.m_customData == null || !item.m_customData.TryGetValue("CDEV_EquipmentSlots_slot", out var value))
		{
			return false;
		}
		item.m_customData.Remove("CDEV_EquipmentSlots_slot");
		string[] array = value.Split(new char[1] { '|' });
		if (array.Length != 2 || !int.TryParse(array[0], out var result) || !long.TryParse(array[1], out var result2) || result2 != player.GetPlayerID() || result < 0 || result >= SlotLayout.All.Count)
		{
			return false;
		}
		SlotCell slotCell = SlotLayout.All[result];
		if (SlotLayout.At(item.m_gridPos) != null || !SlotLayout.Accepts(slotCell, item) || SlotInventory.ItemIn(inventory, slotCell) != null)
		{
			return false;
		}
		item.m_gridPos = slotCell.Pos;
		return true;
	}

	internal static void TakeBack(Inventory into, Inventory from)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null || from == null || into != ((Humanoid)localPlayer).m_inventory || from == into)
		{
			return;
		}
		long playerID = localPlayer.GetPlayerID();
		bool flag = false;
		for (int i = 0; i < 3; i++)
		{
			if (i == 1)
			{
				WearQuiver(localPlayer, into);
			}
			foreach (ItemDrop.ItemData item in new List<ItemDrop.ItemData>(from.m_inventory))
			{
				Vector2i cell;
				string head2;
				if (Stamped(item, "CDEV_EquipmentSlots_slot", playerID, out var head))
				{
					if (i == 2 || !int.TryParse(head, out var result) || result < 0 || result >= SlotLayout.All.Count)
					{
						continue;
					}
					SlotCell slotCell = SlotLayout.All[result];
					if (slotCell.Kind == SlotKind.Quiver != (i == 1) || !SlotLayout.Accepts(slotCell, item) || SlotInventory.ItemIn(into, slotCell) != null)
					{
						continue;
					}
					cell = slotCell.Pos;
				}
				else if (i != 2 || !Stamped(item, "CDEV_EquipmentSlots_cell", playerID, out head2) || !TryCell(head2, out cell) || !SlotInventory.InGrid(into, cell) || into.GetItemAt(cell.x, cell.y) != null)
				{
					continue;
				}
				if (into.AddItem(item, item.m_stack, cell.x, cell.y, false))
				{
					from.RemoveItem(item);
					ItemDrop.ItemData itemAt = into.GetItemAt(cell.x, cell.y);
					if (itemAt?.m_customData != null)
					{
						itemAt.m_customData.Remove("CDEV_EquipmentSlots_slot");
						itemAt.m_customData.Remove("CDEV_EquipmentSlots_cell");
					}
					flag = true;
				}
			}
		}
		if (flag)
		{
			into.Changed(false, false);
		}
	}

	private static void WearQuiver(Player player, Inventory inventory)
	{
		if (!BetterArcheryHost.QuiverActive || BetterArcheryHost.IsQuiverEquipped())
		{
			return;
		}
		foreach (SlotCell item in SlotLayout.CellsOf(SlotKind.Ammo))
		{
			ItemDrop.ItemData itemData = SlotInventory.ItemIn(inventory, item);
			if (itemData != null && BetterArcheryHost.IsQuiver(itemData) && !itemData.m_equipped)
			{
				player.EquipItem(itemData, triggerEquipEffects: false);
			}
		}
	}

	private static bool Stamped(ItemDrop.ItemData item, string key, long playerId, out string head)
	{
		head = null;
		if (item.m_customData == null || !item.m_customData.TryGetValue(key, out var value))
		{
			return false;
		}
		int num = value.LastIndexOf('|');
		if (num <= 0 || !long.TryParse(value.Substring(num + 1), out var result) || result != playerId)
		{
			return false;
		}
		head = value.Substring(0, num);
		return true;
	}

	private static bool TryCell(string xy, out Vector2i cell)
	{
		cell = default;
		string[] array = xy.Split(new char[1] { ',' });
		if (array.Length != 2 || !int.TryParse(array[0], out var result) || !int.TryParse(array[1], out var result2))
		{
			return false;
		}
		cell = new Vector2i(result, result2);
		return true;
	}

	internal static bool RestoreCells(Player player, Inventory inventory, List<ItemDrop.ItemData> items)
	{
		Pending.Clear();
		long playerID = player.GetPlayerID();
		foreach (ItemDrop.ItemData item in items)
		{
			if (item.m_customData == null || !item.m_customData.TryGetValue("CDEV_EquipmentSlots_cell", out var value))
			{
				continue;
			}
			item.m_customData.Remove("CDEV_EquipmentSlots_cell");
			string[] array = value.Split(new char[1] { '|' });
			string[] array2 = ((array.Length == 2) ? array[0].Split(new char[1] { ',' }) : null);
			if (array2 != null && array2.Length == 2 && int.TryParse(array2[0], out var result) && int.TryParse(array2[1], out var result2) && long.TryParse(array[1], out var result3) && result3 == playerID)
			{
				Vector2i vector2i = new Vector2i(result, result2);
				if (SlotLayout.At(item.m_gridPos) == null && SlotInventory.InGrid(inventory, vector2i) && item.m_gridPos != vector2i)
				{
					Pending.Add(new KeyValuePair<ItemDrop.ItemData, Vector2i>(item, vector2i));
				}
			}
		}
		bool result4 = false;
		bool flag = true;
		while (flag && Pending.Count > 0)
		{
			flag = false;
			for (int num = Pending.Count - 1; num >= 0; num--)
			{
				ItemDrop.ItemData key = Pending[num].Key;
				Vector2i value2 = Pending[num].Value;
				ItemDrop.ItemData itemAt = inventory.GetItemAt(value2.x, value2.y);
				if (itemAt == null)
				{
					key.m_gridPos = value2;
				}
				else
				{
					if (!IsPending(itemAt))
					{
						continue;
					}
					itemAt.m_gridPos = key.m_gridPos;
					key.m_gridPos = value2;
				}
				Pending.RemoveAt(num);
				result4 = (flag = true);
			}
		}
		Pending.Clear();
		return result4;
	}

	private static bool IsPending(ItemDrop.ItemData item)
	{
		for (int i = 0; i < Pending.Count; i++)
		{
			if (Pending[i].Key == item)
			{
				return true;
			}
		}
		return false;
	}

	internal static void CompactGrave(Inventory grave, long playerId)
	{
		List<ItemDrop.ItemData> inventory = grave.m_inventory;
		HashSet<Vector2i> hashSet = new HashSet<Vector2i>();
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>();
		List<ItemDrop.ItemData> list2 = new List<ItemDrop.ItemData>();
		foreach (ItemDrop.ItemData item in inventory)
		{
			if (SlotInventory.InGrid(grave, item.m_gridPos) && hashSet.Add(item.m_gridPos))
			{
				list.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
		if (list2.Count == 0)
		{
			return;
		}
		list2.Sort((ItemDrop.ItemData a, ItemDrop.ItemData b) => Order(a).CompareTo(Order(b)));
		list.Sort((ItemDrop.ItemData a, ItemDrop.ItemData b) => (a.m_gridPos.y == b.m_gridPos.y) ? a.m_gridPos.x.CompareTo(b.m_gridPos.x) : a.m_gridPos.y.CompareTo(b.m_gridPos.y));
		int width = grave.m_width;
		int num = 0;
		foreach (ItemDrop.ItemData item2 in list2)
		{
			SlotCell slotCell = SlotLayout.At(item2.m_gridPos);
			if (slotCell != null && slotCell.Kind != SlotKind.Stash)
			{
				Stamp(item2, slotCell, playerId);
			}
			item2.m_gridPos = new Vector2i(num % width, num / width);
			num++;
		}
		foreach (ItemDrop.ItemData item3 in list)
		{
			StampCell(item3, item3.m_gridPos, playerId);
			item3.m_gridPos = new Vector2i(num % width, num / width);
			num++;
		}
		grave.m_height = Math.Max(grave.m_height, (num + width - 1) / width);
		grave.Changed(false, false);
	}

	private static int Order(ItemDrop.ItemData item)
	{
		return SlotLayout.At(item.m_gridPos)?.Index ?? int.MaxValue;
	}
}
