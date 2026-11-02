using System.Collections.Generic;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

internal static class Migration
{
	private const string EaqsSlot = "eaqs_slot";

	private const string EaqsPlayer = "eaqs_player";

	internal static int Normalize(Inventory inventory, int visibleRows)
	{
		List<ItemDrop.ItemData> list = new List<ItemDrop.ItemData>();
		HashSet<Vector2i> hashSet = new HashSet<Vector2i>();
		foreach (ItemDrop.ItemData item in inventory.m_inventory)
		{
			Vector2i gridPos = item.m_gridPos;
			if (((gridPos.x < 0 || gridPos.x >= inventory.m_width || gridPos.y < 0 || gridPos.y >= visibleRows) && !SlotLayout.InRegion(gridPos)) || !hashSet.Add(gridPos))
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			return 0;
		}
		list.Sort((ItemDrop.ItemData a, ItemDrop.ItemData b) => StampRank(a).CompareTo(StampRank(b)));
		int num = 0;
		foreach (ItemDrop.ItemData item2 in list)
		{
			Vector2i gridPos2 = item2.m_gridPos;
			SlotCell slotCell = FromEaqsStamp(item2);
			if (slotCell == null && item2.m_equipped)
			{
				SlotKind slotKind = SlotLayout.HomeKind(item2);
				if (slotKind != SlotKind.None)
				{
					foreach (SlotCell item3 in SlotLayout.CellsOf(slotKind))
					{
						if (!hashSet.Contains(item3.Pos))
						{
							slotCell = item3;
							break;
						}
					}
				}
			}
			Vector2i vector2i;
			if (slotCell != null && !hashSet.Contains(slotCell.Pos) && (slotCell.Kind == SlotKind.Quiver || SlotLayout.Accepts(slotCell, item2)))
			{
				vector2i = slotCell.Pos;
			}
			else
			{
				vector2i = FreeCell(inventory, visibleRows, hashSet);
				if (vector2i.x < 0)
				{
					continue;
				}
				if (SlotLayout.At(vector2i) == null || !SlotLayout.At(vector2i).IsEquipment)
				{
					item2.m_equipped = false;
				}
			}
			hashSet.Add(vector2i);
			item2.m_gridPos = vector2i;
			if (item2.m_customData != null)
			{
				item2.m_customData.Remove("eaqs_slot");
				item2.m_customData.Remove("eaqs_player");
			}
			SlotCell slotCell2 = SlotLayout.At(vector2i);
			num++;
		}
		return num;
	}

	private static int StampRank(ItemDrop.ItemData item)
	{
		if (FromEaqsStamp(item) == null)
		{
			if (!item.m_equipped)
			{
				return 2;
			}
			return 1;
		}
		return 0;
	}

	private static SlotCell FromEaqsStamp(ItemDrop.ItemData item)
	{
		if (item.m_customData == null || !item.m_customData.TryGetValue("eaqs_slot", out var value) || string.IsNullOrEmpty(value))
		{
			return null;
		}
		if (value.StartsWith("Quick") && int.TryParse(value.Substring(5), out var result) && result >= 1 && result <= 6)
		{
			return SlotLayout.Quick[result - 1];
		}
		if (value.StartsWith("CustomBetterArcheryQuiver") && int.TryParse(value.Substring("CustomBetterArcheryQuiver".Length), out var result2) && result2 >= 1 && result2 <= 3)
		{
			return SlotLayout.Quiver[result2 - 1];
		}
		SlotKind kind;
		switch (value)
		{
		case "Helmet":
			kind = SlotKind.Helmet;
			break;
		case "Chest":
			kind = SlotKind.Chest;
			break;
		case "Legs":
			kind = SlotKind.Legs;
			break;
		case "Shoulder":
			kind = SlotKind.Cape;
			break;
		case "Utility":
			kind = SlotKind.Belt;
			break;
		case "Trinket":
			kind = SlotKind.Trinket;
			break;
		default:
			return null;
		}
		using (IEnumerator<SlotCell> enumerator = SlotLayout.CellsOf(kind).GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				return enumerator.Current;
			}
		}
		return null;
	}

	private static Vector2i FreeCell(Inventory inventory, int visibleRows, HashSet<Vector2i> taken)
	{
		for (int i = 1; i < visibleRows; i++)
		{
			for (int j = 0; j < inventory.m_width; j++)
			{
				Vector2i vector2i = new Vector2i(j, i);
				if (!taken.Contains(vector2i))
				{
					return vector2i;
				}
			}
		}
		for (int k = 0; k < inventory.m_width; k++)
		{
			Vector2i vector2i2 = new Vector2i(k, 0);
			if (!taken.Contains(vector2i2))
			{
				return vector2i2;
			}
		}
		foreach (SlotCell item in SlotLayout.Stash)
		{
			if (!taken.Contains(item.Pos))
			{
				return item.Pos;
			}
		}
		return new Vector2i(-1, -1);
	}

	private static string Name(ItemDrop.ItemData item)
	{
		string text = ((item.m_dropPrefab != null) ? item.m_dropPrefab.name : (item.m_shared?.m_name ?? "?"));
		if (item.m_stack <= 1)
		{
			return text;
		}
		return $"{text} x{item.m_stack}";
	}

	internal static int VisibleRows(Player player)
	{
		if (player.TryGetUniqueKeyValue("invrows", out var value) && int.TryParse(value, out var result))
		{
			return Mathf.Clamp(result, 1, 9);
		}
		return Mathf.Max(((Humanoid)player).m_inventory.m_height, 4);
	}
}
