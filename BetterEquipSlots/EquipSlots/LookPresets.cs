namespace BetterEquipSlots.EquipSlots;

/// <summary>
/// Four saved styles (how the helmet, chest, legs and cape look, hidden pieces included), kept on the character beside the
/// loadouts. They are put on from the buttons of the Style tab, whatever gear is worn, and a loadout can remember its style too
/// (Loadouts → Include Style).
/// </summary>
internal static class LookPresets
{
	private const string KeyPrefix = "BES_look_";

	private const string Version = "L:";

	private static string StoreKey(int number) => KeyPrefix + number;

	internal static bool Exists(Player player, int number)
	{
		return player?.m_customData != null && player.m_customData.ContainsKey(StoreKey(number));
	}

	internal static void Save(Player player, int number)
	{
		player.m_customData[StoreKey(number)] = Version + Appearance.Snapshot(player);
		player.Message(MessageHud.MessageType.Center, Texts.Get(Texts.StyleSaved, number.ToString()));
	}

	internal static void Apply(Player player, int number)
	{
		if (!Exists(player, number))
		{
			player.Message(MessageHud.MessageType.Center, Texts.Get(Texts.StyleEmpty, number.ToString()));
			return;
		}
		string value = player.m_customData[StoreKey(number)];
		Appearance.Restore(player, value.StartsWith(Version) ? value.Substring(Version.Length) : value);
		player.Message(MessageHud.MessageType.Center, Texts.Get(Texts.StyleApplied, number.ToString()));
	}

	/// <summary>A click on a style button of the panel: use the style, or save the current one to it.</summary>
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
}
