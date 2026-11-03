using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

internal static class SlotPanel
{
	internal enum PanelTab
	{
		Equipment,
		Style,
		Info
	}

	private const float Margin = 26f;

	private const float Gap = 10f;

	private static Vector4 s_pad = new Vector4(26f, 26f, 26f, 26f);

	private const float TitleHeight = 30f;

	private const float TitleDrop = 8f;

	private const int Columns = 5;

	private static int s_sheetRows = 6;

	private const int SectionColumns = 3;

	internal static InventoryGrid Grid;

	private static PanelTab s_tab;

	private static GameObject s_tooltipPrefab;

	private static Button s_tabGear;

	private static Button s_tabLook;

	private static Button s_tabInfo;

	private static RectTransform s_root;

	private static RectTransform s_cellsRoot;

	private static TMP_Text s_titleTemplate;

	private static readonly List<TMP_Text> Titles = new List<TMP_Text>();

	private static readonly List<SlotCell> Cells = new List<SlotCell>();

	private static readonly List<Vector2> CellPositions = new List<Vector2>();

	private static readonly List<SlotCell> Wanted = new List<SlotCell>();

	private static float s_space = 70f;

	private static float s_cellSize = 64f;

	private static readonly (SlotKind kind, int ordinal)[] LeftWithJewelry = new (SlotKind, int)[6]
	{
		(SlotKind.Helmet, 0),
		(SlotKind.Chest, 0),
		(SlotKind.Legs, 0),
		(SlotKind.Cape, 0),
		(SlotKind.Belt, 0),
		(SlotKind.Trinket, 0)
	};

	private static readonly (SlotKind kind, int ordinal)[] RightWithJewelry = new (SlotKind, int)[6]
	{
		(SlotKind.Earring, 0),
		(SlotKind.Earring, 1),
		(SlotKind.Amulet, 0),
		(SlotKind.Ring, 0),
		(SlotKind.Ring, 1),
		(SlotKind.Ammo, 0)
	};

	private static readonly (SlotKind kind, int ordinal)[] RightWithJewelryAndBackpack = new (SlotKind, int)[7]
	{
		(SlotKind.Earring, 0),
		(SlotKind.Earring, 1),
		(SlotKind.Amulet, 0),
		(SlotKind.Ring, 0),
		(SlotKind.Ring, 1),
		(SlotKind.Ammo, 0),
		(SlotKind.Backpack, 0)
	};

	private static readonly (SlotKind kind, int ordinal)[] LeftPlain = new (SlotKind, int)[4]
	{
		(SlotKind.Helmet, 0),
		(SlotKind.Chest, 0),
		(SlotKind.Legs, 0),
		(SlotKind.Cape, 0)
	};

	// Top-aligned beside the armour: belt, trinket and ammo in the first three rows, the backpack (with a backpack mod) under them
	private static readonly (SlotKind kind, int ordinal)[] RightPlain = new (SlotKind, int)[3]
	{
		(SlotKind.Belt, 0),
		(SlotKind.Trinket, 0),
		(SlotKind.Ammo, 0)
	};

	private static readonly (SlotKind kind, int ordinal)[] RightPlainWithBackpack = new (SlotKind, int)[4]
	{
		(SlotKind.Belt, 0),
		(SlotKind.Trinket, 0),
		(SlotKind.Ammo, 0),
		(SlotKind.Backpack, 0)
	};

	private static readonly List<SlotCell> Sheet = new List<SlotCell>();

	private static readonly List<Vector2> SheetPlaces = new List<Vector2>();

	private static RectTransform s_bkg;

	private static Image s_braidLeft;

	private static Image s_braidRight;

	private static RectTransform s_quiverWindow;

	private static RectTransform s_utilityWindow;

	private static RawImage s_preview;

	private static readonly Dictionary<InventoryElement, TMP_Text> SlotNames = new Dictionary<InventoryElement, TMP_Text>();

	private static Image s_backing;

	// The panel's offset while it's being dragged, saved to the config when it's let go
	private static Vector2? s_moving;

	private static TMP_Text s_hintTemplate;

	private static TMP_Text s_hintAttack;

	private static TMP_Text s_hintBlock;

	private static Image s_iconAttack;

	private static Image s_iconBlock;

	private static readonly string[] WantedArt = new string[3] { "woodpanel_info", "woodpanel_crafting", "woodpanel_playerinventory" };

	private const float BraidLine = 0.4f;

	private static readonly List<Image> Braids = new List<Image>();

	private static bool s_keyOpen;

	private static bool s_failedOnce;

	private static float s_overhang = 72f;

	private static readonly List<Graphic> Graphics = new List<Graphic>();

	private static readonly Vector3[] Corners = new Vector3[4];

	private static readonly Dictionary<InventoryElement, CanvasGroup> Dimmers = new Dictionary<InventoryElement, CanvasGroup>();

	private static readonly Color SlotNameColor = new Color(0.85f, 0.8f, 0.7f, 0.85f);

	private const float RepairTabInset = 10f;

	private const float WindowGap = 8f;

	private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();

	private static float PadLeft => s_pad.x;

	private static float PadBottom => s_pad.y;

	private static float PadRight => s_pad.z;

	private static float PadTop => s_pad.w;

	internal static bool LookMode => s_tab == PanelTab.Style;

	internal static bool InfoMode => s_tab == PanelTab.Info;

	internal static int CellCount
	{
		get
		{
			if (!(Grid != null))
			{
				return 0;
			}
			return Mathf.Min(Cells.Count, Grid.m_elements.Count);
		}
	}

	internal static RectTransform PanelRect => s_bkg;

	internal static RectTransform QuiverRect => s_quiverWindow;

	internal static RectTransform UtilityRect => s_utilityWindow;

	internal static bool PanelWanted
	{
		get
		{
			if (ModConfig.ShowOnKey.Value)
			{
				return s_keyOpen;
			}
			return true;
		}
	}

	private static float SideTitleTop => Mathf.Max(8f, PadTop * 0.5f);

	internal static bool IsShown()
	{
		if (s_root != null)
		{
			return s_root.gameObject.activeInHierarchy;
		}
		return false;
	}

	internal static bool IsPanelGrid(InventoryGrid grid)
	{
		if ((object)grid == Grid)
		{
			return (object)grid != null;
		}
		return false;
	}

	internal static SlotCell CellAt(int index)
	{
		if (index < 0 || index >= Cells.Count)
		{
			return null;
		}
		return Cells[index];
	}

	internal static Vector2 CellPlace(int index)
	{
		if (index < 0 || index >= CellPositions.Count)
		{
			return Vector2.zero;
		}
		return CellPositions[index];
	}

	internal static void Destroy()
	{
		LookPicker.Close();
		InfoView.Destroy();
		if (s_root != null)
		{
			GameObject gameObject = s_root.gameObject;
			gameObject.SetActive(value: false);
			gameObject.name = "CDEV_EquipmentSlots_old";
			UnityEngine.Object.Destroy(gameObject);
		}
		s_root = null;
		Grid = null;
		s_tabGear = (s_tabLook = (s_tabInfo = null));
		Cells.Clear();
		CellPositions.Clear();
	}

	internal static void Create(InventoryGui gui)
	{
		try
		{
			Build(gui);
		}
		catch (Exception arg)
		{
			Plugin.Log.LogError($"Could not build the equipment panel: {arg}");
		}
	}

	private static void Build(InventoryGui gui)
	{
		InventoryGrid componentInChildren = gui.m_player.GetComponentInChildren<InventoryGrid>(includeInactive: true);
		if (componentInChildren == null)
		{
			Plugin.Log.LogError("InventoryGui has no player grid; no equipment panel.");
			return;
		}
		GameObject gameObject = new GameObject("CDEV_EquipmentSlots", typeof(RectTransform));
		s_root = (RectTransform)gameObject.transform;
		s_root.SetParent(gui.m_player, worldPositionStays: false);
		RectTransform rectTransform = s_root;
		Vector2 anchorMin = (s_root.anchorMax = new Vector2(1f, 1f));
		rectTransform.anchorMin = anchorMin;
		s_root.pivot = new Vector2(0f, 1f);
		Image art = WindowArt(gui);
		MeasurePadding(art);
		FindBraids(gui);
		s_bkg = NewWindow("Bkg", art);
		s_quiverWindow = NewWindow("QuiverBkg", art);
		s_quiverWindow.gameObject.SetActive(value: false);
		s_utilityWindow = NewWindow("UtilityBkg", art);
		s_utilityWindow.gameObject.SetActive(value: false);
		GameObject gameObject2 = new GameObject("CharacterBacking", typeof(RectTransform));
		gameObject2.transform.SetParent(s_root, worldPositionStays: false);
		s_backing = gameObject2.AddComponent<Image>();
		s_backing.raycastTarget = false;
		s_hintTemplate = gui.m_containerName;
		Image component = componentInChildren.m_elementPrefab.GetComponent<Image>();
		if (component != null)
		{
			s_backing.sprite = component.sprite;
			s_backing.type = component.type;
			Color color = Color.white;
			Button component2 = componentInChildren.m_elementPrefab.GetComponent<Button>();
			if (component2 != null && component2.transition == Selectable.Transition.ColorTint)
			{
				color = component2.colors.normalColor * component2.colors.colorMultiplier;
			}
			s_backing.color = component.color * color;
			Color color2 = s_backing.color;
			if (color2.a > 0.6f && color2.r + color2.g + color2.b > 2.1f)
			{
				s_backing.sprite = null;
				s_backing.color = new Color(0f, 0f, 0f, 0.35f);
			}
			s_backing.material = component.material;
			s_backing.pixelsPerUnitMultiplier = component.pixelsPerUnitMultiplier;
		}
		else
		{
			s_backing.color = new Color(0f, 0f, 0f, 0.3f);
		}
		GameObject gameObject3 = new GameObject("Character", typeof(RectTransform));
		gameObject3.transform.SetParent(s_root, worldPositionStays: false);
		s_preview = gameObject3.AddComponent<RawImage>();
		s_preview.raycastTarget = true;
		gameObject3.AddComponent<PreviewDrag>();
		s_preview.enabled = false;
		s_titleTemplate = gui.m_containerName;
		Titles.Clear();
		GameObject gameObject4 = new GameObject("Cells", typeof(RectTransform));
		s_cellsRoot = (RectTransform)gameObject4.transform;
		s_cellsRoot.SetParent(s_root, worldPositionStays: false);
		Stretch(s_cellsRoot);
		GameObject gameObject5 = new GameObject("TooltipAnchor", typeof(RectTransform));
		RectTransform rectTransform2 = (RectTransform)gameObject5.transform;
		rectTransform2.SetParent(s_root, worldPositionStays: false);
		Vector2 vector2 = (rectTransform2.pivot = new Vector2(1f, 1f));
		anchorMin = (rectTransform2.anchorMax = vector2);
		rectTransform2.anchorMin = anchorMin;
		rectTransform2.sizeDelta = Vector2.one;
		rectTransform2.anchoredPosition = new Vector2(90f, 40f);
		UIGroupHandler uIGroupHandler = gameObject.AddComponent<UIGroupHandler>();
		uIGroupHandler.m_groupPriority = ((componentInChildren.m_uiGroup != null) ? componentInChildren.m_uiGroup.m_groupPriority : 0);
		uIGroupHandler.SetActive(active: false);
		gameObject.SetActive(value: false);
		Grid = gameObject4.AddComponent<InventoryGrid>();
		Grid.m_elementPrefab = componentInChildren.m_elementPrefab;
		Grid.m_gridRoot = s_cellsRoot;
		Grid.m_elementSpace = componentInChildren.m_elementSpace;
		Grid.m_uiGroup = uIGroupHandler;
		Grid.m_tooltipAnchor = rectTransform2;
		InventoryGrid grid = Grid;
		grid.m_onSelected = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier>)Delegate.Combine(grid.m_onSelected, (Action<InventoryGrid, ItemDrop.ItemData, Vector2i, InventoryGrid.Modifier>)((InventoryGrid g, ItemDrop.ItemData item, Vector2i pos, InventoryGrid.Modifier mod) =>
		{
			if (!LookClick(gui, pos))
			{
				ItemDrop.ItemData itemData = ((item == null && gui.m_dragGo == null) ? EquipSync.MirrorOf(Player.m_localPlayer, SlotLayout.At(pos)) : null);
				if (itemData != null)
				{
					gui.OnSelectedItem(gui.m_playerGrid, itemData, itemData.m_gridPos, mod);
				}
				else
				{
					gui.OnSelectedItem(g, item, pos, mod);
				}
			}
		}));
		InventoryGrid grid2 = Grid;
		grid2.m_onRightClick = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i>)Delegate.Combine(grid2.m_onRightClick, new Action<InventoryGrid, ItemDrop.ItemData, Vector2i>(OnRightClick));
		InventoryGrid grid3 = Grid;
		grid3.m_onReleased = (Action<InventoryGrid, ItemDrop.ItemData, Vector2i>)Delegate.Combine(grid3.m_onReleased, (Action<InventoryGrid, ItemDrop.ItemData, Vector2i>)((InventoryGrid g, ItemDrop.ItemData item, Vector2i pos) =>
		{
			if (!LookClick(gui, pos))
			{
				gui.OnReleasedItem(g, item, pos);
			}
		}));
		InventoryGrid grid4 = Grid;
		grid4.m_onEnter = (Action<InventoryGrid, Vector2i>)Delegate.Combine(grid4.m_onEnter, new Action<InventoryGrid, Vector2i>(gui.OnEnterElement));
		InventoryGrid grid5 = Grid;
		grid5.CanDropDragOntoItem = (Func<ItemDrop.ItemData, bool>)Delegate.Combine(grid5.CanDropDragOntoItem, new Func<ItemDrop.ItemData, bool>(gui.CanDropDragOntoItem));
		gameObject.SetActive(value: true);
		s_space = componentInChildren.m_elementSpace;
		RectTransform rectTransform3 = Grid.m_elementPrefab.transform as RectTransform;
		s_cellSize = ((rectTransform3 != null && rectTransform3.rect.width > 1f) ? rectTransform3.rect.width : (s_space - 6f));
		Cells.Clear();
		CellPositions.Clear();
		CreateTabs(gui);
		LookPicker.Build(s_root, s_backing, s_hintTemplate, s_tooltipPrefab);
		InfoView.Build(s_root, s_hintTemplate, s_backing, s_titleTemplate);
		if (MultiUtility.Active)
		{
			MoveRepairTab(gui);
		}
	}

	private static Image WindowArt(InventoryGui gui)
	{
		Transform root = ((gui.m_player != null) ? gui.m_player.parent : null);
		Image image = null;
		int num = 0;
		while (image == null && num < WantedArt.Length)
		{
			image = WoodPanel(root, WantedArt[num]) ?? WoodPanel(gui.m_player, WantedArt[num]);
			num++;
		}
		if (image == null)
		{
			Transform transform = ((gui.m_player != null) ? gui.m_player.Find("Bkg") : null);
			image = ((transform != null) ? transform.GetComponent<Image>() : null);
		}
		return image;
	}

	private static void MeasurePadding(Image art)
	{
		if (!(art == null) && !(art.sprite == null) && !(art.sprite.border == Vector4.zero))
		{
			Vector4 border = art.sprite.border;
			float num = Mathf.Max(Mathf.Max(border.x, border.z), Mathf.Max(border.y, border.w));
			s_pad = new Vector4(Mathf.Clamp(26f - (num - border.x), 12f, 26f), Mathf.Clamp(26f - (num - border.y), 12f, 26f), Mathf.Clamp(26f - (num - border.z), 12f, 26f), Mathf.Clamp(26f - (num - border.w), 12f, 26f));
		}
	}

	private static void FindBraids(InventoryGui gui)
	{
		Transform transform = ((gui.m_player != null) ? gui.m_player.parent : null);
		if (transform == null)
		{
			return;
		}
		Image[] componentsInChildren = transform.GetComponentsInChildren<Image>(includeInactive: true);
		foreach (Image image in componentsInChildren)
		{
			string text = ((image.sprite != null) ? image.sprite.name : null);
			if (text != null && text.IndexOf("Braid", StringComparison.OrdinalIgnoreCase) >= 0 && (!(s_root != null) || !image.transform.IsChildOf(s_root)))
			{
				if (s_braidLeft == null && text.EndsWith("HalfL", StringComparison.OrdinalIgnoreCase))
				{
					s_braidLeft = image;
				}
				else if (s_braidRight == null && text.EndsWith("HalfR", StringComparison.OrdinalIgnoreCase))
				{
					s_braidRight = image;
				}
			}
		}
	}

	private static float LiftToTop(TMP_Text title, float top, bool raiseHalf)
	{
		title.ForceMeshUpdate();
		Bounds textBounds = title.textBounds;
		float num = 15f;
		float num2 = (textBounds.max.y + textBounds.min.y) * 0.5f;
		float num3 = Mathf.Clamp(0f - num - num2, -30f, 30f);
		if (raiseHalf)
		{
			num3 += Mathf.Clamp(textBounds.size.y, 4f, 30f) * 0.5f;
		}
		RectTransform rectTransform = title.rectTransform;
		rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y + num3);
		return top + num;
	}

	private static void AddBraids(RectTransform window, TMP_Text title, float middle, float windowWidth)
	{
		if (!(s_braidLeft == null) && !(s_braidRight == null))
		{
			float num = Mathf.Min(title.GetPreferredValues(title.text).x, windowWidth - PadLeft - PadRight);
			float num2 = windowWidth * 0.5f;
			float num3 = num2 - num * 0.5f - 10f - PadLeft;
			if (!(num3 < 24f))
			{
				float height = ((RectTransform)s_braidLeft.transform).rect.height;
				float num4 = middle - 0.4f * height;
				Braid(s_braidLeft, window, new Vector2(PadLeft, 0f - num4), num3, height);
				Braid(s_braidRight, window, new Vector2(windowWidth - PadRight - num3, 0f - num4), num3, height);
			}
		}
	}

	private static void Braid(Image template, RectTransform window, Vector2 position, float width, float height)
	{
		Image image = UnityEngine.Object.Instantiate(template, window, worldPositionStays: false);
		image.name = "TitleBraid";
		image.gameObject.SetActive(value: true);
		image.raycastTarget = false;
		RectTransform rectTransform = image.rectTransform;
		Vector2 vector = (rectTransform.pivot = new Vector2(0f, 1f));
		Vector2 anchorMin = (rectTransform.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		rectTransform.sizeDelta = new Vector2(width, height);
		rectTransform.anchoredPosition = position;
		Braids.Add(image);
	}

	private static Image WoodPanel(Transform root, string sprite)
	{
		if (root == null)
		{
			return null;
		}
		Image result = null;
		float num = 0f;
		Image[] componentsInChildren = root.GetComponentsInChildren<Image>(includeInactive: true);
		foreach (Image image in componentsInChildren)
		{
			if (!(image.sprite == null) && (!(s_root != null) || !image.transform.IsChildOf(s_root)) && !(image.color.a < 0.5f) && image.sprite.name.IndexOf(sprite, StringComparison.OrdinalIgnoreCase) >= 0 && image.sprite.name.IndexOf("mask", StringComparison.OrdinalIgnoreCase) < 0)
			{
				Rect rect = image.rectTransform.rect;
				float num2 = rect.width * rect.height;
				if (num2 > num)
				{
					num = num2;
					result = image;
				}
			}
		}
		return result;
	}

	internal static void BeginMove()
	{
		s_moving = ModConfig.PanelOffset.Value;
	}

	internal static void Move(Vector2 screenDelta)
	{
		if (!s_moving.HasValue || s_root == null || s_root.parent == null)
		{
			return;
		}
		Vector3 lossyScale = s_root.parent.lossyScale;
		if (Mathf.Abs(lossyScale.x) < 1E-05f || Mathf.Abs(lossyScale.y) < 1E-05f)
		{
			return;
		}
		s_moving = s_moving.Value + new Vector2(screenDelta.x / lossyScale.x, screenDelta.y / lossyScale.y);
	}

	internal static void EndMove()
	{
		if (s_moving.HasValue)
		{
			Vector2 value = s_moving.Value;
			s_moving = null;
			ModConfig.PanelOffset.Value = new Vector2(Mathf.Round(value.x), Mathf.Round(value.y));
		}
	}

	private static RectTransform NewWindow(string name, Image art)
	{
		GameObject gameObject = new GameObject(name, typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(s_root, worldPositionStays: false);
		TopLeft(rectTransform);
		Image image = gameObject.AddComponent<Image>();
		image.raycastTarget = true;
		gameObject.AddComponent<PanelDrag>();
		if (art != null)
		{
			image.sprite = art.sprite;
			image.type = art.type;
			image.fillCenter = true;
			image.color = art.color;
			image.material = art.material;
			image.pixelsPerUnitMultiplier = art.pixelsPerUnitMultiplier;
			image.preserveAspect = false;
		}
		else
		{
			image.color = new Color(0f, 0f, 0f, 0.6f);
		}
		return rectTransform;
	}

	private static void TopLeft(RectTransform rect)
	{
		Vector2 vector = (rect.pivot = new Vector2(0f, 1f));
		Vector2 anchorMin = (rect.anchorMax = vector);
		rect.anchorMin = anchorMin;
		rect.anchoredPosition = Vector2.zero;
	}

	private static void Stretch(RectTransform rect)
	{
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
	}

	internal static void TickKey()
	{
		if (!InventoryGui.IsVisible())
		{
			s_keyOpen = false;
		}
		if (!ModConfig.ShowOnKey.Value || Typing() || !QuickSlots.ShortcutDown(ModConfig.PanelKey.Value))
		{
			return;
		}
		InventoryGui instance = InventoryGui.instance;
		Player localPlayer = Player.m_localPlayer;
		if (!(instance == null) && !(localPlayer == null))
		{
			if (InventoryGui.IsVisible())
			{
				s_keyOpen = !s_keyOpen;
			}
			else if (((Character)localPlayer).TakeInput() && !localPlayer.IsDead() && !localPlayer.IsTeleporting())
			{
				s_keyOpen = true;
				instance.Show(null);
			}
		}
	}

	private static bool Typing()
	{
		if (!Console.IsVisible() && !TextInput.IsVisible())
		{
			if (Chat.instance != null)
			{
				return Chat.instance.HasFocus();
			}
			return false;
		}
		return true;
	}

	internal static bool ApplyShown()
	{
		if (s_root == null)
		{
			return false;
		}
		bool panelWanted = PanelWanted;
		if (s_root.gameObject.activeSelf != panelWanted)
		{
			if (!panelWanted)
			{
				LookPicker.Close();
				GamepadPanel.Leave(InventoryGui.instance);
			}
			s_root.gameObject.SetActive(panelWanted);
		}
		return panelWanted;
	}

	internal static void Refresh(InventoryGui gui, Player player)
	{
		if (Grid == null || s_root == null || player == null)
		{
			return;
		}
		try
		{
			Grid.m_inventory = ((Humanoid)player).m_inventory;
			Render(Grid, player, gui.m_dragItem);
			EquipProgress.Draw(Grid, player);
			CellLocks.Refresh(Grid, player);
			GamepadPanel.Update(gui);
		}
		catch (Exception arg)
		{
			if (!s_failedOnce)
			{
				s_failedOnce = true;
				Plugin.Log.LogError($"Equipment panel update failed: {arg}");
			}
		}
		try
		{
			Vector2 value = s_moving ?? ModConfig.PanelOffset.Value;
			MeasureOverhang(gui);
			float num = CenteredX(gui);
			if (float.IsNaN(num) || float.IsInfinity(num) || num < 0f || num > 4000f)
			{
				num = 84f;
			}
			Vector2 vector = new Vector2(num + value.x, value.y);
			if ((s_root.anchoredPosition - vector).sqrMagnitude > 0.25f)
			{
				s_root.anchoredPosition = vector;
			}
			bool flag = ModConfig.PanelCharacter.Value && !InfoMode;
			InfoView.Tick(player);
			Texture texture = (flag ? PlayerPreview.Render(player) : null);
			s_preview.texture = texture;
			s_preview.enabled = texture != null;
		}
		catch (Exception arg2)
		{
			if (!s_failedOnce)
			{
				s_failedOnce = true;
				Plugin.Log.LogError($"Equipment panel update failed: {arg2}");
			}
		}
	}

	private static float CenteredX(InventoryGui gui)
	{
		float num = s_overhang;
		float width = s_root.rect.width;
		RectTransform rectTransform = gui.m_player.parent as RectTransform;
		RectTransform crafting = gui.m_crafting;
		if (rectTransform == null || crafting == null)
		{
			return num + 12f;
		}
		float num2 = 40f + gui.m_player.rect.width;
		float num3 = rectTransform.rect.width - 40f - crafting.rect.width;
		float num4 = (num2 + num + num3) * 0.5f;
		float num5 = ((BetterArcheryHost.QuiverActive || MultiUtility.Active) ? (8f + PadLeft + PadRight + s_cellSize) : 0f);
		return Mathf.Max(num + 8f, num4 - (width + num5) * 0.5f - num2);
	}

	private static void MeasureOverhang(InventoryGui gui)
	{
		RectTransform rectTransform = ((gui != null) ? gui.m_player : null);
		float num = ((rectTransform != null) ? rectTransform.lossyScale.x : 0f);
		if (num <= 0f)
		{
			return;
		}
		rectTransform.GetWorldCorners(Corners);
		float x = Corners[2].x;
		float num2 = 0f;
		rectTransform.GetComponentsInChildren(includeInactive: false, Graphics);
		// The chest panel is a child of the inventory panel; it is not the frame's art, and mods can move it about
		RectTransform container = gui.m_container;
		for (int i = 0; i < Graphics.Count; i++)
		{
			Graphic graphic = Graphics[i];
			if (!(graphic == null) && graphic.enabled && (!(s_root != null) || !graphic.transform.IsChildOf(s_root)) && (!(container != null) || !graphic.transform.IsChildOf(container)))
			{
				graphic.rectTransform.GetWorldCorners(Corners);
				num2 = Mathf.Max(num2, (Corners[2].x - x) / num);
			}
		}
		Graphics.Clear();
		if (num2 > 0f && num2 < 400f)
		{
			s_overhang = num2;
		}
	}

	internal static void Render(InventoryGrid grid, Player player, ItemDrop.ItemData dragItem)
	{
		Inventory inventory = grid.m_inventory;
		if (inventory == null)
		{
			return;
		}
		ChooseCells(inventory);
		if (!SameCells())
		{
			Rebuild(grid);
		}
		InventoryElement hoveredElement = grid.GetHoveredElement();
		int num = (GamepadPanel.Active ? GamepadPanel.Cursor : (-1));
		for (int i = 0; i < grid.m_elements.Count && i < Cells.Count; i++)
		{
			InventoryElement inventoryElement = grid.m_elements[i];
			SlotCell slotCell = Cells[i];
			bool flag = slotCell.Kind == SlotKind.Look && LookPicker.Index == slotCell.Ordinal;
			Dim(inventoryElement, LookPicker.Index >= 0 && !flag);
			if (slotCell.Kind == SlotKind.Look)
			{
				DrawLook(inventoryElement, slotCell, player);
				EpicLootHost.Cell(inventoryElement, null);
				inventoryElement.m_selected.SetActive(flag || i == num);
				continue;
			}
			ItemDrop.ItemData itemData = inventory.GetItemAt(slotCell.Pos.x, slotCell.Pos.y) ?? EquipSync.MirrorOf(player, slotCell);
			if (itemData != null)
			{
				DrawItem(grid, inventoryElement, itemData, player, dragItem, inventoryElement == hoveredElement || i == num);
			}
			else
			{
				DrawEmpty(inventoryElement, slotCell);
			}
			inventoryElement.m_selected.SetActive(i == num);
		}
	}

	internal static Color FoodColor(ItemDrop.ItemData.SharedData shared)
	{
		if (shared.m_food < shared.m_foodEitr / 2f && shared.m_foodStamina < shared.m_foodEitr / 2f)
		{
			return new Color(0.6f, 0.6f, 1f, 1f);
		}
		if (shared.m_foodStamina < shared.m_food / 2f)
		{
			return new Color(1f, 0.5f, 0.5f, 1f);
		}
		if (shared.m_food < shared.m_foodStamina / 2f)
		{
			return new Color(1f, 1f, 0.5f, 1f);
		}
		return Color.white;
	}

	private static void DrawItem(InventoryGrid grid, InventoryElement element, ItemDrop.ItemData item, Player player, ItemDrop.ItemData dragItem, bool hovered)
	{
		element.m_used = true;
		ShowSlotName(element, show: false);
		HiddenMark.Show(element.m_icon, show: false);
		element.m_icon.enabled = true;
		element.m_icon.sprite = item.GetIcon();
		element.m_icon.color = ((item == dragItem) ? Color.grey : Color.white);
		bool flag = item.m_shared.m_useDurability && item.m_durability < item.GetMaxDurability();
		element.m_durability.gameObject.SetActive(flag);
		if (flag)
		{
			if (item.m_durability <= 0f)
			{
				element.m_durability.SetValue(1f);
				element.m_durability.SetColor((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : new Color(0f, 0f, 0f, 0f));
			}
			else
			{
				element.m_durability.SetValue(item.GetDurabilityPercentage());
				element.m_durability.ResetColor();
			}
		}
		element.m_equiped.enabled = item.m_equipped && !EpicLootHost.IsMagic(item);
		EpicLootHost.Cell(element, item);
		element.m_queued.enabled = player.IsEquipActionQueued(item);
		element.m_noteleport.enabled = !item.m_shared.m_teleportable && !ZoneSystem.instance.GetGlobalKey(GlobalKeys.TeleportAll);
		element.m_food.enabled = item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable && (item.m_shared.m_food > 0f || item.m_shared.m_foodStamina > 0f || item.m_shared.m_foodEitr > 0f);
		if (element.m_food.enabled)
		{
			element.m_food.color = FoodColor(item.m_shared);
		}
		element.m_quality.enabled = item.m_shared.m_maxQuality > 1;
		if (element.m_quality.enabled)
		{
			element.m_quality.text = BetterUiHost.QualityText(item) ?? item.m_quality.ToString();
		}
		element.m_amount.enabled = item.m_shared.m_maxStackSize > 1;
		if (element.m_amount.enabled)
		{
			element.m_amount.text = $"{item.m_stack}/{item.m_shared.m_maxStackSize}";
		}
		BetterUiHost.Cell(element, item);
		if (hovered)
		{
			grid.CreateItemTooltip(item, element.m_tooltip);
		}
		element.m_canBeDroppedOn = grid.CanDropDragOntoItem != null && grid.CanDropDragOntoItem(item);
	}

	private static void Dim(InventoryElement element, bool dim)
	{
		if (!Dimmers.TryGetValue(element, out var value) || value == null)
		{
			if (!dim)
			{
				return;
			}
			value = element.gameObject.GetComponent<CanvasGroup>() ?? element.gameObject.AddComponent<CanvasGroup>();
			Dimmers[element] = value;
		}
		float num = (dim ? 0.25f : 1f);
		if (!Mathf.Approximately(value.alpha, num))
		{
			value.alpha = num;
		}
	}

	private static void DrawLook(InventoryElement element, SlotCell cell, Player player)
	{
		GameObject gameObject = Appearance.Chosen(player, cell.Ordinal);
		bool flag = gameObject == null && Appearance.IsHidden(player, cell.Ordinal);
		if (gameObject == null)
		{
			DrawEmpty(element, cell);
			if (flag)
			{
				ShowSlotName(element, show: false);
				element.m_icon.enabled = element.m_icon.sprite != null;
			}
		}
		else
		{
			element.m_used = false;
			element.m_canBeDroppedOn = false;
			element.m_durability.gameObject.SetActive(value: false);
			element.m_amount.enabled = false;
			element.m_quality.enabled = false;
			element.m_equiped.enabled = false;
			element.m_queued.enabled = false;
			element.m_noteleport.enabled = false;
			element.m_food.enabled = false;
			ShowSlotName(element, show: false);
			element.m_icon.enabled = true;
			element.m_icon.sprite = Appearance.Icon(gameObject);
			element.m_icon.color = Color.white;
		}
		HiddenMark.Show(element.m_icon, flag);
		ref string topic = ref element.m_tooltip.m_topic;
		string text;
		if (gameObject != null)
		{
			text = Appearance.DisplayName(gameObject);
		}
		else
		{
			text = (flag ? Localization.instance.Localize("$cdev_es_look_hidden") : Localization.instance.Localize(Placeholders.NameKey(SlotLayout.LookKind(cell))));
		}
		topic = text;
		element.m_tooltip.m_text = Localization.instance.Localize("$cdev_es_look_cell");
	}

	private static bool LookClick(InventoryGui gui, Vector2i pos)
	{
		SlotCell slotCell = SlotLayout.At(pos);
		if (slotCell == null || slotCell.Kind != SlotKind.Look)
		{
			return false;
		}
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer == null)
		{
			return true;
		}
		ItemDrop.ItemData dragItem = gui.m_dragItem;
		if (dragItem != null)
		{
			if (Appearance.KindOf(dragItem) == SlotLayout.LookKind(slotCell))
			{
				Appearance.TakeLookOf(localPlayer, dragItem);
			}
			gui.SetupDragItem((ItemDrop.ItemData)null, (Inventory)null, 1);
			LookPicker.Close();
			return true;
		}
		LookPicker.Toggle(slotCell.Ordinal);
		return true;
	}

	private static void DrawEmpty(InventoryElement element, SlotCell cell)
	{
		element.m_used = false;
		element.m_canBeDroppedOn = false;
		element.m_durability.gameObject.SetActive(value: false);
		element.m_amount.enabled = false;
		element.m_quality.enabled = false;
		element.m_equiped.enabled = false;
		element.m_queued.enabled = false;
		element.m_noteleport.enabled = false;
		element.m_food.enabled = false;
		// The Utility panel's cells stay plain, without the faded belt
		Sprite sprite = (IsExtraBeltCell(cell) ? null : Placeholders.Get(SlotLayout.LookKind(cell)));
		element.m_icon.enabled = sprite != null;
		element.m_icon.sprite = sprite;
		element.m_icon.color = Placeholders.Tint;
		element.m_tooltip.m_topic = "";
		element.m_tooltip.m_text = "";
		ShowSlotName(element, show: true);
		HiddenMark.Show(element.m_icon, show: false);
		EpicLootHost.Cell(element, null);
	}

	private static void ShowSlotName(InventoryElement element, bool show)
	{
		if (SlotNames.TryGetValue(element, out var value) && value != null && value.enabled != show)
		{
			value.enabled = show;
		}
	}

	private static void AddSlotName(InventoryElement element, SlotCell cell)
	{
		if ((cell.IsEquipment || cell.Kind == SlotKind.Quiver || cell.Kind == SlotKind.Look) && !(element.m_amount == null))
		{
			TMP_Text tMP_Text = UnityEngine.Object.Instantiate(element.m_amount, element.m_amount.transform.parent, worldPositionStays: false);
			tMP_Text.name = "slotname";
			tMP_Text.enabled = false;
			tMP_Text.raycastTarget = false;
			tMP_Text.enableAutoSizing = false;
			tMP_Text.fontSize = 11f;
			tMP_Text.textWrappingMode = TextWrappingModes.NoWrap;
			tMP_Text.overflowMode = TextOverflowModes.Ellipsis;
			tMP_Text.alignment = TextAlignmentOptions.Bottom;
			tMP_Text.color = SlotNameColor;
			string text = Localization.instance.Localize(IsExtraBeltCell(cell) ? "$cdev_es_utility" : Placeholders.NameKey(SlotLayout.LookKind(cell)));
			int num = text.IndexOf(':');
			tMP_Text.text = ((num >= 0 && num + 1 < text.Length) ? text.Substring(num + 1).Trim() : text);
			RectTransform rectTransform = tMP_Text.rectTransform;
			Vector2 anchorMin = (rectTransform.anchorMax = new Vector2(0.5f, 0f));
			rectTransform.anchorMin = anchorMin;
			rectTransform.pivot = new Vector2(0.5f, 0f);
			rectTransform.sizeDelta = new Vector2(s_cellSize - 4f, 16f);
			rectTransform.anchoredPosition = new Vector2(0f, 3f);
			SlotNames[element] = tMP_Text;
		}
	}

	private static void ChooseCells(Inventory inventory)
	{
		BuildSheet(inventory);
		Wanted.Clear();
		Wanted.AddRange(Sheet);
		Wanted.AddRange(SlotLayout.Quick);
		bool flag = false;
		foreach (SlotCell item in SlotLayout.Quiver)
		{
			flag |= SlotInventory.ItemIn(inventory, item) != null;
		}
		if (flag || BetterArcheryHost.IsQuiverEquipped())
		{
			Wanted.AddRange(SlotLayout.Quiver);
		}
		foreach (SlotCell item2 in SlotLayout.ExtraBelt)
		{
			if (SlotLayout.IsOpen(item2) || SlotInventory.ItemIn(inventory, item2) != null)
			{
				Wanted.Add(item2);
			}
		}
		foreach (SlotCell item3 in SlotLayout.Stash)
		{
			if (SlotInventory.ItemIn(inventory, item3) != null)
			{
				Wanted.Add(item3);
			}
		}
	}

	private static void BuildSheet(Inventory inventory)
	{
		bool flag = Jewelry.AnyKnown || HoldsJewelry(inventory);
		bool backpack = BackpackHost.Active || SlotInventory.ItemIn(inventory, SlotLayout.Backpack) != null;
		(SlotKind, int)[] array = (flag ? LeftWithJewelry : LeftPlain);
		(SlotKind, int)[] array2 = (flag ? (backpack ? RightWithJewelryAndBackpack : RightWithJewelry) : (backpack ? RightPlainWithBackpack : RightPlain));
		s_sheetRows = Mathf.Max(array.Length, array2.Length);
		Sheet.Clear();
		SheetPlaces.Clear();
		AddColumn(array, 0f, spread: true);
		AddColumn(array2, 4f, spread: flag);
		if (LookMode)
		{
			for (int i = 0; i < Sheet.Count; i++)
			{
				int num = Appearance.IndexOf(Sheet[i].Kind);
				if (num >= 0 && Sheet[i].Ordinal == 0)
				{
					Sheet[i] = SlotLayout.Look[num];
				}
			}
		}
		if (InfoMode)
		{
			Sheet.Clear();
			SheetPlaces.Clear();
		}
	}

	// Spread: a shorter column is stretched over the sheet's height; otherwise it keeps one row apart from the top
	private static void AddColumn((SlotKind kind, int ordinal)[] column, float col, bool spread)
	{
		float num = ((!spread) ? 1f : ((column.Length > 1) ? (((float)s_sheetRows - 1f) / (float)(column.Length - 1)) : 0f));
		for (int i = 0; i < column.Length; i++)
		{
			Sheet.Add(Find(SlotLayout.Equipment, column[i].kind, column[i].ordinal));
			SheetPlaces.Add(new Vector2(col, (float)i * num));
		}
	}

	private static bool HoldsJewelry(Inventory inventory)
	{
		foreach (SlotCell item in SlotLayout.Equipment)
		{
			if (SlotLayout.IsJewelryKind(item.Kind) && SlotInventory.ItemIn(inventory, item) != null)
			{
				return true;
			}
		}
		return false;
	}

	private static SlotCell Find(List<SlotCell> cells, SlotKind kind, int ordinal)
	{
		foreach (SlotCell cell in cells)
		{
			if (cell.Kind == kind && cell.Ordinal == ordinal)
			{
				return cell;
			}
		}
		throw new InvalidOperationException($"No {kind} {ordinal} slot.");
	}

	private static bool SameCells()
	{
		if (Wanted.Count != Cells.Count || Grid.m_elements.Count != Cells.Count)
		{
			return false;
		}
		for (int i = 0; i < Wanted.Count; i++)
		{
			if (Wanted[i] != Cells[i])
			{
				return false;
			}
		}
		return true;
	}

	private static void Rebuild(InventoryGrid grid)
	{
		foreach (InventoryElement element in grid.m_elements)
		{
			if (element != null)
			{
				UnityEngine.Object.Destroy(element.gameObject);
			}
		}
		grid.m_elements.Clear();
		SlotNames.Clear();
		Dimmers.Clear();
		Cells.Clear();
		Cells.AddRange(Wanted);
		foreach (TMP_Text title in Titles)
		{
			if (title != null)
			{
				UnityEngine.Object.Destroy(title.gameObject);
			}
		}
		Titles.Clear();
		foreach (Image braid in Braids)
		{
			if (braid != null)
			{
				UnityEngine.Object.Destroy(braid.gameObject);
			}
		}
		Braids.Clear();
		CellPositions.Clear();
		float num = PadLeft + PadRight + 5f * s_space - (s_space - s_cellSize);
		float y = PadTop;
		if (!PlaceTabs(ref y, num))
		{
			AddTitle("$cdev_es_title", ref y, num);
		}
		float num2 = y;
		int count = Sheet.Count;
		for (int i = 0; i < count; i++)
		{
			CellPositions.Add(new Vector2(PadLeft + SheetPlaces[i].x * s_space, y + SheetPlaces[i].y * s_space));
		}
		RectTransform rectTransform = s_preview.rectTransform;
		Vector2 anchorMin = (rectTransform.anchorMax = new Vector2(0f, 1f));
		rectTransform.anchorMin = anchorMin;
		rectTransform.pivot = new Vector2(0f, 1f);
		rectTransform.anchoredPosition = new Vector2(PadLeft + s_space, 0f - y);
		rectTransform.sizeDelta = new Vector2(3f * s_space - (s_space - s_cellSize), (float)s_sheetRows * s_space - (s_space - s_cellSize));
		PlayerPreview.SetAspect(rectTransform.sizeDelta);
		RectTransform rectTransform2 = s_backing.rectTransform;
		anchorMin = (rectTransform2.anchorMax = rectTransform.anchorMin);
		rectTransform2.anchorMin = anchorMin;
		rectTransform2.pivot = rectTransform.pivot;
		rectTransform2.anchoredPosition = rectTransform.anchoredPosition;
		rectTransform2.sizeDelta = rectTransform.sizeDelta;
		PlaceHint(rectTransform);
		if (LookMode)
		{
			LookPicker.Place(rectTransform2, s_space, s_cellSize);
		}
		else
		{
			LookPicker.Close();
		}
		s_backing.enabled = !InfoMode;
		InfoView.Place(new Vector2(PadLeft, y), new Vector2(5f * s_space - (s_space - s_cellSize), (float)s_sheetRows * s_space - (s_space - s_cellSize)));
		InfoView.Show(InfoMode);
		y += (float)s_sheetRows * s_space;
		y = Section(count, SlotKind.Quick, "$cdev_es_quick", y);
		int num3 = CountCells(count, IsExtraBeltCell);
		float top = ((num3 > 0) ? (SideWindowHeight(num3) + 8f) : 0f);
		SideWindow(s_quiverWindow, count, num, onLeft: false, top, "$cdev_es_quiver", IsQuiverCell);
		SideWindow(s_utilityWindow, count, num, onLeft: false, 0f, "$cdev_es_utility", IsExtraBeltCell);
		y = Section(count, SlotKind.Stash, "$cdev_es_stash", y);
		float num4 = y - (s_space - s_cellSize) + PadBottom;
		if (s_bkg != null)
		{
			s_bkg.sizeDelta = new Vector2(num, num4);
		}
		s_root.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num);
		s_root.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num4);
		for (int j = 0; j < Cells.Count; j++)
		{
			grid.m_elements.Add(CreateElement(grid, Cells[j], CellPositions[j]));
		}
	}

	private static void MoveRepairTab(InventoryGui gui)
	{
		RectTransform crafting = gui.m_crafting;
		if (crafting == null)
		{
			return;
		}
		List<Transform> parts = new List<Transform>();
		Component[] array = new Component[4] { gui.m_repairPanel, gui.m_repairPanelSelection, gui.m_repairButton, gui.m_repairButtonGlow };
		foreach (Component component in array)
		{
			if (component != null && component.transform.IsChildOf(crafting) && !parts.Contains(component.transform))
			{
				parts.Add(component.transform);
			}
		}
		parts.RemoveAll((Transform t) => parts.Exists((Transform other) => other != t && t.IsChildOf(other)));
		if (parts.Count == 0)
		{
			return;
		}
		float num = float.MinValue;
		float num2 = float.MaxValue;
		foreach (Transform item in parts)
		{
			if (item is RectTransform rectTransform)
			{
				rectTransform.GetWorldCorners(Corners);
				for (int num3 = 0; num3 < 4; num3++)
				{
					float y = crafting.InverseTransformPoint(Corners[num3]).y;
					num = Mathf.Max(num, y);
					num2 = Mathf.Min(num2, y);
				}
			}
		}
		Rect rect = crafting.rect;
		if (num <= num2 || num > rect.yMax + 1f || num2 < rect.yMin - 1f)
		{
			return;
		}
		float num4 = rect.yMin;
		if (crafting.Find("Bkg") is RectTransform rectTransform2)
		{
			rectTransform2.GetWorldCorners(Corners);
			num4 = crafting.InverseTransformPoint(Corners[0]).y;
		}
		float num5 = num4 + 10f - num2;
		if (Mathf.Abs(num5) < 0.5f)
		{
			return;
		}
		Vector3 vector = crafting.TransformVector(new Vector3(0f, num5, 0f));
		foreach (Transform item2 in parts)
		{
			item2.position += vector;
		}
	}

	private static bool IsQuiverCell(SlotCell cell)
	{
		return cell.Kind == SlotKind.Quiver;
	}

	private static bool IsExtraBeltCell(SlotCell cell)
	{
		if (cell.Kind == SlotKind.Belt)
		{
			return cell.Ordinal > 0;
		}
		return false;
	}

	private static void SideWindow(RectTransform window, int start, float panelWidth, bool onLeft, float top, string key, Predicate<SlotCell> belongs)
	{
		int num = CountCells(start, belongs);
		if (window == null || num == 0)
		{
			if (window != null)
			{
				window.gameObject.SetActive(value: false);
			}
			return;
		}
		float sideTitleTop = SideTitleTop;
		float num2 = PadLeft + PadRight + s_cellSize;
		float num3 = (onLeft ? (0f - (8f + num2)) : (panelWidth + 8f));
		float y = SideWindowHeight(num);
		window.gameObject.SetActive(value: true);
		window.anchoredPosition = new Vector2(num3, 0f - top);
		window.sizeDelta = new Vector2(num2, y);
		AddWindowTitle(window, key, sideTitleTop);
		for (int i = 0; i < num; i++)
		{
			CellPositions.Add(new Vector2(num3 + PadLeft, top + sideTitleTop + 30f + (float)i * s_space));
		}
	}

	private static int CountCells(int start, Predicate<SlotCell> belongs)
	{
		int num = 0;
		for (int i = start; i < Cells.Count; i++)
		{
			if (belongs(Cells[i]))
			{
				num++;
			}
		}
		return num;
	}

	private static float SideWindowHeight(int count)
	{
		return SideTitleTop + PadBottom + 30f + (float)count * s_space - (s_space - s_cellSize);
	}

	private static void AddWindowTitle(RectTransform window, string key, float top)
	{
		if (!(s_titleTemplate == null))
		{
			TMP_Text tMP_Text = UnityEngine.Object.Instantiate(s_titleTemplate, window, worldPositionStays: false);
			tMP_Text.name = "Title";
			tMP_Text.gameObject.SetActive(value: true);
			tMP_Text.text = Localization.instance.Localize(key);
			tMP_Text.alignment = TextAlignmentOptions.Top;
			tMP_Text.enableAutoSizing = false;
			tMP_Text.fontSize = Mathf.Min(tMP_Text.fontSize, 16f);
			tMP_Text.textWrappingMode = TextWrappingModes.NoWrap;
			tMP_Text.overflowMode = TextOverflowModes.Overflow;
			RectTransform rectTransform = tMP_Text.rectTransform;
			rectTransform.anchorMin = new Vector2(0f, 1f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.pivot = new Vector2(0.5f, 1f);
			rectTransform.sizeDelta = new Vector2(0f, 30f);
			rectTransform.anchoredPosition = new Vector2(0f, 0f - top);
			Titles.Add(tMP_Text);
			AddBraids(window, tMP_Text, LiftToTop(tMP_Text, top, raiseHalf: false), window.sizeDelta.x);
		}
	}

	private static float Section(int start, SlotKind kind, string title, float y)
	{
		int num = 0;
		for (int i = start; i < Cells.Count; i++)
		{
			if (Cells[i].Kind == kind)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return y;
		}
		y += 6f;
		AddTitle(title, ref y, PadLeft + PadRight + 5f * s_space - (s_space - s_cellSize));
		int num2 = 0;
		for (int j = start; j < Cells.Count; j++)
		{
			if (Cells[j].Kind == kind)
			{
				int num3 = Math.Min(3, num - num2 / 3 * 3);
				float num4 = PadLeft + (float)(5 - num3) * s_space * 0.5f;
				CellPositions.Add(new Vector2(num4 + (float)(num2 % 3) * s_space, y + (float)(num2 / 3) * s_space));
				num2++;
			}
		}
		return y + (float)((num + 3 - 1) / 3) * s_space;
	}

	private static void PlaceHint(RectTransform previewRect)
	{
		if (InfoMode)
		{
			Graphic[] array = new Graphic[4] { s_hintAttack, s_hintBlock, s_iconAttack, s_iconBlock };
			foreach (Graphic graphic in array)
			{
				if (graphic != null)
				{
					graphic.enabled = false;
				}
			}
		}
		else if (LookMode)
		{
			Vector2 bottomCenter = previewRect.anchoredPosition + new Vector2(previewRect.sizeDelta.x * 0.5f, 0f - previewRect.sizeDelta.y + 5f);
			PlaceHintGroup(ref s_hintAttack, ref s_iconAttack, null, "$cdev_es_look_hint", bottomCenter, previewRect.sizeDelta.x, previewRect);
			if (s_hintBlock != null)
			{
				s_hintBlock.enabled = false;
			}
			if (s_iconBlock != null)
			{
				s_iconBlock.enabled = false;
			}
		}
		else
		{
			float num = previewRect.sizeDelta.x * 0.5f;
			Vector2 vector = previewRect.anchoredPosition + new Vector2(0f, 0f - previewRect.sizeDelta.y + 5f);
			Sprite sprite = GameSprite("mouse1_icon");
			Sprite sprite2 = GameSprite("mouse2_icon");
			PlaceHintGroup(ref s_hintAttack, ref s_iconAttack, sprite, (sprite != null) ? "$cdev_es_preview_attack_icon" : "$cdev_es_preview_attack", vector + new Vector2(num * 0.5f, 0f), num, previewRect);
			PlaceHintGroup(ref s_hintBlock, ref s_iconBlock, sprite2, (sprite2 != null) ? "$cdev_es_preview_block_icon" : "$cdev_es_preview_block", vector + new Vector2(num * 1.5f, 0f), num, previewRect);
		}
	}

	private static Sprite GameSprite(string name)
	{
		if (Sprites.TryGetValue(name, out var value) && value != null)
		{
			return value;
		}
		Sprite[] array = Resources.FindObjectsOfTypeAll<Sprite>();
		foreach (Sprite sprite in array)
		{
			if (sprite != null && sprite.name == name)
			{
				Sprites[name] = sprite;
				return sprite;
			}
		}
		return null;
	}

	private static void PlaceHintGroup(ref TMP_Text text, ref Image icon, Sprite sprite, string key, Vector2 bottomCenter, float width, RectTransform previewRect)
	{
		if (text == null && s_hintTemplate != null)
		{
			text = UnityEngine.Object.Instantiate(s_hintTemplate, s_root, worldPositionStays: false);
			text.name = "CharacterHint";
			text.gameObject.SetActive(value: true);
			text.raycastTarget = false;
			text.enableAutoSizing = false;
			text.fontSize = 14f;
			text.alignment = TextAlignmentOptions.BottomLeft;
			text.textWrappingMode = TextWrappingModes.NoWrap;
		}
		if (icon == null)
		{
			GameObject gameObject = new GameObject("CharacterHintIcon", typeof(RectTransform));
			gameObject.transform.SetParent(s_root, worldPositionStays: false);
			icon = gameObject.AddComponent<Image>();
			icon.raycastTarget = false;
			icon.preserveAspect = true;
		}
		bool value = ModConfig.PanelCharacter.Value;
		icon.sprite = sprite;
		icon.enabled = value && sprite != null;
		float num = (icon.enabled ? 23f : 0f);
		float num2 = 0f;
		if (text != null)
		{
			text.text = Localization.instance.Localize(key);
			text.enabled = value;
			num2 = Mathf.Min(text.GetPreferredValues(text.text).x, width - num);
		}
		float num3 = bottomCenter.x - (num + num2) * 0.5f;
		RectTransform rectTransform = icon.rectTransform;
		Vector2 anchorMin = (rectTransform.anchorMax = previewRect.anchorMin);
		rectTransform.anchorMin = anchorMin;
		rectTransform.pivot = new Vector2(0f, 0f);
		rectTransform.sizeDelta = new Vector2(20f, 20f);
		rectTransform.anchoredPosition = new Vector2(num3, bottomCenter.y - 2f);
		icon.transform.SetAsLastSibling();
		if (text != null)
		{
			RectTransform rectTransform2 = text.rectTransform;
			anchorMin = (rectTransform2.anchorMax = previewRect.anchorMin);
			rectTransform2.anchorMin = anchorMin;
			rectTransform2.pivot = new Vector2(0f, 0f);
			rectTransform2.sizeDelta = new Vector2(num2 + 2f, 20f);
			rectTransform2.anchoredPosition = new Vector2(num3 + num, bottomCenter.y);
			text.transform.SetAsLastSibling();
		}
	}

	private static void CreateTabs(InventoryGui gui)
	{
		if (!(gui.m_tabCraft == null) && !(s_bkg == null))
		{
			UITooltip uITooltip = ((Grid.m_elementPrefab != null) ? Grid.m_elementPrefab.GetComponentInChildren<UITooltip>(includeInactive: true) : null);
			s_tooltipPrefab = ((uITooltip != null) ? uITooltip.m_tooltipPrefab : null);
			s_tabGear = Tab(gui, "TabEquipment", "$cdev_es_title", PanelTab.Equipment, uITooltip, null);
			s_tabLook = Tab(gui, "TabAppearance", "$cdev_es_look", PanelTab.Style, uITooltip, "$cdev_es_look_tip");
			s_tabInfo = Tab(gui, "TabInfo", "$cdev_es_info", PanelTab.Info, uITooltip, "$cdev_es_info_tip");
		}
	}

	private static Button Tab(InventoryGui gui, string name, string key, PanelTab tab, UITooltip tooltipTemplate, string tooltipKey)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(gui.m_tabCraft.gameObject, s_bkg, worldPositionStays: false);
		gameObject.name = name;
		gameObject.SetActive(value: true);
		UIGamePad[] componentsInChildren = gameObject.GetComponentsInChildren<UIGamePad>(includeInactive: true);
		foreach (UIGamePad uIGamePad in componentsInChildren)
		{
			if (uIGamePad.m_hint != null)
			{
				UnityEngine.Object.Destroy(uIGamePad.m_hint);
			}
			UnityEngine.Object.Destroy(uIGamePad);
		}
		UITooltip[] componentsInChildren2 = gameObject.GetComponentsInChildren<UITooltip>(includeInactive: true);
		foreach (UITooltip obj in componentsInChildren2)
		{
			UnityEngine.Object.Destroy(obj);
		}
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		Vector2 vector = (rectTransform.pivot = new Vector2(0f, 1f));
		Vector2 anchorMin = (rectTransform.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		Button component = gameObject.GetComponent<Button>();
		component.onClick = new Button.ButtonClickedEvent();
		component.onClick.AddListener(() =>
		{
			SetTab(tab);
		});
		TMP_Text componentInChildren = gameObject.GetComponentInChildren<TMP_Text>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.text = Localization.instance.Localize(key);
		}
		if (tooltipKey != null && tooltipTemplate != null)
		{
			UITooltip uITooltip = gameObject.AddComponent<UITooltip>();
			uITooltip.m_tooltipPrefab = tooltipTemplate.m_tooltipPrefab;
			uITooltip.m_anchor = Grid.m_tooltipAnchor;
			uITooltip.m_topic = Localization.instance.Localize(key);
			uITooltip.m_text = Localization.instance.Localize(tooltipKey);
		}
		return component;
	}

	internal static void StepTab(int step)
	{
		SetTab((PanelTab)((int)(s_tab + step + 3) % 3));
	}

	private static void SetTab(PanelTab tab)
	{
		if (s_tab != tab)
		{
			s_tab = tab;
			LookPicker.Close();
			Invalidate();
		}
	}

	private static bool PlaceTabs(ref float y, float windowWidth)
	{
		if (s_tabGear == null || s_tabLook == null || s_tabInfo == null)
		{
			return false;
		}
		float num = windowWidth - PadLeft - PadRight;
		float num2 = (num - 12f) / 3f;
		float top = y + 2f;
		Place(s_tabGear, PadLeft, top, num2);
		Place(s_tabLook, PadLeft + num2 + 6f, top, num2);
		Place(s_tabInfo, PadLeft + 2f * (num2 + 6f), top, num2);
		Label(s_tabGear, "$cdev_es_title");
		Label(s_tabLook, "$cdev_es_look");
		Label(s_tabInfo, "$cdev_es_info");
		Tip(s_tabLook, "$cdev_es_look", "$cdev_es_look_tip");
		Tip(s_tabInfo, "$cdev_es_info", "$cdev_es_info_tip");
		s_tabGear.interactable = s_tab != PanelTab.Equipment;
		s_tabLook.interactable = s_tab != PanelTab.Style;
		s_tabInfo.interactable = s_tab != PanelTab.Info;
		y += 34f;
		return true;
	}

	private static void Tip(Button tab, string topic, string text)
	{
		UITooltip component = tab.GetComponent<UITooltip>();
		if (component != null)
		{
			component.m_topic = Localization.instance.Localize(topic);
			component.m_text = Localization.instance.Localize(text);
		}
	}

	private static void Label(Button tab, string key)
	{
		TMP_Text componentInChildren = tab.GetComponentInChildren<TMP_Text>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.text = Localization.instance.Localize(key);
		}
	}

	private static void Place(Button tab, float left, float top, float width)
	{
		RectTransform rectTransform = (RectTransform)tab.transform;
		rectTransform.sizeDelta = new Vector2(width, 28f);
		rectTransform.anchoredPosition = new Vector2(left, 0f - top);
		TMP_Text componentInChildren = tab.GetComponentInChildren<TMP_Text>(includeInactive: true);
		if (componentInChildren != null)
		{
			RectTransform rectTransform2 = componentInChildren.rectTransform;
			rectTransform2.anchorMin = Vector2.zero;
			rectTransform2.anchorMax = Vector2.one;
			rectTransform2.pivot = new Vector2(0.5f, 0.5f);
			rectTransform2.offsetMin = new Vector2(5f, 0f);
			rectTransform2.offsetMax = new Vector2(-5f, 0f);
			componentInChildren.enableAutoSizing = true;
			componentInChildren.fontSizeMin = 9f;
			componentInChildren.fontSizeMax = 16f;
			componentInChildren.textWrappingMode = TextWrappingModes.NoWrap;
			componentInChildren.overflowMode = TextOverflowModes.Overflow;
			componentInChildren.alignment = TextAlignmentOptions.Center;
		}
	}

	private static void AddTitle(string key, ref float y, float windowWidth)
	{
		if (s_titleTemplate == null)
		{
			y += 30f;
			return;
		}
		TMP_Text tMP_Text = UnityEngine.Object.Instantiate(s_titleTemplate, (s_bkg != null) ? s_bkg : s_root, worldPositionStays: false);
		tMP_Text.name = "Title";
		tMP_Text.gameObject.SetActive(value: true);
		tMP_Text.text = Localization.instance.Localize(key);
		RectTransform rectTransform = tMP_Text.rectTransform;
		rectTransform.anchorMin = new Vector2(0f, 1f);
		rectTransform.anchorMax = new Vector2(1f, 1f);
		rectTransform.pivot = new Vector2(0.5f, 1f);
		rectTransform.sizeDelta = new Vector2(0f - (PadLeft + PadRight), 30f);
		rectTransform.anchoredPosition = new Vector2(0f, 0f - (y + 8f));
		tMP_Text.alignment = TextAlignmentOptions.Top;
		tMP_Text.enableAutoSizing = false;
		tMP_Text.fontSize = 22f;
		tMP_Text.textWrappingMode = TextWrappingModes.NoWrap;
		tMP_Text.overflowMode = TextOverflowModes.Overflow;
		Titles.Add(tMP_Text);
		float middle = LiftToTop(tMP_Text, y + 8f, raiseHalf: true);
		AddBraids((s_bkg != null) ? s_bkg : s_root, tMP_Text, middle, windowWidth);
		y += 30f;
	}

	private static InventoryElement CreateElement(InventoryGrid grid, SlotCell cell, Vector2 topLeft)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(grid.m_elementPrefab, grid.m_gridRoot);
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		Vector2 anchorMin = (rectTransform.anchorMax = new Vector2(0f, 1f));
		rectTransform.anchorMin = anchorMin;
		rectTransform.pivot = new Vector2(0f, 1f);
		rectTransform.anchoredPosition = new Vector2(topLeft.x, 0f - topLeft.y);
		UIInputHandler componentInChildren = gameObject.GetComponentInChildren<UIInputHandler>();
		componentInChildren.m_onRightDown = (Action<UIInputHandler>)Delegate.Combine(componentInChildren.m_onRightDown, new Action<UIInputHandler>(grid.OnRightDown));
		componentInChildren.m_onLeftDown = (Action<UIInputHandler>)Delegate.Combine(componentInChildren.m_onLeftDown, new Action<UIInputHandler>(grid.OnLeftDown));
		componentInChildren.m_onLeftClick = (Action<UIInputHandler>)Delegate.Combine(componentInChildren.m_onLeftClick, new Action<UIInputHandler>(grid.OnLeftClick));
		componentInChildren.m_onLeftUp = (Action<UIInputHandler>)Delegate.Combine(componentInChildren.m_onLeftUp, new Action<UIInputHandler>(grid.OnLeftRelease));
		componentInChildren.m_onPointerEnter = (Action<UIInputHandler>)Delegate.Combine(componentInChildren.m_onPointerEnter, new Action<UIInputHandler>(grid.OnPointerEnter));
		UIDragHandler componentInChildren2 = gameObject.GetComponentInChildren<UIDragHandler>();
		componentInChildren2.m_onBeginDrag = (Action<UIDragHandler>)Delegate.Combine(componentInChildren2.m_onBeginDrag, new Action<UIDragHandler>(grid.OnBeginDrag));
		componentInChildren2.m_onEndDrag = (Action<UIDragHandler>)Delegate.Combine(componentInChildren2.m_onEndDrag, new Action<UIDragHandler>(grid.OnDragEnd));
		componentInChildren2.m_onReleasedOn = (Action<UIDragHandler>)Delegate.Combine(componentInChildren2.m_onReleasedOn, new Action<UIDragHandler>(grid.OnReleasedOn));
		Transform transform = gameObject.transform.Find("binding");
		TMP_Text tMP_Text = ((transform != null) ? transform.GetComponent<TMP_Text>() : null);
		if (tMP_Text != null)
		{
			string text = ((cell.Kind == SlotKind.Quick) ? QuickSlots.Label(cell.Ordinal) : null);
			tMP_Text.enabled = text != null;
			if (text != null)
			{
				tMP_Text.text = text;
				tMP_Text.enableAutoSizing = false;
				tMP_Text.fontSize = Mathf.Min(tMP_Text.fontSize, 13f);
				tMP_Text.alignment = TextAlignmentOptions.BottomRight;
				tMP_Text.overflowMode = TextOverflowModes.Overflow;
				RectTransform rectTransform2 = tMP_Text.rectTransform;
				Vector2 vector2 = (rectTransform2.pivot = new Vector2(1f, 0f));
				anchorMin = (rectTransform2.anchorMax = vector2);
				rectTransform2.anchorMin = anchorMin;
				rectTransform2.sizeDelta = new Vector2(s_cellSize - 6f, 16f);
				rectTransform2.anchoredPosition = new Vector2(-4f, 3f);
			}
		}
		InventoryElement component = gameObject.GetComponent<InventoryElement>();
		component.Initialize(cell.Pos.x, cell.Pos.y);
		AddSlotName(component, cell);
		return component;
	}

	internal static void Invalidate()
	{
		Cells.Clear();
	}

	internal static Vector2i PositionOf(int index)
	{
		if (index < 0 || index >= Cells.Count)
		{
			return new Vector2i(-1, -1);
		}
		return Cells[index].Pos;
	}

	internal static InventoryElement ElementAt(InventoryGrid grid, int x, int y)
	{
		for (int i = 0; i < Cells.Count && i < grid.m_elements.Count; i++)
		{
			if (Cells[i].Pos.x == x && Cells[i].Pos.y == y)
			{
				return grid.m_elements[i];
			}
		}
		return null;
	}

	private static void OnRightClick(InventoryGrid grid, ItemDrop.ItemData item, Vector2i pos)
	{
		SlotCell slotCell = SlotLayout.At(pos);
		if (slotCell == null)
		{
			return;
		}
		if (slotCell.Kind == SlotKind.Look)
		{
			Appearance.Set(Player.m_localPlayer, slotCell.Ordinal, null);
			LookPicker.Close();
			return;
		}
		InventoryGui instance = InventoryGui.instance;
		if (instance != null && (instance.m_dragGo != null || slotCell.Kind == SlotKind.Quick || slotCell.Kind == SlotKind.Quiver))
		{
			instance.OnRightClickItem(grid, item, pos);
		}
		else
		{
			EquipSync.OnSlotRightClick(Player.m_localPlayer, slotCell, item);
		}
	}
}
