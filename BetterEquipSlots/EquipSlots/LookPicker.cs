using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

internal static class LookPicker
{
	private sealed class Entry
	{
		internal GameObject Go;

		internal Image Back;

		internal Image Icon;

		internal UITooltip Tooltip;

		internal Button Button;

		internal GameObject Look;

		internal bool Hidden;
	}

	private static RectTransform s_root;

	private static Image s_style;

	private static TMP_Text s_textTemplate;

	private static GameObject s_tooltipPrefab;

	private static RectTransform s_window;

	private static TMP_Text s_emptyText;

	private static readonly List<Entry> Entries = new List<Entry>();

	private static readonly List<GameObject> Known = new List<GameObject>();

	private static readonly List<GameObject> Shown = new List<GameObject>();

	private static Button s_prev;

	private static Button s_next;

	private static TMP_Text s_pageText;

	private static float s_space = 70f;

	private static float s_cell = 64f;

	private static int s_page;

	private static int s_pages = 1;

	private static int s_perPage = 1;

	private const int Cols = 3;

	private static int s_pad = -1;

	private static readonly Color PadColor = new Color(0.55f, 0.8f, 1f, 0.9f);

	private static readonly Color Chosen = new Color(1f, 0.72f, 0.3f, 0.95f);

	private const int HiddenAt = 1;

	internal static int Index { get; private set; } = -1;

	internal static void Build(RectTransform root, Image style, TMP_Text textTemplate, GameObject tooltipPrefab)
	{
		s_root = root;
		s_style = style;
		s_textTemplate = textTemplate;
		s_tooltipPrefab = tooltipPrefab;
		Entries.Clear();
		GameObject gameObject = new GameObject("LookPicker", typeof(RectTransform));
		s_window = (RectTransform)gameObject.transform;
		s_window.SetParent(root, worldPositionStays: false);
		RectTransform rectTransform = s_window;
		RectTransform rectTransform2 = s_window;
		Vector2 vector = (s_window.pivot = new Vector2(0f, 1f));
		Vector2 anchorMin = (rectTransform2.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		Image image = gameObject.AddComponent<Image>();
		image.color = new Color(0.06f, 0.05f, 0.04f, 0.97f);
		image.raycastTarget = true;
		if (style != null && style.sprite != null)
		{
			image.sprite = style.sprite;
			image.type = style.type;
			image.pixelsPerUnitMultiplier = style.pixelsPerUnitMultiplier;
		}
		s_emptyText = Text("Empty", 13f, TextAlignmentOptions.Center);
		s_prev = PageButton("Prev", "<", -1);
		s_next = PageButton("Next", ">", 1);
		s_pageText = Text("Page", 13f, TextAlignmentOptions.Center);
		gameObject.SetActive(value: false);
	}

	internal static void Place(RectTransform area, float space, float cell)
	{
		if (!(s_window == null) && !(area == null))
		{
			s_space = space;
			s_cell = cell;
			s_window.anchoredPosition = area.anchoredPosition;
			s_window.sizeDelta = area.sizeDelta;
			if (Index >= 0)
			{
				Fill();
			}
		}
	}

	internal static void Toggle(int index)
	{
		if (Index == index)
		{
			Close();
			return;
		}
		Index = index;
		s_page = -1;
		s_pad = ((!GamepadPanel.Active) ? (-1) : 0);
		Fill();
	}

	internal static void Close()
	{
		Index = -1;
		s_pad = -1;
		if (s_window != null)
		{
			s_window.gameObject.SetActive(value: false);
		}
	}

	private static void Fill()
	{
		Player localPlayer = Player.m_localPlayer;
		if (s_window == null || localPlayer == null || Index < 0)
		{
			Close();
			return;
		}
		SlotKind kind = Appearance.Kinds[Index];
		Appearance.Known(localPlayer, kind, Known);
		Shown.Clear();
		Shown.Add(null);
		Shown.Add(null);
		Shown.AddRange(Known);
		Vector2 sizeDelta = s_window.sizeDelta;
		float num = Mathf.Max(4f, s_space - s_cell);
		float num2 = (sizeDelta.x - 20f + num) / 3f;
		float num3 = num2 - num;
		int num4 = Mathf.Max(2, Mathf.FloorToInt((sizeDelta.y - 20f + num) / num2));
		bool flag = Shown.Count > 3 * num4;
		int num5 = (flag ? (3 * (num4 - 1)) : (3 * num4));
		int num6 = (s_pages = (Shown.Count + num5 - 1) / num5);
		s_perPage = num5;
		if (s_page < 0)
		{
			GameObject gameObject = Appearance.Chosen(localPlayer, Index);
			int b = ((gameObject != null) ? Shown.IndexOf(gameObject) : (Appearance.IsHidden(localPlayer, Index) ? 1 : 0));
			s_page = Mathf.Max(0, b) / num5;
		}
		s_page = Mathf.Clamp(s_page, 0, num6 - 1);
		float num7 = (sizeDelta.x - (3f * num2 - num)) * 0.5f;
		float num8 = 10f;
		GameObject gameObject2 = Appearance.Chosen(localPlayer, Index);
		bool flag2 = gameObject2 == null && Appearance.IsHidden(localPlayer, Index);
		int num9 = s_page * num5;
		int num10 = Mathf.Min(num5, Shown.Count - num9);
		for (int i = 0; i < Mathf.Max(num10, Entries.Count); i++)
		{
			if (i >= num10)
			{
				Entries[i].Go.SetActive(value: false);
				continue;
			}
			Entry entry = ((i < Entries.Count) ? Entries[i] : NewEntry());
			GameObject gameObject3 = (entry.Look = Shown[num9 + i]);
			entry.Hidden = num9 + i == 1;
			entry.Go.SetActive(value: true);
			RectTransform rectTransform = (RectTransform)entry.Go.transform;
			rectTransform.sizeDelta = new Vector2(num3, num3);
			rectTransform.anchoredPosition = new Vector2(num7 + (float)(i % 3) * num2, 0f - (num8 + (float)(i / 3) * num2));
			bool flag3 = (entry.Hidden ? flag2 : (gameObject3 == gameObject2 && (gameObject3 != null || !flag2)));
			Image back = entry.Back;
			Color color;
			if (i == s_pad)
			{
				color = PadColor;
			}
			else
			{
				color = (flag3 ? Chosen : BackColor());
			}
			back.color = color;
			entry.Icon.sprite = ((gameObject3 != null) ? Appearance.Icon(gameObject3) : Placeholders.Get(kind));
			entry.Icon.color = ((gameObject3 != null) ? Color.white : Placeholders.Tint);
			entry.Icon.enabled = entry.Icon.sprite != null;
			HiddenMark.Show(entry.Icon, entry.Hidden);
			if (entry.Tooltip != null)
			{
				string text = (entry.Hidden ? "$cdev_es_look_hidden" : "$cdev_es_look_none");
				entry.Tooltip.m_topic = ((gameObject3 != null) ? Appearance.DisplayName(gameObject3) : Localization.instance.Localize(text));
				entry.Tooltip.m_text = ((gameObject3 != null) ? "" : Localization.instance.Localize(text + "_tip"));
			}
		}
		s_emptyText.gameObject.SetActive(Known.Count == 0);
		if (Known.Count == 0)
		{
			s_emptyText.text = Localization.instance.Localize("$cdev_es_look_empty");
			RectTransform rectTransform2 = s_emptyText.rectTransform;
			rectTransform2.sizeDelta = new Vector2(sizeDelta.x - 20f, sizeDelta.y - num2 - 20f);
			rectTransform2.anchoredPosition = new Vector2(10f, 0f - (num8 + num2));
		}
		s_prev.gameObject.SetActive(flag);
		s_next.gameObject.SetActive(flag);
		s_pageText.gameObject.SetActive(flag);
		if (flag)
		{
			float y = 0f - (num8 + (float)(num4 - 1) * num2);
			PlaceSmall((RectTransform)s_prev.transform, num7, y, num3);
			PlaceSmall((RectTransform)s_next.transform, num7 + 2f * num2, y, num3);
			RectTransform rectTransform3 = s_pageText.rectTransform;
			rectTransform3.sizeDelta = new Vector2(3f * num2 - num - 2f * num3, num3);
			rectTransform3.anchoredPosition = new Vector2(num7 + num3, y);
			s_pageText.text = $"{s_page + 1} / {num6}";
			s_prev.interactable = s_page > 0;
			s_next.interactable = s_page < num6 - 1;
		}
		s_window.SetAsLastSibling();
		s_window.gameObject.SetActive(value: true);
	}

	internal static void PadUpdate()
	{
		if (Index < 0)
		{
			return;
		}
		int num = 0;
		foreach (Entry entry in Entries)
		{
			if (entry.Go.activeSelf)
			{
				num++;
			}
		}
		if (num == 0)
		{
			return;
		}
		s_pad = Mathf.Clamp(s_pad, 0, num - 1);
		int num2 = 0;
		if (ZInput.GetButtonDown("JoyDPadLeft") || ZInput.GetButtonDown("JoyLStickLeft"))
		{
			num2 = -1;
		}
		else if (ZInput.GetButtonDown("JoyDPadRight") || ZInput.GetButtonDown("JoyLStickRight"))
		{
			num2 = 1;
		}
		else if (ZInput.GetButtonDown("JoyDPadUp") || ZInput.GetButtonDown("JoyLStickUp"))
		{
			num2 = -3;
		}
		else if (ZInput.GetButtonDown("JoyDPadDown") || ZInput.GetButtonDown("JoyLStickDown"))
		{
			num2 = 3;
		}
		if (num2 != 0)
		{
			int num3 = s_pad + num2;
			if (num3 < 0 && s_page > 0)
			{
				s_page--;
				s_pad = s_perPage - 1;
			}
			else if (num3 >= num && s_page < s_pages - 1)
			{
				s_page++;
				s_pad = 0;
			}
			else
			{
				s_pad = Mathf.Clamp(num3, 0, num - 1);
			}
			Fill();
		}
		else if (ZInput.GetButtonDown("JoyButtonA"))
		{
			Choose(Entries[s_pad]);
		}
		else if (ZInput.GetButtonDown("JoyButtonX") || ZInput.GetButtonDown("JoyButtonB"))
		{
			Close();
		}
	}

	private static void PlaceSmall(RectTransform rect, float x, float y, float cell)
	{
		rect.sizeDelta = new Vector2(cell, cell);
		rect.anchoredPosition = new Vector2(x, y);
	}

	private static Color BackColor()
	{
		Color result = ((s_style != null) ? s_style.color : new Color(0f, 0f, 0f, 0.35f));
		result.a = Mathf.Max(result.a, 0.6f);
		return result;
	}

	private static Entry NewEntry()
	{
		Entry entry = new Entry();
		entry.Go = new GameObject("Look", typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)entry.Go.transform;
		rectTransform.SetParent(s_window, worldPositionStays: false);
		Vector2 vector = (rectTransform.pivot = new Vector2(0f, 1f));
		Vector2 anchorMin = (rectTransform.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		entry.Back = entry.Go.AddComponent<Image>();
		if (s_style != null && s_style.sprite != null)
		{
			entry.Back.sprite = s_style.sprite;
			entry.Back.type = s_style.type;
			entry.Back.pixelsPerUnitMultiplier = s_style.pixelsPerUnitMultiplier;
		}
		entry.Button = entry.Go.AddComponent<Button>();
		entry.Button.targetGraphic = entry.Back;
		entry.Button.onClick.AddListener(() =>
		{
			Choose(entry);
		});
		GameObject gameObject = new GameObject("Icon", typeof(RectTransform));
		RectTransform rectTransform2 = (RectTransform)gameObject.transform;
		rectTransform2.SetParent(rectTransform, worldPositionStays: false);
		rectTransform2.anchorMin = Vector2.zero;
		rectTransform2.anchorMax = Vector2.one;
		rectTransform2.offsetMin = new Vector2(6f, 6f);
		rectTransform2.offsetMax = new Vector2(-6f, -6f);
		entry.Icon = gameObject.AddComponent<Image>();
		entry.Icon.preserveAspect = true;
		entry.Icon.raycastTarget = false;
		if (s_tooltipPrefab != null)
		{
			entry.Tooltip = entry.Go.AddComponent<UITooltip>();
			entry.Tooltip.m_tooltipPrefab = s_tooltipPrefab;
			entry.Tooltip.m_anchor = ((SlotPanel.Grid != null) ? SlotPanel.Grid.m_tooltipAnchor : null);
		}
		Entries.Add(entry);
		return entry;
	}

	private static void Choose(Entry entry)
	{
		Player localPlayer = Player.m_localPlayer;
		if (localPlayer != null && Index >= 0)
		{
			Player player = localPlayer;
			int index = Index;
			string prefab;
			if (entry.Hidden)
			{
				prefab = "-";
			}
			else
			{
				prefab = ((entry.Look != null) ? entry.Look.name : null);
			}
			Appearance.Set(player, index, prefab);
		}
		Close();
	}

	private static Button PageButton(string name, string label, int step)
	{
		GameObject gameObject = new GameObject(name, typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(s_window, worldPositionStays: false);
		Vector2 vector = (rectTransform.pivot = new Vector2(0f, 1f));
		Vector2 anchorMin = (rectTransform.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		Image image = gameObject.AddComponent<Image>();
		image.color = BackColor();
		if (s_style != null && s_style.sprite != null)
		{
			image.sprite = s_style.sprite;
			image.type = s_style.type;
			image.pixelsPerUnitMultiplier = s_style.pixelsPerUnitMultiplier;
		}
		Button button = gameObject.AddComponent<Button>();
		button.targetGraphic = image;
		button.onClick.AddListener(() =>
		{
			s_page += step;
			Fill();
		});
		TMP_Text tMP_Text = Text(name + "Label", 22f, TextAlignmentOptions.Center, rectTransform);
		tMP_Text.text = label;
		RectTransform rectTransform2 = tMP_Text.rectTransform;
		rectTransform2.anchorMin = Vector2.zero;
		rectTransform2.anchorMax = Vector2.one;
		anchorMin = (rectTransform2.offsetMax = Vector2.zero);
		rectTransform2.offsetMin = anchorMin;
		return button;
	}

	private static TMP_Text Text(string name, float size, TextAlignmentOptions alignment, RectTransform parent = null)
	{
		TMP_Text tMP_Text = Object.Instantiate(s_textTemplate, (parent != null) ? parent : s_window, worldPositionStays: false);
		tMP_Text.name = name;
		tMP_Text.gameObject.SetActive(value: true);
		tMP_Text.raycastTarget = false;
		tMP_Text.enableAutoSizing = false;
		tMP_Text.fontSize = size;
		tMP_Text.alignment = alignment;
		tMP_Text.textWrappingMode = TextWrappingModes.Normal;
		RectTransform rectTransform = tMP_Text.rectTransform;
		Vector2 vector = (rectTransform.pivot = new Vector2(0f, 1f));
		Vector2 anchorMin = (rectTransform.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		return tMP_Text;
	}
}
