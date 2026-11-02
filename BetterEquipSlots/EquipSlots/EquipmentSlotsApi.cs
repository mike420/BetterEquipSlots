using System.Collections.Generic;

namespace BetterEquipSlots.EquipSlots;

public static class EquipmentSlotsApi
{
	public static bool RegisterJewelry(string prefabName, string kind)
	{
		SlotKind kind2;
		switch ((kind ?? "").Trim().ToLowerInvariant())
		{
		case "ring":
			kind2 = SlotKind.Ring;
			break;
		case "earring":
			kind2 = SlotKind.Earring;
			break;
		case "amulet":
		case "necklace":
			kind2 = SlotKind.Amulet;
			break;
		default:
			return false;
		}
		Jewelry.Register(prefabName, kind2);
		return true;
	}

	public static void GetWornJewelry(Humanoid humanoid, List<ItemDrop.ItemData> worn)
	{
		if (humanoid == null || worn == null)
		{
			worn?.Clear();
		}
		else
		{
			Jewelry.CollectWorn(humanoid, worn);
		}
	}
}
