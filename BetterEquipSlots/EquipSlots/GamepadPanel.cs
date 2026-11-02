using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

[HarmonyPatch]
internal static class GamepadPanel
{
	internal static int Cursor;

	private static readonly Vector2[] Directions = new Vector2[4]
	{
		Vector2.left,
		Vector2.right,
		new Vector2(0f, -1f),
		new Vector2(0f, 1f)
	};

	private static readonly string[][] DirectionButtons = new string[4][]
	{
		new string[2] { "JoyDPadLeft", "JoyLStickLeft" },
		new string[2] { "JoyDPadRight", "JoyLStickRight" },
		new string[2] { "JoyDPadUp", "JoyLStickUp" },
		new string[2] { "JoyDPadDown", "JoyLStickDown" }
	};

	private static bool s_reading;

	private static int s_modalFrame = -10;

	private static bool s_wasFree;

	private static float s_scrollHeld;

	private static int s_dialogFrame = -1;

	private static bool s_dialogOpen;

	internal static bool Active
	{
		get
		{
			if (SlotPanel.Grid != null && SlotPanel.Grid.m_uiGroup != null && SlotPanel.Grid.m_uiGroup.m_userActive)
			{
				return InventoryGui.IsVisible();
			}
			return false;
		}
	}

	private static bool Free()
	{
		InventoryGui instance = InventoryGui.instance;
		if (!Active || instance == null)
		{
			return false;
		}
		if (DialogOpen(instance) || !SlotPanel.Grid.m_uiGroup.IsActive)
		{
			s_modalFrame = Time.frameCount;
			return false;
		}
		return Time.frameCount - s_modalFrame > 1;
	}

	private static bool DialogOpen(InventoryGui gui)
	{
		if (s_dialogFrame == Time.frameCount)
		{
			return s_dialogOpen;
		}
		s_dialogFrame = Time.frameCount;
		s_dialogOpen = (gui.m_splitDialog != null && gui.m_splitDialog.IsActive) || Open(gui.m_variantDialog) || Open(gui.m_skillsDialog) || Open(gui.m_textsDialog) || Open(gui.m_achievementsPanel) || (gui.m_trophiesPanel != null && gui.m_trophiesPanel.activeSelf) || UnifiedPopup.IsVisible() || Console.IsVisible();
		Transform transform = gui.transform;
		int num = 0;
		while (!s_dialogOpen && num < transform.childCount)
		{
			Transform child = transform.GetChild(num);
			s_dialogOpen = child.gameObject.activeSelf && child.GetComponent<SplitDialog>() != null;
			num++;
		}
		return s_dialogOpen;
	}

	private static bool Open(Component component)
	{
		if (component != null)
		{
			return component.gameObject.activeSelf;
		}
		return false;
	}

	internal static bool Takes(string name)
	{
		if (s_reading || name == null || !name.StartsWith("Joy", StringComparison.Ordinal) || !Free())
		{
			return false;
		}
		if (name == "JoyButtonB")
		{
			return LookPicker.Index >= 0;
		}
		if (!name.StartsWith("JoyDPad", StringComparison.Ordinal) && (!name.StartsWith("JoyLStick", StringComparison.Ordinal) || name.Length <= "JoyLStick".Length) && (!name.StartsWith("JoyRStick", StringComparison.Ordinal) || name.Length <= "JoyRStick".Length) && !(name == "JoyButtonA"))
		{
			return name == "JoyButtonX";
		}
		return true;
	}

	[HarmonyPatch(typeof(InventoryGui), "SetActiveGroup", new Type[]
	{
		typeof(int),
		typeof(bool)
	})]
	[HarmonyPrefix]
	private static bool SetActiveGroup(InventoryGui __instance, ref int index)
	{
		if (!Plugin.Active || SlotPanel.Grid == null || SlotPanel.Grid.m_uiGroup == null || __instance.m_playerGrid == null)
		{
			return true;
		}
		try
		{
			UIGroupHandler uiGroup = SlotPanel.Grid.m_uiGroup;
			int num = Array.IndexOf(__instance.m_uiGroups, __instance.m_playerGrid.m_uiGroup);
			bool buttonDown = ZInput.GetButtonDown("JoyTabRight");
			bool buttonDown2 = ZInput.GetButtonDown("JoyTabLeft");
			if (uiGroup.m_userActive)
			{
				uiGroup.SetActive(active: false);
				if (buttonDown2 && index < num)
				{
					index = num;
				}
				return true;
			}
			bool flag = buttonDown && __instance.m_activeGroup == num && index == num + 1;
			bool flag2 = buttonDown2 && __instance.m_activeGroup == num + 1 && index == num;
			if (num < 0 || (!flag && !flag2) || !SlotPanel.IsShown() || !ZInput.IsGamepadActive())
			{
				return true;
			}
			UIGroupHandler[] uiGroups = __instance.m_uiGroups;
			for (int i = 0; i < uiGroups.Length; i++)
			{
				uiGroups[i]?.SetActive(active: false);
			}
			__instance.m_activeGroup = num;
			uiGroup.SetActive(active: true);
			Cursor = Mathf.Clamp(Cursor, 0, Mathf.Max(0, SlotPanel.CellCount - 1));
			SelectCursor();
			return false;
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Controller step onto the equipment panel failed: " + ex.Message);
			return true;
		}
	}

	internal static void Update(InventoryGui gui)
	{
		if (!Active)
		{
			return;
		}
		s_reading = true;
		try
		{
			ReadButtons(gui);
		}
		finally
		{
			s_reading = false;
		}
	}

	private static void ReadButtons(InventoryGui gui)
	{
		if (!ZInput.IsGamepadActive() || !SlotPanel.IsShown())
		{
			Leave(gui);
			return;
		}
		bool flag = Free() && ZInput.IsExclusiveGamepadActive();
		if (flag && !s_wasFree)
		{
			SelectCursor();
		}
		s_wasFree = flag;
		if (!flag)
		{
			return;
		}
		if (LookPicker.Index >= 0)
		{
			LookPicker.PadUpdate();
			return;
		}
		if (ZInput.GetButtonDown("JoyRStickLeft"))
		{
			SlotPanel.StepTab(-1);
		}
		else if (ZInput.GetButtonDown("JoyRStickRight"))
		{
			SlotPanel.StepTab(1);
		}
		int num = (ZInput.GetButton("JoyRStickUp") ? 1 : (ZInput.GetButton("JoyRStickDown") ? (-1) : 0));
		if (num == 0)
		{
			s_scrollHeld = 0f;
		}
		else if (ZInput.GetButtonDown((num > 0) ? "JoyRStickUp" : "JoyRStickDown"))
		{
			s_scrollHeld = 0f;
			InfoView.Scroll(num);
		}
		else
		{
			s_scrollHeld += Time.unscaledDeltaTime;
			if (s_scrollHeld > 0.3f)
			{
				InfoView.Scroll((float)num * Time.unscaledDeltaTime * 4f);
			}
		}
		int cellCount = SlotPanel.CellCount;
		if (cellCount == 0)
		{
			return;
		}
		Cursor = Mathf.Clamp(Cursor, 0, cellCount - 1);
		for (int i = 0; i < Directions.Length; i++)
		{
			if (ZInput.GetButtonDown(DirectionButtons[i][0]) || ZInput.GetButtonDown(DirectionButtons[i][1]))
			{
				int num2 = Nearest(Cursor, Directions[i], cellCount);
				if (num2 >= 0)
				{
					Cursor = num2;
					SelectCursor();
				}
				break;
			}
		}
		InventoryGrid grid = SlotPanel.Grid;
		SlotCell slotCell = SlotPanel.CellAt(Cursor);
		if (slotCell == null)
		{
			return;
		}
		ItemDrop.ItemData arg = ((grid.m_inventory != null) ? grid.m_inventory.GetItemAt(slotCell.Pos.x, slotCell.Pos.y) : null);
		if (ZInput.GetButtonDown("JoyButtonA"))
		{
			InventoryGrid.Modifier arg2 = (ZInput.GetButton("JoyLTrigger") ? InventoryGrid.Modifier.Split : (ZInput.GetButton("JoyRTrigger") ? InventoryGrid.Modifier.Drop : InventoryGrid.Modifier.Select));
			grid.m_onSelected?.Invoke(grid, arg, slotCell.Pos, arg2);
		}
		else if (ZInput.GetButtonDown("JoyButtonX"))
		{
			if (ZInput.GetButton("JoyLTrigger"))
			{
				grid.m_onSelected?.Invoke(grid, arg, slotCell.Pos, InventoryGrid.Modifier.Move);
			}
			else
			{
				grid.m_onRightClick?.Invoke(grid, arg, slotCell.Pos);
			}
		}
	}

	internal static void Leave(InventoryGui gui)
	{
		if (Active)
		{
			SlotPanel.Grid.m_uiGroup.SetActive(active: false);
			if (gui != null && gui.m_playerGrid != null)
			{
				gui.SetActiveGroup(gui.m_playerGrid.m_uiGroup, false);
			}
		}
	}

	private static int Nearest(int from, Vector2 direction, int count)
	{
		Vector2 vector = SlotPanel.CellPlace(from);
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i < count; i++)
		{
			if (i == from)
			{
				continue;
			}
			Vector2 lhs = SlotPanel.CellPlace(i) - vector;
			float num2 = Vector2.Dot(lhs, direction);
			if (!(num2 < 1f))
			{
				float num3 = Mathf.Abs(lhs.x * direction.y - lhs.y * direction.x);
				float num4 = num2 + num3 * 2f;
				if (num4 < num)
				{
					num = num4;
					result = i;
				}
			}
		}
		return result;
	}

	private static void SelectCursor()
	{
		InventoryGrid grid = SlotPanel.Grid;
		if (!(grid == null) && Cursor >= 0 && Cursor < grid.m_elements.Count && !(grid.m_elements[Cursor] == null))
		{
			Selectable component = grid.m_elements[Cursor].GetComponent<Selectable>();
			if (component != null)
			{
				component.Select();
			}
		}
	}
}
