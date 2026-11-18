using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

[HarmonyPatch]
internal static class Appearance
{
	internal const string Key = "CDEV_EquipmentSlots_look";

	internal static readonly SlotKind[] Kinds = new SlotKind[4]
	{
		SlotKind.Helmet,
		SlotKind.Chest,
		SlotKind.Legs,
		SlotKind.Cape
	};

	internal const string HiddenValue = "-";

	private const int GhostRank = 2;

	private static readonly Dictionary<string, GameObject> RealByName = new Dictionary<string, GameObject>();

	private static ObjectDB s_realDb;

	private static int s_realItems = -1;

	private static int s_realRecipes = -1;

	private static readonly HashSet<GameObject> Crafted = new HashSet<GameObject>();

	internal static int IndexOf(SlotKind kind)
	{
		return Array.IndexOf(Kinds, kind);
	}

	internal static GameObject Chosen(Player player, int index)
	{
		string text = ChosenName(player, index);
		GameObject gameObject = ((text != null && text != "-" && ObjectDB.instance != null) ? ObjectDB.instance.GetItemPrefab(text) : null);
		GameObject gameObject2 = Real(gameObject);
		if (!(gameObject != null) || Rank(gameObject) != 2)
		{
			return gameObject;
		}
		return gameObject2 ?? gameObject;
	}

	// The whole look as it is saved ("helmet|chest|legs|cape", empty: none chosen), and put back
	internal static string Snapshot(Player player)
	{
		return (player?.m_customData != null && player.m_customData.TryGetValue(Key, out var value)) ? value : "";
	}

	internal static void Restore(Player player, string value)
	{
		if (player?.m_customData == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(value) || value.Replace("|", "").Length == 0)
		{
			player.m_customData.Remove(Key);
		}
		else
		{
			player.m_customData[Key] = value;
		}
		if (((Humanoid)player).m_visEquipment != null)
		{
			((Humanoid)player).SetupVisEquipment(((Humanoid)player).m_visEquipment, false);
		}
	}

	internal static bool IsHidden(Player player, int index)
	{
		return ChosenName(player, index) == "-";
	}

	private static string ChosenName(Player player, int index)
	{
		if (player?.m_customData == null || index < 0 || !player.m_customData.TryGetValue("CDEV_EquipmentSlots_look", out var value))
		{
			return null;
		}
		string[] array = value.Split(new char[1] { '|' });
		if (index >= array.Length || array[index].Length <= 0)
		{
			return null;
		}
		return array[index];
	}

	internal static void Set(Player player, int index, string prefab)
	{
		if (!(player == null) && index >= 0 && index < Kinds.Length)
		{
			string[] array = new string[Kinds.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = ChosenName(player, i) ?? "";
			}
			array[index] = prefab ?? "";
			string text = string.Join("|", array);
			if (text.Replace("|", "").Length == 0)
			{
				player.m_customData.Remove("CDEV_EquipmentSlots_look");
			}
			else
			{
				player.m_customData["CDEV_EquipmentSlots_look"] = text;
			}
			if (((Humanoid)player).m_visEquipment != null)
			{
				((Humanoid)player).SetupVisEquipment(((Humanoid)player).m_visEquipment, false);
			}
		}
	}

	internal static SlotKind KindOf(ItemDrop.ItemData item)
	{
		return item?.m_shared?.m_itemType switch
		{
			ItemDrop.ItemData.ItemType.Helmet => SlotKind.Helmet, 
			ItemDrop.ItemData.ItemType.Chest => SlotKind.Chest, 
			ItemDrop.ItemData.ItemType.Legs => SlotKind.Legs, 
			ItemDrop.ItemData.ItemType.Shoulder => SlotKind.Cape, 
			_ => SlotKind.None, 
		};
	}

	internal static List<GameObject> Known(Player player, SlotKind kind, List<GameObject> into)
	{
		into.Clear();
		ObjectDB instance = ObjectDB.instance;
		if (player == null || instance == null)
		{
			return into;
		}
		HashSet<string> hashSet = new HashSet<string>();
		foreach (GameObject item in instance.m_items)
		{
			ItemDrop itemDrop = ((item != null) ? item.GetComponent<ItemDrop>() : null);
			ItemDrop.ItemData.SharedData sharedData = ((itemDrop != null) ? itemDrop.m_itemData.m_shared : null);
			if (sharedData != null && KindOf(itemDrop.m_itemData) == kind && sharedData.m_icons != null && sharedData.m_icons.Length != 0 && player.m_knownMaterial.Contains(sharedData.m_name) && hashSet.Add(sharedData.m_name))
			{
				into.Add(Real(item) ?? item);
			}
		}
		return into;
	}

	private static GameObject Real(GameObject prefab)
	{
		ItemDrop itemDrop = ((prefab != null) ? prefab.GetComponent<ItemDrop>() : null);
		if (itemDrop == null || ObjectDB.instance == null)
		{
			return null;
		}
		ObjectDB instance = ObjectDB.instance;
		if (instance != s_realDb || instance.m_items.Count != s_realItems || instance.m_recipes.Count != s_realRecipes)
		{
			s_realDb = instance;
			s_realItems = instance.m_items.Count;
			s_realRecipes = instance.m_recipes.Count;
			Crafted.Clear();
			foreach (Recipe recipe in instance.m_recipes)
			{
				if (recipe != null && recipe.m_item != null)
				{
					Crafted.Add(recipe.m_item.gameObject);
				}
			}
			RealByName.Clear();
			foreach (GameObject item in instance.m_items)
			{
				string text = ((item != null) ? item.GetComponent<ItemDrop>() : null)?.m_itemData?.m_shared?.m_name;
				if (text != null && (!RealByName.TryGetValue(text, out var value) || Rank(item) < Rank(value)))
				{
					RealByName[text] = item;
				}
			}
		}
		if (!RealByName.TryGetValue(itemDrop.m_itemData.m_shared.m_name, out var value2))
		{
			return prefab;
		}
		return value2;
	}

	private static int Rank(GameObject prefab)
	{
		if (Crafted.Contains(prefab))
		{
			return 0;
		}
		string name = prefab.name;
		if (!name.StartsWith("FW_", StringComparison.Ordinal) && !name.StartsWith("SP_", StringComparison.Ordinal))
		{
			return 1;
		}
		return 2;
	}

	internal static bool TakeLookOf(Player player, ItemDrop.ItemData item)
	{
		int num = IndexOf(KindOf(item));
		if (player == null || num < 0 || item.m_dropPrefab == null)
		{
			return false;
		}
		Set(player, num, item.m_dropPrefab.name);
		return true;
	}

	internal static Sprite Icon(GameObject prefab)
	{
		ItemDrop itemDrop = ((prefab != null) ? prefab.GetComponent<ItemDrop>() : null);
		Sprite[] array = ((itemDrop != null) ? itemDrop.m_itemData.m_shared.m_icons : null);
		if (array == null || array.Length == 0)
		{
			return null;
		}
		return array[0];
	}

	internal static string DisplayName(GameObject prefab)
	{
		ItemDrop itemDrop = ((prefab != null) ? prefab.GetComponent<ItemDrop>() : null);
		if (!(itemDrop != null))
		{
			return "";
		}
		return Localization.instance.Localize(itemDrop.m_itemData.m_shared.m_name);
	}

	[HarmonyPatch(typeof(Humanoid), "SetupVisEquipment")]
	[HarmonyPostfix]
	private static void Dress(Humanoid __instance, VisEquipment visEq)
	{
		if (!Plugin.Active || visEq == null || !(__instance is Player { m_customData: not null } player) || !player.m_customData.ContainsKey("CDEV_EquipmentSlots_look"))
		{
			return;
		}
		try
		{
			for (int i = 0; i < Kinds.Length; i++)
			{
				GameObject gameObject = Chosen(player, i);
				bool flag = gameObject == null && IsHidden(player, i);
				if (!(gameObject == null) || flag)
				{
					int num = ((!flag) ? gameObject.name.GetStableHashCode() : 0);
					switch (Kinds[i])
					{
					case SlotKind.Helmet:
						visEq.SetHelmetItem(num);
						break;
					case SlotKind.Chest:
						visEq.SetChestItem(num);
						break;
					case SlotKind.Legs:
						visEq.SetLegItem(num);
						break;
					default:
						visEq.SetShoulderItem(num, 0, 1);
						break;
					}
				}
			}
		}
		catch (Exception arg)
		{
			Plugin.Log.LogError($"Dressing the chosen appearance failed: {arg}");
		}
	}
}
