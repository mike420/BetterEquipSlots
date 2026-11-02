using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace BetterEquipSlots.EquipSlots;

internal static class Locks
{
	internal const string DataKey = "cdev_es_locks";

	private static readonly HashSet<int> Cells = new HashSet<int>();

	private static Player s_for;

	internal static bool Any
	{
		get
		{
			EnsureLoaded(Player.m_localPlayer);
			return Cells.Count > 0;
		}
	}

	private static int Key(Vector2i pos)
	{
		return (pos.y << 8) | pos.x;
	}

	private static void EnsureLoaded(Player player)
	{
		if (player == null || (object)player == s_for)
		{
			return;
		}
		s_for = player;
		Cells.Clear();
		if (player.m_customData == null || !player.m_customData.TryGetValue("cdev_es_locks", out var value) || string.IsNullOrEmpty(value))
		{
			return;
		}
		string[] array = value.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		foreach (string text in array)
		{
			string[] array2 = text.Split(new char[1] { ',' });
			if (array2.Length == 2 && int.TryParse(array2[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) && int.TryParse(array2[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2) && result >= 0 && result < 256 && result2 >= 0)
			{
				Cells.Add(Key(new Vector2i(result, result2)));
			}
		}
	}

	internal static bool IsLocked(Vector2i pos)
	{
		if (pos.x < 0 || pos.y < 0)
		{
			return false;
		}
		EnsureLoaded(Player.m_localPlayer);
		if (Cells.Count > 0)
		{
			return Cells.Contains(Key(pos));
		}
		return false;
	}

	internal static void Toggle(Vector2i pos)
	{
		Player localPlayer = Player.m_localPlayer;
		if (!(localPlayer == null) && pos.x >= 0 && pos.y >= 0)
		{
			EnsureLoaded(localPlayer);
			int item = Key(pos);
			if (!Cells.Remove(item))
			{
				Cells.Add(item);
			}
			Save(localPlayer);
		}
	}

	private static void Save(Player player)
	{
		if (player.m_customData == null)
		{
			return;
		}
		if (Cells.Count == 0)
		{
			player.m_customData.Remove("cdev_es_locks");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int cell in Cells)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(';');
			}
			stringBuilder.Append(cell & 0xFF).Append(',').Append(cell >> 8);
		}
		player.m_customData["cdev_es_locks"] = stringBuilder.ToString();
	}
}
