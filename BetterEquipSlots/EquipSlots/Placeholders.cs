using System.Collections.Generic;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

internal static class Placeholders
{
	internal static readonly Color Tint = new Color(0.55f, 0.55f, 0.55f, 0.3f);

	private static readonly Dictionary<SlotKind, Sprite> Cache = new Dictionary<SlotKind, Sprite>();

	private static ObjectDB s_cachedFor;

	internal static Sprite Get(SlotKind kind)
	{
		ObjectDB instance = ObjectDB.instance;
		if (instance == null)
		{
			return null;
		}
		if (s_cachedFor != instance)
		{
			Cache.Clear();
			s_cachedFor = instance;
		}
		if (!Cache.TryGetValue(kind, out var value))
		{
			string text = PrefabName(kind);
			GameObject gameObject = ((text != null) ? instance.GetItemPrefab(text) : null);
			ItemDrop itemDrop = ((gameObject != null) ? gameObject.GetComponent<ItemDrop>() : null);
			Sprite[] array = ((itemDrop != null) ? itemDrop.m_itemData.m_shared.m_icons : null);
			value = ((array != null && array.Length != 0) ? array[0] : null);
			Cache[kind] = value;
		}
		return value;
	}

	private static string PrefabName(SlotKind kind)
	{
		switch (kind)
		{
		case SlotKind.Helmet:
			return "HelmetLeather";
		case SlotKind.Chest:
			return "ArmorLeatherChest";
		case SlotKind.Legs:
			return "ArmorLeatherLegs";
		case SlotKind.Cape:
			return "CapeDeerHide";
		case SlotKind.Belt:
			return "BeltStrength";
		case SlotKind.Trinket:
			return "TrinketBronzeHealth";
		case SlotKind.Ring:
			return "Ruby";
		case SlotKind.Earring:
			return "AmberPearl";
		case SlotKind.Amulet:
			return "SilverNecklace";
		case SlotKind.Ammo:
		case SlotKind.Quiver:
			return "ArrowWood";
		case SlotKind.Backpack:
			return BackpackHost.PlaceholderPrefab();
		default:
			return null;
		}
	}

	internal static string NameKey(SlotKind kind)
	{
		return kind switch
		{
			SlotKind.Helmet => "$cdev_es_slot_helmet", 
			SlotKind.Chest => "$cdev_es_slot_chest", 
			SlotKind.Legs => "$cdev_es_slot_legs", 
			SlotKind.Cape => "$cdev_es_slot_cape", 
			SlotKind.Belt => "$cdev_es_slot_belt", 
			SlotKind.Trinket => "$cdev_es_slot_trinket", 
			SlotKind.Ring => "$cdev_es_slot_ring", 
			SlotKind.Earring => "$cdev_es_slot_earring", 
			SlotKind.Amulet => "$cdev_es_slot_amulet", 
			SlotKind.Ammo => "$cdev_es_slot_ammo", 
			SlotKind.Quick => "$cdev_es_slot_quick", 
			SlotKind.Quiver => "$cdev_es_slot_quiver", 
			SlotKind.Backpack => "$cdev_es_slot_backpack", 
			_ => "$cdev_es_slot_stash", 
		};
	}
}
