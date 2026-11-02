using HarmonyLib;

namespace BetterEquipSlots.EquipSlots;

[HarmonyPatch]
internal static class AmmoRefill
{
	private static string s_ranOutName;

	private static string s_ranOutType;

	[HarmonyPatch(typeof(Attack), "UseAmmo")]
	[HarmonyPostfix]
	private static void UseAmmo(Attack __instance, ref ItemDrop.ItemData ammoItem, bool __result)
	{
		if (Plugin.Active && __result && ammoItem != null && ModConfig.AutoEquipAmmo.Value && __instance.m_character is Player player && !(player != Player.m_localPlayer) && (!player.GetInventory().ContainsItem(ammoItem) || ammoItem.m_stack <= 0))
		{
			s_ranOutName = ammoItem.m_shared.m_name;
			s_ranOutType = ammoItem.m_shared.m_ammoType;
		}
	}

	internal static void Tick()
	{
		if (s_ranOutType == null)
		{
			return;
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null || localPlayer.IsDead())
		{
			s_ranOutType = null;
		}
		else
		{
			if (localPlayer.InAttack())
			{
				return;
			}
			string text = s_ranOutName;
			string text2 = s_ranOutType;
			s_ranOutName = (s_ranOutType = null);
			ItemDrop.ItemData ammoItem = localPlayer.GetAmmoItem();
			if (ammoItem != null && localPlayer.GetInventory().ContainsItem(ammoItem) && ammoItem.m_shared.m_ammoType == text2)
			{
				return;
			}
			Inventory inventory = localPlayer.GetInventory();
			ItemDrop.ItemData itemData = null;
			foreach (ItemDrop.ItemData item in inventory.m_inventory)
			{
				if (item.m_shared.m_name == text && item.m_shared.m_ammoType == text2)
				{
					itemData = item;
					break;
				}
			}
			itemData = itemData ?? inventory.GetAmmoItem(text2);
			if (itemData != null && (itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Ammo || itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.AmmoNonEquipable))
			{
				localPlayer.EquipItem(itemData);
			}
		}
	}
}
