using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BetterEquipSlots.EquipSlots;

internal static class Jewelry
{
	private static readonly Dictionary<string, SlotKind> FromConfig = new Dictionary<string, SlotKind>(StringComparer.Ordinal);

	private static readonly Dictionary<string, SlotKind> FromApi = new Dictionary<string, SlotKind>(StringComparer.Ordinal);

	private static readonly Dictionary<ItemDrop.ItemData.SharedData, SlotKind> Cache = new Dictionary<ItemDrop.ItemData.SharedData, SlotKind>();

	private static readonly ConditionalWeakTable<Humanoid, HashSet<StatusEffect>> Effects = new ConditionalWeakTable<Humanoid, HashSet<StatusEffect>>();

	private static readonly HashSet<StatusEffect> Wanted = new HashSet<StatusEffect>();

	private static readonly List<StatusEffect> Stale = new List<StatusEffect>();

	private static readonly List<ItemDrop.ItemData> Worn = new List<ItemDrop.ItemData>();

	internal static bool AnyKnown
	{
		get
		{
			if (FromApi.Count <= 0)
			{
				return FromConfig.Count > 0;
			}
			return true;
		}
	}

	internal static void ReloadConfig()
	{
		FromConfig.Clear();
		Read(ModConfig.Rings.Value, SlotKind.Ring);
		Read(ModConfig.Earrings.Value, SlotKind.Earring);
		Read(ModConfig.Amulets.Value, SlotKind.Amulet);
		Cache.Clear();
	}

	internal static void Register(string prefabName, SlotKind kind)
	{
		if (!string.IsNullOrWhiteSpace(prefabName) && SlotLayout.IsJewelryKind(kind))
		{
			FromApi[prefabName.Trim()] = kind;
			Cache.Clear();
		}
	}

	internal static void Unregister(string prefabName)
	{
		if (prefabName != null && FromApi.Remove(prefabName.Trim()))
		{
			Cache.Clear();
		}
	}

	private static void Read(string list, SlotKind kind)
	{
		if (string.IsNullOrEmpty(list))
		{
			return;
		}
		string[] array = list.Split(new char[2] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			string text2 = text.Trim();
			if (text2.Length > 0)
			{
				FromConfig[text2] = kind;
			}
		}
	}

	internal static SlotKind KindOf(ItemDrop.ItemData item)
	{
		if (item?.m_shared == null || item.m_dropPrefab == null)
		{
			return SlotKind.None;
		}
		if (Cache.TryGetValue(item.m_shared, out var value))
		{
			return value;
		}
		string name = item.m_dropPrefab.name;
		if (!FromConfig.TryGetValue(name, out value) && !FromApi.TryGetValue(name, out value))
		{
			value = SlotKind.None;
		}
		if (value != SlotKind.None && (item.IsEquipableVanilla() || item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable))
		{
			Plugin.Log.LogWarning($"{name} is listed as jewelry but is a {item.m_shared.m_itemType}; ignored.");
			value = SlotKind.None;
		}
		Cache[item.m_shared] = value;
		return value;
	}

	internal static bool IsJewelry(ItemDrop.ItemData item)
	{
		return KindOf(item) != SlotKind.None;
	}

	internal static bool IsWorn(Humanoid humanoid, ItemDrop.ItemData item)
	{
		if (item.m_equipped && IsJewelry(item) && SlotInventory.InOwnCell(item))
		{
			return humanoid.m_inventory.ContainsItem(item);
		}
		return false;
	}

	internal static void CollectWorn(Humanoid humanoid, List<ItemDrop.ItemData> worn)
	{
		worn.Clear();
		Inventory inventory = humanoid.m_inventory;
		if (inventory == null)
		{
			return;
		}
		foreach (SlotCell item in SlotLayout.Equipment)
		{
			if (SlotLayout.IsJewelryKind(item.Kind))
			{
				ItemDrop.ItemData itemData = SlotInventory.ItemIn(inventory, item);
				if (itemData != null && itemData.m_equipped && KindOf(itemData) == item.Kind)
				{
					worn.Add(itemData);
				}
			}
		}
	}

	internal static float Armor(Humanoid humanoid)
	{
		CollectWorn(humanoid, Worn);
		float num = 0f;
		foreach (ItemDrop.ItemData item in Worn)
		{
			num += item.GetArmor();
		}
		Worn.Clear();
		return num;
	}

	internal static void SyncStatusEffects(Humanoid humanoid)
	{
		if (((Character)humanoid).m_seman == null)
		{
			return;
		}
		HashSet<StatusEffect> orCreateValue = Effects.GetOrCreateValue(humanoid);
		CollectWorn(humanoid, Worn);
		Wanted.Clear();
		foreach (ItemDrop.ItemData item in Worn)
		{
			if (item.m_shared.m_equipStatusEffect != null)
			{
				Wanted.Add(item.m_shared.m_equipStatusEffect);
			}
		}
		Worn.Clear();
		Stale.Clear();
		foreach (StatusEffect item2 in orCreateValue)
		{
			if (!Wanted.Contains(item2))
			{
				Stale.Add(item2);
			}
		}
		foreach (StatusEffect item3 in Stale)
		{
			orCreateValue.Remove(item3);
			if (!humanoid.m_equipmentStatusEffects.Contains(item3))
			{
				((Character)humanoid).m_seman.RemoveStatusEffect(item3.NameHash(), quiet: true);
			}
		}
		foreach (StatusEffect item4 in Wanted)
		{
			orCreateValue.Add(item4);
			if (!((Character)humanoid).m_seman.HaveStatusEffect(item4.NameHash()))
			{
				((Character)humanoid).m_seman.AddStatusEffect(item4, resetTime: false, 0, 0f, -1);
			}
		}
		Stale.Clear();
		Wanted.Clear();
	}

	private static bool IsEquipableVanilla(this ItemDrop.ItemData item)
	{
		switch (item.m_shared.m_itemType)
		{
		case ItemDrop.ItemData.ItemType.OneHandedWeapon:
		case ItemDrop.ItemData.ItemType.Bow:
		case ItemDrop.ItemData.ItemType.Shield:
		case ItemDrop.ItemData.ItemType.Helmet:
		case ItemDrop.ItemData.ItemType.Chest:
		case ItemDrop.ItemData.ItemType.Ammo:
		case ItemDrop.ItemData.ItemType.Legs:
		case ItemDrop.ItemData.ItemType.TwoHandedWeapon:
		case ItemDrop.ItemData.ItemType.Torch:
		case ItemDrop.ItemData.ItemType.Shoulder:
		case ItemDrop.ItemData.ItemType.Utility:
		case ItemDrop.ItemData.ItemType.Tool:
		case ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft:
		case ItemDrop.ItemData.ItemType.Trinket:
			return true;
		default:
			return false;
		}
	}
}
