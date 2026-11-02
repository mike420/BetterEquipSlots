using System.Collections.Generic;
using System.Text;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace BetterEquipSlots.EquipSlots;

internal static class QuickSlots
{
	private struct Binding
	{
		internal KeyCode Main;

		internal KeyCode[] Modifiers;

		internal int Slot;
	}

	private static readonly List<Binding> Bindings = new List<Binding>();

	private static readonly bool[] Wanted = new bool[6];

	private static readonly List<KeyCode> FiredKeys = new List<KeyCode>();

	private static readonly List<bool> FiredWithModifiers = new List<bool>();

	private static int s_resolvedFrame = -1;

	private static bool s_bindingsDirty = true;

	private static readonly HashSet<KeyCode> Warned = new HashSet<KeyCode>();

	internal static void Invalidate()
	{
		s_bindingsDirty = true;
	}

	internal static bool AltHeld()
	{
		KeyCode[] array = ModConfig.AltKeys();
		if (array.Length != 0)
		{
			return AllHeld(array);
		}
		return false;
	}

	private static void BuildBindings()
	{
		s_bindingsDirty = false;
		Bindings.Clear();
		for (int i = 0; i < 6; i++)
		{
			Binding item = BindingOf(i);
			if (item.Main == KeyCode.None)
			{
				continue;
			}
			string text = Unreadable(ModConfig.SlotKey(i));
			if (text != null)
			{
				if (Warned.Add(item.Main))
				{
					Plugin.Log.LogWarning($"Quick slot {i + 1}: the key {text} cannot be read by the game's input system " + "and is ignored - choose another key for QuickSlots.Key" + (i + 1) + ".");
				}
			}
			else
			{
				Bindings.Add(item);
			}
		}
	}

	internal static bool Readable(KeyCode code)
	{
		KeyCode keyCode = ForZInput(code);
		if (keyCode == KeyCode.None)
		{
			return false;
		}
		if (!IsMouse(keyCode))
		{
			if (ZInput.TryKeyCodeToKey(keyCode, out Key result))
			{
				return result != Key.None;
			}
			return false;
		}
		return true;
	}

	internal static string Unreadable(KeyboardShortcut shortcut)
	{
		if (shortcut.MainKey != KeyCode.None && !Readable(shortcut.MainKey))
		{
			return shortcut.MainKey.ToString();
		}
		foreach (KeyCode modifier in shortcut.Modifiers)
		{
			if (!Readable(modifier))
			{
				return modifier.ToString();
			}
		}
		return null;
	}

	private static bool Down(KeyCode code)
	{
		if (Readable(code))
		{
			return ZInput.GetKeyDown(ForZInput(code), logWarning: false);
		}
		return false;
	}

	private static bool Held(KeyCode code)
	{
		if (Readable(code))
		{
			return ZInput.GetKey(ForZInput(code), logWarning: false);
		}
		return false;
	}

	private static Binding BindingOf(int ordinal)
	{
		KeyboardShortcut keyboardShortcut = ModConfig.SlotKey(ordinal);
		return new Binding
		{
			Main = keyboardShortcut.MainKey,
			Modifiers = new List<KeyCode>(keyboardShortcut.Modifiers).ToArray(),
			Slot = ordinal
		};
	}

	internal static bool IsMouse(KeyCode key)
	{
		if (key >= KeyCode.Mouse0)
		{
			return key <= KeyCode.Mouse6;
		}
		return false;
	}

	private static KeyCode ForZInput(KeyCode key)
	{
		return key switch
		{
			KeyCode.Mouse4 => KeyCode.Mouse3, 
			KeyCode.Mouse3 => KeyCode.Mouse4, 
			_ => key, 
		};
	}

	private static void Resolve()
	{
		if (s_resolvedFrame == Time.frameCount || Time.inFixedTimeStep)
		{
			return;
		}
		s_resolvedFrame = Time.frameCount;
		if (s_bindingsDirty)
		{
			BuildBindings();
		}
		for (int i = 0; i < Wanted.Length; i++)
		{
			Wanted[i] = false;
		}
		FiredKeys.Clear();
		FiredWithModifiers.Clear();
		if (ZInput.instance == null)
		{
			return;
		}
		for (int j = 0; j < Bindings.Count; j++)
		{
			KeyCode main = Bindings[j].Main;
			if (FiredKeys.Contains(main) || !Down(main))
			{
				continue;
			}
			int num = -1;
			int num2 = -1;
			for (int k = 0; k < Bindings.Count; k++)
			{
				if (Bindings[k].Main == main && Bindings[k].Modifiers.Length > num2 && AllHeld(Bindings[k].Modifiers))
				{
					num = k;
					num2 = Bindings[k].Modifiers.Length;
				}
			}
			if (num >= 0)
			{
				Wanted[Bindings[num].Slot] = true;
				FiredKeys.Add(main);
				FiredWithModifiers.Add(num2 > 0);
			}
		}
	}

	internal static bool ShortcutDown(KeyboardShortcut shortcut)
	{
		if (shortcut.MainKey == KeyCode.None || ZInput.instance == null || Time.inFixedTimeStep || !Down(shortcut.MainKey))
		{
			return false;
		}
		foreach (KeyCode modifier in shortcut.Modifiers)
		{
			if (!Held(modifier))
			{
				return false;
			}
		}
		return true;
	}

	private static bool AllHeld(KeyCode[] keys)
	{
		for (int i = 0; i < keys.Length; i++)
		{
			if (!Held(keys[i]))
			{
				return false;
			}
		}
		return true;
	}

	internal static void Tick()
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null || ZInput.instance == null || !((Character)localPlayer).TakeInput() || Hud.InRadial())
		{
			return;
		}
		Resolve();
		for (int i = 0; i < 6; i++)
		{
			if (Wanted[i])
			{
				SlotCell slotCell = SlotLayout.QuickCell(i);
				ItemDrop.ItemData itemAt = ((Humanoid)localPlayer).m_inventory.GetItemAt(slotCell.Pos.x, slotCell.Pos.y);
				if (itemAt != null)
				{
					localPlayer.UseItem(null, itemAt, fromInventoryGui: false);
				}
			}
		}
	}

	internal static bool TakesButton(string name)
	{
		if (Player.m_localPlayer == null || ZInput.instance == null || Time.inFixedTimeStep)
		{
			return false;
		}
		Resolve();
		if (FiredKeys.Count == 0)
		{
			return false;
		}
		InputAction action = ZInput.instance.GetButtonDef(name)?.ButtonAction;
		for (int i = 0; i < FiredKeys.Count; i++)
		{
			if ((FiredWithModifiers[i] || ModConfig.KeysWinOverVanilla.Value) && ActionUses(action, FiredKeys[i]))
			{
				return true;
			}
		}
		return false;
	}

	internal static InputControl ControlOf(KeyCode code)
	{
		if (ZInput.TryKeyCodeToKey(code, out Key result) && result != Key.None)
		{
			return Keyboard.current?[result];
		}
		if (Mouse.current != null)
		{
			switch (code)
			{
			case KeyCode.Mouse0:
				return Mouse.current.leftButton;
			case KeyCode.Mouse1:
				return Mouse.current.rightButton;
			case KeyCode.Mouse2:
				return Mouse.current.middleButton;
			case KeyCode.Mouse3:
				return Mouse.current.backButton;
			case KeyCode.Mouse4:
				return Mouse.current.forwardButton;
			}
		}
		return null;
	}

	internal static bool ActionUses(InputAction action, KeyCode code)
	{
		InputControl inputControl = ((action != null) ? ControlOf(code) : null);
		if (inputControl == null)
		{
			return false;
		}
		ReadOnlyArray<InputControl> controls = action.controls;
		for (int i = 0; i < controls.Count; i++)
		{
			if (controls[i] == inputControl)
			{
				return true;
			}
		}
		return false;
	}

	internal static string Label(int ordinal)
	{
		return Label(ModConfig.SlotKey(ordinal));
	}

	internal static string Label(KeyboardShortcut key)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyCode modifier in key.Modifiers)
		{
			stringBuilder.Append(KeyName(modifier)).Append('+');
		}
		return stringBuilder.Append(KeyName(key.MainKey)).ToString();
	}

	private static string KeyName(KeyCode key)
	{
		switch (key)
		{
		case KeyCode.RightAlt:
		case KeyCode.LeftAlt:
			return "Alt";
		case KeyCode.RightControl:
		case KeyCode.LeftControl:
			return "Ctrl";
		case KeyCode.RightShift:
		case KeyCode.LeftShift:
			return "Shift";
		case KeyCode.None:
			return "-";
		default:
		{
			string text = key.ToString();
			if (IsMouse(key))
			{
				return "M" + (int)(key - 323 + 1);
			}
			if (!text.StartsWith("Alpha"))
			{
				return text;
			}
			return text.Substring(5);
		}
		}
	}
}
