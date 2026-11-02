using System;
using HarmonyLib;
using UnityEngine;

namespace BetterEquipSlots.EquipSlots;

[HarmonyPatch]
internal static class GamepadQuickSlots
{
	private const string Left = "JoyHotbarLeft";

	private const string Right = "JoyHotbarRight";

	private const string Use = "JoyHotbarUse";

	private static int s_takenFrame = -1;

	private static bool s_takeLeft;

	private static bool s_takeRight;

	private static bool s_takeUse;

	internal static int Selected { get; private set; } = -1;

	internal static bool Takes(string name)
	{
		if (s_takenFrame != Time.frameCount || Time.inFixedTimeStep)
		{
			return false;
		}
		if ((!s_takeLeft || !(name == "JoyHotbarLeft")) && (!s_takeRight || !(name == "JoyHotbarRight")))
		{
			if (s_takeUse)
			{
				return name == "JoyHotbarUse";
			}
			return false;
		}
		return true;
	}

	internal static void Reset()
	{
		Selected = -1;
	}

	[HarmonyPatch(typeof(HotkeyBar), "Update")]
	[HarmonyPrefix]
	private static void Step(HotkeyBar __instance)
	{
		if (!Plugin.Active || !ModConfig.GamepadHotbar.Value || !ModConfig.HudEnabled.Value)
		{
			Selected = -1;
			return;
		}
		try
		{
			Player localPlayer = Player.m_localPlayer;
			if (localPlayer == null || !HotbarTakesInput() || ZInput.GetButton("JoyAltKeys"))
			{
				return;
			}
			bool buttonDown = ZInput.GetButtonDown("JoyHotbarLeft");
			bool buttonDown2 = ZInput.GetButtonDown("JoyHotbarRight");
			bool buttonDown3 = ZInput.GetButtonDown("JoyHotbarUse");
			if (!buttonDown && !buttonDown2 && !buttonDown3)
			{
				return;
			}
			int num = __instance.m_elements.Count - 1;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (Selected >= 0)
			{
				if (buttonDown2)
				{
					flag2 = true;
					Selected++;
					if (Selected >= 6)
					{
						Selected = -1;
						__instance.m_selected = 0;
					}
				}
				else if (buttonDown)
				{
					flag = true;
					Selected--;
					if (Selected < 0)
					{
						__instance.m_selected = Mathf.Max(0, num);
					}
				}
				if (buttonDown3 && Selected >= 0)
				{
					flag3 = true;
					SlotCell slotCell = SlotLayout.QuickCell(Selected);
					ItemDrop.ItemData itemAt = ((Humanoid)localPlayer).m_inventory.GetItemAt(slotCell.Pos.x, slotCell.Pos.y);
					if (itemAt != null)
					{
						localPlayer.UseItem(null, itemAt, fromInventoryGui: false);
					}
				}
			}
			else if (buttonDown2 && __instance.m_selected >= num)
			{
				flag2 = true;
				Selected = 0;
			}
			else if (buttonDown && __instance.m_selected <= 0)
			{
				flag = true;
				Selected = 5;
			}
			s_takenFrame = Time.frameCount;
			s_takeLeft = flag;
			s_takeRight = flag2;
			s_takeUse = flag3;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Controller quick slots failed: " + ex.Message);
			Selected = -1;
		}
	}

	[HarmonyPatch(typeof(HotkeyBar), "UpdateIcons")]
	[HarmonyPostfix]
	private static void HideHotbarSelection(HotkeyBar __instance)
	{
		if (Selected < 0 || !Plugin.Active)
		{
			return;
		}
		foreach (HotkeyBar.ElementData element in __instance.m_elements)
		{
			if (element?.m_selection != null && element.m_selection.activeSelf)
			{
				element.m_selection.SetActive(value: false);
			}
		}
	}

	private static bool HotbarTakesInput()
	{
		if (!InventoryGui.IsVisible() && !Menu.IsVisible() && !GameCamera.InFreeFly() && !Minimap.IsOpen() && !Hud.IsPieceSelectionVisible() && !StoreGui.IsVisible() && !Console.IsVisible() && (!(Chat.instance != null) || !Chat.instance.HasFocus()) && !PlayerCustomizaton.IsBarberGuiVisible())
		{
			return !Hud.InRadial();
		}
		return false;
	}
}
