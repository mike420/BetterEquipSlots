using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

internal static class EquipSync
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal readonly struct BusyScope : IDisposable
	{
		internal static BusyScope Enter()
		{
			s_busy++;
			return default;
		}

		public void Dispose()
		{
			s_busy--;
		}
	}

	internal struct DragState
	{
		internal bool Dropping;

		internal Inventory TargetInventory;

		internal Vector2i TargetPos;

		internal Inventory SourceInventory;

		internal Vector2i SourcePos;
	}

	private static int s_busy;

	internal static int EquipDepth;

	private static bool s_dirty = true;

	private static float s_nextPass;

	private static readonly List<ItemDrop.ItemData> Items = new List<ItemDrop.ItemData>();

	private static readonly Dictionary<ItemDrop.ItemData, Vector2i> Refused = new Dictionary<ItemDrop.ItemData, Vector2i>();

	private static Player s_refusedFor;

	private static readonly Dictionary<ItemDrop.ItemData, Vector2i> ReturnCells = new Dictionary<ItemDrop.ItemData, Vector2i>();

	internal static bool Busy => s_busy > 0;

	internal static void Enter()
	{
		s_busy++;
	}

	internal static void Exit()
	{
		s_busy = Math.Max(0, s_busy - 1);
	}

	internal static void MarkDirty()
	{
		s_dirty = true;
	}

	internal static bool IsLocal(Humanoid humanoid)
	{
		if (humanoid != null)
		{
			return humanoid == Player.m_localPlayer;
		}
		return false;
	}

	private static bool CanManage(Player player)
	{
		if (player != null && !player.m_isLoading && !player.IsDead())
		{
			return ((Humanoid)player).m_inventory != null;
		}
		return false;
	}

	internal static bool StaysInGrid(ItemDrop.ItemData item, SlotKind kind)
	{
		if (!ModConfig.KeepWornInInventory.Value || item == null)
		{
			return false;
		}
		switch (kind)
		{
		case SlotKind.Helmet:
		case SlotKind.Chest:
		case SlotKind.Legs:
		case SlotKind.Cape:
		case SlotKind.Belt:
		case SlotKind.Trinket:
			return true;
		case SlotKind.Ammo:
			if (!BetterArcheryHost.IsQuiver(item))
			{
				return !BetterArcheryHost.IsQuiverEquipped();
			}
			return false;
		default:
			return false;
		}
	}

	internal static ItemDrop.ItemData MirrorOf(Player player, SlotCell cell)
	{
		if (!ModConfig.KeepWornInInventory.Value || player == null || cell == null)
		{
			return null;
		}
		ItemDrop.ItemData itemData;
		switch (cell.Kind)
		{
		case SlotKind.Helmet:
			itemData = ((Humanoid)player).m_helmetItem;
			break;
		case SlotKind.Chest:
			itemData = ((Humanoid)player).m_chestItem;
			break;
		case SlotKind.Legs:
			itemData = ((Humanoid)player).m_legItem;
			break;
		case SlotKind.Cape:
			itemData = ((Humanoid)player).m_shoulderItem;
			break;
		case SlotKind.Belt:
			itemData = ((cell.Ordinal == 0) ? ((Humanoid)player).m_utilityItem : null);
			break;
		case SlotKind.Trinket:
			itemData = ((Humanoid)player).m_trinketItem;
			break;
		case SlotKind.Ammo:
			itemData = ((Humanoid)player).m_ammoItem;
			break;
		default:
			return null;
		}
		if (itemData == null || !StaysInGrid(itemData, cell.Kind) || SlotInventory.InOwnCell(itemData) || !((Humanoid)player).m_inventory.ContainsItem(itemData))
		{
			return null;
		}
		return itemData;
	}

	internal static void OnEquipped(Player player, ItemDrop.ItemData item)
	{
		if (Busy || !CanManage(player) || item == null)
		{
			return;
		}
		SlotKind slotKind = SlotLayout.HomeKind(item);
		if (slotKind == SlotKind.None || SlotInventory.InOwnCell(item) || !((Humanoid)player).m_inventory.ContainsItem(item) || StaysInGrid(item, slotKind))
		{
			return;
		}
		using (BusyScope.Enter())
		{
			if (PlaceIntoSlot(((Humanoid)player).m_inventory, item, slotKind))
			{
				((Humanoid)player).m_inventory.Changed(false, false);
			}
		}
	}

	internal static void OnUnequipped(Player player, ItemDrop.ItemData item)
	{
		if (Busy || EquipDepth > 0 || !CanManage(player) || item == null || player.IsItemEquiped(item) || !SlotInventory.InOwnCell(item) || !((Humanoid)player).m_inventory.ContainsItem(item) || (item.m_shared.m_useDurability && item.m_durability <= 0f))
		{
			return;
		}
		SlotCell slotCell = SlotLayout.At(item.m_gridPos);
		if (slotCell != null && slotCell.Kind == SlotKind.Quiver)
		{
			return;
		}
		using (BusyScope.Enter())
		{
			Vector2i gridPos = SlotInventory.FreeGridCellPreferBag(((Humanoid)player).m_inventory);
			if (gridPos.x >= 0)
			{
				item.m_gridPos = gridPos;
				((Humanoid)player).m_inventory.Changed(false, false);
			}
		}
	}

	internal static bool PlaceIntoSlot(Inventory inventory, ItemDrop.ItemData item, SlotKind kind)
	{
		if (kind == SlotKind.Ammo && BetterArcheryHost.IsQuiverEquipped() && !BetterArcheryHost.IsQuiver(item))
		{
			foreach (SlotCell item2 in SlotLayout.Quiver)
			{
				if (SlotInventory.ItemIn(inventory, item2) == null)
				{
					item.m_gridPos = item2.Pos;
					return true;
				}
			}
			return false;
		}
		SlotCell slotCell = SlotInventory.TargetCell(inventory, kind, item);
		if (slotCell == null)
		{
			return false;
		}
		ItemDrop.ItemData itemData = SlotInventory.ItemIn(inventory, slotCell);
		if (itemData == item)
		{
			return false;
		}
		if (itemData != null && BetterArcheryHost.IsQuiver(itemData))
		{
			return false;
		}
		if (itemData != null && kind == SlotKind.Belt && MultiUtility.Active)
		{
			return false;
		}
		Vector2i gridPos = item.m_gridPos;
		if (itemData != null)
		{
			SlotCell slotCell2 = SlotLayout.At(gridPos);
			if ((slotCell2 == null) ? SlotInventory.InGrid(inventory, gridPos) : SlotLayout.Accepts(slotCell2, itemData))
			{
				itemData.m_gridPos = gridPos;
			}
			else
			{
				item.m_gridPos = new Vector2i(-1000, -1000);
				if (!SlotInventory.MoveToGridOrStash(inventory, itemData))
				{
					item.m_gridPos = gridPos;
					return false;
				}
			}
			if (itemData.m_equipped && Player.m_localPlayer != null && ((Humanoid)Player.m_localPlayer).m_inventory == inventory)
			{
				Player.m_localPlayer.UnequipItem(itemData, triggerEquipEffects: false);
			}
		}
		item.m_gridPos = slotCell.Pos;
		return true;
	}

	internal static void AfterDrop(DragState state)
	{
		Player localPlayer = Player.m_localPlayer;
		if (!state.Dropping || !CanManage(localPlayer))
		{
			return;
		}
		Inventory inventory = ((Humanoid)localPlayer).m_inventory;
		using (BusyScope.Enter())
		{
			if (state.TargetInventory == inventory)
			{
				ItemDrop.ItemData itemAt = inventory.GetItemAt(state.TargetPos.x, state.TargetPos.y);
				ApplyLocation(localPlayer, itemAt);
				if (itemAt != null && state.SourceInventory == inventory && SlotInventory.InGrid(inventory, state.SourcePos) && SlotInventory.InOwnCell(itemAt) && StaysInGrid(itemAt, SlotLayout.HomeKind(itemAt)))
				{
					ReturnCells[itemAt] = state.SourcePos;
				}
			}
			if (state.SourceInventory == inventory)
			{
				ApplyLocation(localPlayer, inventory.GetItemAt(state.SourcePos.x, state.SourcePos.y));
			}
		}
		MarkDirty();
	}

	private static void ApplyLocation(Player player, ItemDrop.ItemData item)
	{
		if (item == null || !((Humanoid)player).m_inventory.ContainsItem(item) || SlotLayout.HomeKind(item) == SlotKind.None)
		{
			return;
		}
		bool flag = SlotInventory.InOwnCell(item);
		bool flag2 = player.IsItemEquiped(item);
		if (flag && !flag2)
		{
			if (item.m_shared.m_equipDuration > 0f && SlotInventory.InGridOrSlot(item))
			{
				if (!player.IsEquipActionQueued(item))
				{
					player.QueueEquipAction(item);
				}
			}
			else
			{
				player.EquipItem(item, triggerEquipEffects: false);
			}
		}
		else if ((!flag & flag2) && !StaysInGrid(item, SlotLayout.HomeKind(item)))
		{
			player.UnequipItem(item, triggerEquipEffects: false);
		}
	}

	internal static void OnSlotRightClick(Player player, SlotCell cell, ItemDrop.ItemData item)
	{
		if (item == null && CanManage(player))
		{
			ItemDrop.ItemData itemData = MirrorOf(player, cell);
			if (itemData != null)
			{
				player.RemoveEquipAction(itemData);
				player.UnequipItem(itemData);
			}
		}
		else
		{
			if (item == null || !CanManage(player))
			{
				return;
			}
			if (cell.Kind == SlotKind.Quick || cell.Kind == SlotKind.Quiver || cell.Kind == SlotKind.Food)
			{
				player.UseItem(((Humanoid)player).m_inventory, item, fromInventoryGui: true);
				return;
			}
			Vector2i gridPos = SlotInventory.FreeGridCellPreferBag(((Humanoid)player).m_inventory);
			if (gridPos.x < 0)
			{
				player.Message(MessageHud.MessageType.Center, "$cdev_es_nospace");
				return;
			}
			using (BusyScope.Enter())
			{
				if (player.IsItemEquiped(item))
				{
					player.RemoveEquipAction(item);
					player.UnequipItem(item);
				}
				item.m_gridPos = gridPos;
				item.m_equipped = false;
			}
			((Humanoid)player).m_inventory.Changed(false, false);
		}
	}

	internal static bool AllowToggle(Player player, ItemDrop.ItemData item)
	{
		if (!IsLocal(player) || item == null || !player.IsItemEquiped(item) || !SlotInventory.InOwnCell(item))
		{
			return true;
		}
		if (SlotInventory.FreeGridCellPreferBag(((Humanoid)player).m_inventory).x >= 0)
		{
			return true;
		}
		player.Message(MessageHud.MessageType.Center, "$cdev_es_nospace");
		return false;
	}

	internal static void Tick()
	{
		Player localPlayer = Player.m_localPlayer;
		if (Busy || !CanManage(localPlayer) || (!s_dirty && Time.time < s_nextPass))
		{
			return;
		}
		s_dirty = false;
		s_nextPass = Time.time + 1f;
		try
		{
			if (Repair(localPlayer))
			{
				((Humanoid)localPlayer).m_inventory.Changed(false, false);
			}
		}
		catch (Exception arg)
		{
			Plugin.Log.LogError($"Slot pass failed: {arg}");
		}
		s_dirty = false;
	}

	private static bool Repair(Player player)
	{
		Inventory inventory = ((Humanoid)player).m_inventory;
		bool flag = false;
		if (s_refusedFor != player)
		{
			Refused.Clear();
			ReturnCells.Clear();
			s_refusedFor = player;
		}
		using (BusyScope.Enter())
		{
			Items.Clear();
			Items.AddRange(inventory.m_inventory);
			foreach (ItemDrop.ItemData item in Items)
			{
				flag |= SlotMemory.Restore(player, inventory, item);
			}
			flag |= SlotMemory.RestoreCells(player, inventory, Items);
			foreach (ItemDrop.ItemData item2 in Items)
			{
				if (item2.m_equipped && Jewelry.IsJewelry(item2) && !SlotInventory.InOwnCell(item2))
				{
					item2.m_equipped = false;
					flag = true;
				}
			}
			foreach (ItemDrop.ItemData item3 in Items)
			{
				SlotKind slotKind = SlotLayout.HomeKind(item3);
				if (slotKind != SlotKind.None && !SlotInventory.InOwnCell(item3) && player.IsItemEquiped(item3) && !StaysInGrid(item3, slotKind))
				{
					flag |= PlaceIntoSlot(inventory, item3, slotKind);
				}
			}
			foreach (ItemDrop.ItemData item4 in Items)
			{
				SlotCell slotCell = SlotLayout.At(item4.m_gridPos);
				if (slotCell != null && slotCell.Kind != SlotKind.Stash && !SlotLayout.Accepts(slotCell, item4))
				{
					if (player.IsItemEquiped(item4))
					{
						player.UnequipItem(item4, triggerEquipEffects: false);
					}
					if (!player.IsItemEquiped(item4))
					{
						item4.m_equipped = false;
					}
					if (SlotInventory.MoveToGridOrStash(inventory, item4))
					{
						flag = true;
					}
				}
			}
			foreach (SlotCell item5 in SlotLayout.Equipment)
			{
				ItemDrop.ItemData itemData = SlotInventory.ItemIn(inventory, item5);
				if (itemData != null && !itemData.m_equipped && SlotLayout.Accepts(item5, itemData) && !player.IsItemEquiped(itemData) && !player.IsEquipActionQueued(itemData) && (!itemData.m_shared.m_useDurability || !(itemData.m_durability <= 0f)) && (!Refused.TryGetValue(itemData, out var value) || !(value == itemData.m_gridPos)) && !player.InAttack() && !player.InDodge() && (!player.IsSwimming() || player.IsOnGround()))
				{
					if (player.EquipItem(itemData, triggerEquipEffects: false))
					{
						Refused.Remove(itemData);
					}
					else
					{
						Refused[itemData] = itemData.m_gridPos;
					}
				}
			}
			foreach (SlotCell item6 in SlotLayout.Equipment)
			{
				ItemDrop.ItemData itemData2 = SlotInventory.ItemIn(inventory, item6);
				if (itemData2 != null && StaysInGrid(itemData2, item6.Kind) && player.IsItemEquiped(itemData2))
				{
					Vector2i gridPos = ((ReturnCells.TryGetValue(itemData2, out var value2) && SlotInventory.InGrid(inventory, value2) && inventory.GetItemAt(value2.x, value2.y) == null) ? value2 : SlotInventory.FreeGridCellPreferBag(inventory));
					ReturnCells.Remove(itemData2);
					if (gridPos.x >= 0)
					{
						itemData2.m_gridPos = gridPos;
						flag = true;
					}
				}
			}
			if (ReturnCells.Count > 32)
			{
				ReturnCells.Clear();
			}
			foreach (SlotCell item7 in SlotLayout.Stash)
			{
				ItemDrop.ItemData itemData3 = SlotInventory.ItemIn(inventory, item7);
				if (itemData3 != null)
				{
					Vector2i gridPos2 = SlotInventory.FreeGridCellPreferBag(inventory);
					if (gridPos2.x < 0)
					{
						break;
					}
					itemData3.m_gridPos = gridPos2;
					flag = true;
				}
			}
			Items.Clear();
			return flag;
		}
	}
}
