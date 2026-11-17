using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

/// <summary>
/// Named sets of worn gear and quick slot items, saved on the character (so they travel with it) and put back on with a key.
/// A set remembers each slot's item by prefab and quality, and applying it finds those items wherever they are in the
/// inventory, swapping what is worn out.
/// </summary>
internal static class Loadouts
{
	internal const int Count = 4;

	private const string KeyPrefix = "BES_loadout_";

	// v2 also holds what is in the hands (cells 1000 and 1001 below); v1 left the hands alone
	private const string Version = "v2";

	private const int RightHand = 1000;

	private const int LeftHand = 1001;

	private struct Entry
	{
		internal int Cell;

		internal string Prefab;

		internal int Quality;
	}

	internal static string PrefabOf(ItemDrop.ItemData item)
	{
		if (item == null)
		{
			return null;
		}
		return item.m_dropPrefab != null ? item.m_dropPrefab.name : item.m_shared?.m_name;
	}

	// The cells a loadout covers: the worn slots (not the backpack, which its own mod manages, nor the quiver's contents) and the quick slots
	private static List<SlotCell> Managed()
	{
		List<SlotCell> list = new List<SlotCell>();
		foreach (SlotCell cell in SlotLayout.Equipment)
		{
			if (cell.Kind == SlotKind.Backpack || !SlotLayout.IsOpen(cell))
			{
				continue;
			}
			if (cell.Kind == SlotKind.Ammo && BetterArcheryHost.QuiverActive)
			{
				continue;
			}
			list.Add(cell);
		}
		list.AddRange(SlotLayout.Quick);
		return list;
	}

	private static string StoreKey(int number) => KeyPrefix + number;

	internal static bool Exists(Player player, int number)
	{
		return player?.m_customData != null && player.m_customData.ContainsKey(StoreKey(number));
	}

	private static List<Entry> Read(Player player, int number)
	{
		List<Entry> list = new List<Entry>();
		if (!Exists(player, number))
		{
			return list;
		}
		string[] parts = player.m_customData[StoreKey(number)].Split(';');
		for (int i = 1; i < parts.Length; i++)
		{
			string[] f = parts[i].Split(':');
			if (f.Length == 3 && int.TryParse(f[0], out int cell) && int.TryParse(f[2], out int quality) && cell >= 0 && (cell < SlotLayout.All.Count || cell == RightHand || cell == LeftHand))
			{
				list.Add(new Entry { Cell = cell, Prefab = f[1], Quality = quality });
			}
		}
		return list;
	}

	internal static void Save(Player player, int number)
	{
		Inventory inventory = ((Humanoid)player).m_inventory;
		StringBuilder sb = new StringBuilder(Version);
		int saved = 0;
		foreach (SlotCell cell in Managed())
		{
			ItemDrop.ItemData item = SlotInventory.ItemIn(inventory, cell);
			string prefab = PrefabOf(item);
			if (prefab != null && prefab.IndexOfAny(new[] { ';', ':' }) < 0)
			{
				sb.Append(';').Append(cell.Index).Append(':').Append(prefab).Append(':').Append(item.m_quality);
				saved++;
			}
		}
		if (MainConfig.loadoutWeapons.Value)
		{
			AppendHand(sb, RightHand, ((Humanoid)player).m_rightItem, ref saved);
			AppendHand(sb, LeftHand, ((Humanoid)player).m_leftItem, ref saved);
		}
		else if (Exists(player, number) && player.m_customData[StoreKey(number)].StartsWith("v2"))
		{
			// Keep the hands the loadout already holds
			foreach (Entry entry in Read(player, number))
			{
				if (entry.Cell == RightHand || entry.Cell == LeftHand)
				{
					sb.Append(';').Append(entry.Cell).Append(':').Append(entry.Prefab).Append(':').Append(entry.Quality);
				}
			}
		}
		player.m_customData[StoreKey(number)] = sb.ToString();
		player.Message(MessageHud.MessageType.Center, Texts.Get(Texts.LoadoutSaved, number.ToString(), saved.ToString()));
	}

	private static void AppendHand(StringBuilder sb, int cell, ItemDrop.ItemData item, ref int saved)
	{
		string prefab = PrefabOf(item);
		if (prefab != null && prefab.IndexOfAny(new[] { ';', ':' }) < 0)
		{
			sb.Append(';').Append(cell).Append(':').Append(prefab).Append(':').Append(item.m_quality);
			saved++;
		}
	}

	private static bool Matches(ItemDrop.ItemData item, Entry entry)
	{
		return item?.m_shared != null && PrefabOf(item) == entry.Prefab;
	}

	private static bool Usable(ItemDrop.ItemData item)
	{
		return !item.m_shared.m_useDurability || item.m_durability > 0f;
	}

	// The best item for an entry among those not yet taken: the same quality first, then the one in best repair
	private static ItemDrop.ItemData Find(List<ItemDrop.ItemData> items, HashSet<ItemDrop.ItemData> taken, HashSet<SlotCell> managed, SlotCell cell, Entry entry)
	{
		ItemDrop.ItemData best = null;
		float bestScore = float.MinValue;
		foreach (ItemDrop.ItemData item in items)
		{
			if (taken.Contains(item) || !Matches(item, entry) || !SlotLayout.Accepts(cell, item) || !Usable(item))
			{
				continue;
			}
			SlotCell at = SlotLayout.At(item.m_gridPos);
			if (at != null && !managed.Contains(at))
			{
				continue;
			}
			float score = (item.m_quality == entry.Quality ? 1000f : 0f) + item.m_quality + (item.m_shared.m_useDurability ? item.GetDurabilityPercentage() : 1f);
			if (score > bestScore)
			{
				best = item;
				bestScore = score;
			}
		}
		return best;
	}

	internal static void Apply(Player player, int number)
	{
		if (!Exists(player, number))
		{
			player.Message(MessageHud.MessageType.Center, Texts.Get(Texts.LoadoutEmpty, number.ToString(), QuickSlots.Label(MainConfig.loadoutSave.Value)));
			return;
		}
		Inventory inventory = ((Humanoid)player).m_inventory;
		List<SlotCell> cells = Managed();
		HashSet<SlotCell> managed = new HashSet<SlotCell>(cells);
		Dictionary<SlotCell, Entry> wanted = new Dictionary<SlotCell, Entry>();
		Entry? rightHand = null;
		Entry? leftHand = null;
		foreach (Entry entry in Read(player, number))
		{
			if (entry.Cell == RightHand)
			{
				rightHand = entry;
				continue;
			}
			if (entry.Cell == LeftHand)
			{
				leftHand = entry;
				continue;
			}
			SlotCell cell = SlotLayout.All[entry.Cell];
			if (managed.Contains(cell))
			{
				wanted[cell] = entry;
			}
		}
		List<ItemDrop.ItemData> items = new List<ItemDrop.ItemData>(inventory.m_inventory);
		HashSet<ItemDrop.ItemData> taken = new HashSet<ItemDrop.ItemData>();
		Dictionary<SlotCell, ItemDrop.ItemData> plan = new Dictionary<SlotCell, ItemDrop.ItemData>();
		int missing = 0;
		int failed = 0;

		// Items already in the right cell stay
		foreach (KeyValuePair<SlotCell, Entry> pair in wanted)
		{
			ItemDrop.ItemData current = SlotInventory.ItemIn(inventory, pair.Key);
			if (current != null && Matches(current, pair.Value) && !taken.Contains(current))
			{
				plan[pair.Key] = current;
				taken.Add(current);
			}
		}
		// The others are found in the inventory (or in another slot)
		foreach (KeyValuePair<SlotCell, Entry> pair in wanted)
		{
			if (plan.ContainsKey(pair.Key))
			{
				continue;
			}
			ItemDrop.ItemData found = Find(items, taken, managed, pair.Key, pair.Value);
			if (found == null)
			{
				missing++;
				continue;
			}
			plan[pair.Key] = found;
			taken.Add(found);
		}

		using (EquipSync.BusyScope.Enter())
		{
			foreach (KeyValuePair<SlotCell, ItemDrop.ItemData> pair in plan)
			{
				SlotCell cell = pair.Key;
				ItemDrop.ItemData item = pair.Value;
				ItemDrop.ItemData current = SlotInventory.ItemIn(inventory, cell);
				if (current == item)
				{
					continue;
				}
				Vector2i from = item.m_gridPos;
				if (current != null)
				{
					// The item that was there takes the place of the one coming in, so no free cell is needed
					SlotCell fromCell = SlotLayout.At(from);
					bool fits = fromCell == null ? SlotInventory.InGrid(inventory, from) : (fromCell.Kind != SlotKind.Stash && SlotLayout.Accepts(fromCell, current));
					Unequip(player, current);
					item.m_gridPos = new Vector2i(-1000, -1000);
					if (fits)
					{
						current.m_gridPos = from;
					}
					else if (!SlotInventory.MoveToGridOrStash(inventory, current))
					{
						item.m_gridPos = from;
						failed++;
						continue;
					}
				}
				item.m_gridPos = cell.Pos;
			}
			// A worn slot the loadout leaves empty is emptied
			foreach (SlotCell cell in cells)
			{
				if (cell.Kind == SlotKind.Quick || wanted.ContainsKey(cell))
				{
					continue;
				}
				ItemDrop.ItemData current = SlotInventory.ItemIn(inventory, cell);
				if (current == null)
				{
					continue;
				}
				Unequip(player, current);
				if (!SlotInventory.MoveToGridOrStash(inventory, current))
				{
					failed++;
				}
			}
		}
		if (MainConfig.loadoutWeapons.Value && player.m_customData[StoreKey(number)].StartsWith("v2"))
		{
			missing += ApplyHands(player, items, taken, rightHand, leftHand);
		}
		inventory.Changed(false, false);
		EquipSync.MarkDirty();

		string message = Texts.Get(Texts.LoadoutApplied, number.ToString());
		if (missing > 0)
		{
			message += Texts.Get(Texts.LoadoutMissing, missing.ToString());
		}
		if (failed > 0)
		{
			message += Texts.Get(Texts.LoadoutFull, failed.ToString());
		}
		player.Message(MessageHud.MessageType.Center, message);
	}

	/// <summary>A click on a loadout button of the panel: put the loadout on, or save to it.</summary>
	internal static void OnPanelClick(int number, bool save)
	{
		Player player = Player.m_localPlayer;
		if (player == null || player.IsDead() || player.m_isLoading)
		{
			return;
		}
		if (save)
		{
			Save(player, number);
		}
		else
		{
			Apply(player, number);
		}
	}

	// The hands: what is held now and not wanted is put away, then the wanted items are taken up; returns how many were not found
	private static int ApplyHands(Player player, List<ItemDrop.ItemData> items, HashSet<ItemDrop.ItemData> taken, Entry? right, Entry? left)
	{
		Humanoid humanoid = player;
		int missing = 0;
		ItemDrop.ItemData wantRight = FindHand(items, taken, right);
		ItemDrop.ItemData wantLeft = FindHand(items, taken, left);
		if (right.HasValue && wantRight == null)
		{
			missing++;
		}
		if (left.HasValue && wantLeft == null)
		{
			missing++;
		}
		// A hand whose item is missing keeps what it holds
		bool keepRight = right.HasValue && wantRight == null;
		bool keepLeft = left.HasValue && wantLeft == null;
		if (humanoid.m_rightItem != null && !keepRight && humanoid.m_rightItem != wantRight)
		{
			humanoid.UnequipItem(humanoid.m_rightItem, true);
		}
		if (humanoid.m_leftItem != null && !keepLeft && humanoid.m_leftItem != wantLeft)
		{
			humanoid.UnequipItem(humanoid.m_leftItem, true);
		}
		if (wantRight != null && humanoid.m_rightItem != wantRight)
		{
			humanoid.EquipItem(wantRight, true);
		}
		if (wantLeft != null && humanoid.m_leftItem != wantLeft)
		{
			humanoid.EquipItem(wantLeft, true);
		}
		return missing;
	}

	private static ItemDrop.ItemData FindHand(List<ItemDrop.ItemData> items, HashSet<ItemDrop.ItemData> taken, Entry? entry)
	{
		if (!entry.HasValue)
		{
			return null;
		}
		ItemDrop.ItemData best = null;
		float bestScore = float.MinValue;
		foreach (ItemDrop.ItemData item in items)
		{
			if (taken.Contains(item) || !Matches(item, entry.Value) || !Usable(item) || SlotLayout.At(item.m_gridPos) != null)
			{
				continue;
			}
			float score = (item.m_quality == entry.Value.Quality ? 1000f : 0f) + item.m_quality + (item.m_equipped ? 0.5f : 0f) + (item.m_shared.m_useDurability ? item.GetDurabilityPercentage() : 1f);
			if (score > bestScore)
			{
				best = item;
				bestScore = score;
			}
		}
		if (best != null)
		{
			taken.Add(best);
		}
		return best;
	}

	private static void Unequip(Player player, ItemDrop.ItemData item)
	{
		if (player.IsItemEquiped(item))
		{
			player.RemoveEquipAction(item);
			player.UnequipItem(item, triggerEquipEffects: false);
		}
		item.m_equipped = false;
	}

	private static KeyboardShortcut WithSave(KeyboardShortcut key)
	{
		List<KeyCode> modifiers = new List<KeyCode>(key.Modifiers);
		modifiers.AddRange(MainConfig.loadoutSave.Value.MainKey != KeyCode.None ? new[] { MainConfig.loadoutSave.Value.MainKey } : new KeyCode[0]);
		return new KeyboardShortcut(key.MainKey, modifiers.ToArray());
	}

	internal static void Tick()
	{
		Player player = Player.m_localPlayer;
		if (!MainConfig.loadoutsEnabled.Value || player == null || ZInput.instance == null || !((Character)player).TakeInput() || Hud.InRadial() || player.IsDead() || player.m_isLoading)
		{
			return;
		}
		for (int i = 1; i <= Count; i++)
		{
			KeyboardShortcut key = MainConfig.LoadoutKey(i);
			if (key.MainKey == KeyCode.None)
			{
				continue;
			}
			// Saving is the key with the Save modifier held as well, checked first so it never also applies
			if (MainConfig.loadoutSave.Value.MainKey != KeyCode.None && QuickSlots.ShortcutDown(WithSave(key)))
			{
				Save(player, i);
				return;
			}
			if (QuickSlots.ShortcutDown(key) && !HeldSave())
			{
				Apply(player, i);
				return;
			}
		}
	}

	private static bool HeldSave()
	{
		KeyCode save = MainConfig.loadoutSave.Value.MainKey;
		return save != KeyCode.None && QuickSlots.Readable(save) && ZInput.GetKey(save, logWarning: false);
	}
}
