using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

/// <summary>
/// The food bag: a few slots on the panel that take only food. They keep you fed by themselves (auto eat: a free food slot is
/// filled, and a food about to run out is eaten again) and a key eats from them right now. It uses the game's own eating, so
/// every rule of the game still holds (a food can be eaten again only after half of its time).
/// </summary>
internal static class FoodBag
{
	private static float s_next;

	// How many bag slots are open (the panel has room for six)
	internal static int Slots => Mathf.Clamp(MainConfig.foodSlots.Value, 0, 6);

	internal static bool IsFood(ItemDrop.ItemData item)
	{
		return item?.m_shared != null && item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable && (item.m_shared.m_food > 0f || item.m_shared.m_foodStamina > 0f || item.m_shared.m_foodEitr > 0f);
	}

	internal static void Setup()
	{
		MainConfig.foodSlots.SettingChanged += (_, _) => SlotPanel.Invalidate();
	}

	// The foods in the bag, in slot order
	private static List<ItemDrop.ItemData> Bag(Player player)
	{
		List<ItemDrop.ItemData> bag = new List<ItemDrop.ItemData>();
		foreach (SlotCell cell in SlotLayout.Food)
		{
			if (!SlotLayout.IsOpen(cell))
			{
				continue;
			}
			ItemDrop.ItemData item = SlotInventory.ItemIn(((Humanoid)player).m_inventory, cell);
			if (item != null && IsFood(item))
			{
				bag.Add(item);
			}
		}
		return bag;
	}

	private static bool Eating(List<Player.Food> foods, ItemDrop.ItemData item)
	{
		foreach (Player.Food food in foods)
		{
			if (food.m_item.m_shared.m_name == item.m_shared.m_name)
			{
				return true;
			}
		}
		return false;
	}

	// How much of its time a food has left (1: all of it)
	private static float Left(Player.Food food)
	{
		float burn = food.m_item.m_shared.m_foodBurnTime;
		return burn > 0f ? food.m_time / burn : 1f;
	}

	// A food of the bag that is not being eaten, and that the game lets you eat now
	private static ItemDrop.ItemData NewFood(Player player, List<ItemDrop.ItemData> bag, List<Player.Food> foods)
	{
		foreach (ItemDrop.ItemData item in bag)
		{
			if (!Eating(foods, item) && player.CanEat(item, false))
			{
				return item;
			}
		}
		return null;
	}

	private static ItemDrop.ItemData SameFood(Player player, List<ItemDrop.ItemData> bag, Player.Food food)
	{
		foreach (ItemDrop.ItemData item in bag)
		{
			if (item.m_shared.m_name == food.m_item.m_shared.m_name && player.CanEat(item, false))
			{
				return item;
			}
		}
		return null;
	}

	/// <summary>
	/// Eats once from the bag. With a free food slot, a food that is not being eaten; otherwise (or with none such) the food
	/// closest to running out is eaten again if the bag has it, else another food (as set). Auto eat does that only when a
	/// food is below Refresh Below; the key does it whenever.
	/// </summary>
	private static bool Eat(Player player, bool auto)
	{
		List<ItemDrop.ItemData> bag = Bag(player);
		if (bag.Count == 0)
		{
			return false;
		}
		List<Player.Food> foods = player.GetFoods();
		ItemDrop.ItemData choice = null;
		if (foods.Count < Player.m_maxFoods)
		{
			choice = NewFood(player, bag, foods);
		}
		float below = Mathf.Clamp(MainConfig.foodRefreshBelow.Value, 0, 50) / 100f;
		if (choice == null && foods.Count > 0 && (!auto || below > 0f))
		{
			Player.Food lowest = foods[0];
			foreach (Player.Food food in foods)
			{
				if (Left(food) < Left(lowest))
				{
					lowest = food;
				}
			}
			if (!auto || Left(lowest) <= below)
			{
				choice = SameFood(player, bag, lowest);
				if (choice == null && (!auto || MainConfig.foodUseAnother.Value))
				{
					choice = NewFood(player, bag, foods);
				}
			}
		}
		if (choice == null)
		{
			return false;
		}
		((Humanoid)player).UseItem(null, choice, false);
		return true;
	}

	internal static void Tick()
	{
		Player player = Player.m_localPlayer;
		if (player == null || player.IsDead() || player.m_isLoading || Slots == 0 || ZInput.instance == null)
		{
			return;
		}
		KeyboardShortcut key = MainConfig.foodKey.Value;
		if (key.MainKey != KeyCode.None && ((Character)player).TakeInput() && !Hud.InRadial() && QuickSlots.ShortcutDown(key))
		{
			if (!Eat(player, auto: false) && MainConfig.foodMessages.Value)
			{
				player.Message(MessageHud.MessageType.Center, Texts.Get(Texts.FoodNone));
			}
			return;
		}
		if (!MainConfig.foodAutoEat.Value || Time.time < s_next)
		{
			return;
		}
		s_next = Time.time + 1f;
		Eat(player, auto: true);
	}
}
