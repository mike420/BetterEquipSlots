using System;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

internal static class CellLocks
{
	private const string ChildName = "CDEV_Lock";

	private const float Size = 15f;

	private const float Margin = 3f;

	private static readonly Color Open = new Color(1f, 1f, 1f, 0.16f);

	private static readonly Color Shut = new Color(1f, 0.82f, 0.45f, 1f);

	private static Sprite s_icon;

	internal static void Refresh(InventoryGrid grid, Player player)
	{
		bool value = ModConfig.CellLockIcons.Value;
		for (int i = 0; i < grid.m_elements.Count; i++)
		{
			InventoryElement inventoryElement = grid.m_elements[i];
			if (inventoryElement == null)
			{
				continue;
			}
			Transform transform = inventoryElement.transform.Find("CDEV_Lock");
			SlotCell slotCell = SlotLayout.At(inventoryElement.Position);
			if (!value || (slotCell != null && slotCell.Kind == SlotKind.Look))
			{
				if (transform != null)
				{
					transform.gameObject.SetActive(value: false);
				}
				continue;
			}
			Image image = ((transform != null) ? transform.GetComponent<Image>() : Create(inventoryElement));
			if (!(image == null))
			{
				if (!image.gameObject.activeSelf)
				{
					image.gameObject.SetActive(value: true);
				}
				Color color = (Locks.IsLocked(inventoryElement.Position) ? Shut : Open);
				if (image.color != color)
				{
					image.color = color;
				}
			}
		}
	}

	private static Image Create(InventoryElement element)
	{
		Sprite sprite = Icon();
		if (sprite == null)
		{
			return null;
		}
		GameObject gameObject = new GameObject("CDEV_Lock", typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(element.transform, worldPositionStays: false);
		Vector2 vector = (rectTransform.pivot = new Vector2(0f, 1f));
		Vector2 anchorMin = (rectTransform.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		rectTransform.sizeDelta = new Vector2(15f, 15f);
		rectTransform.anchoredPosition = new Vector2(3f, -3f);
		rectTransform.SetAsLastSibling();
		Image image = gameObject.AddComponent<Image>();
		image.sprite = sprite;
		image.preserveAspect = true;
		image.raycastTarget = true;
		AddTooltip(gameObject, element);
		Button button = gameObject.AddComponent<Button>();
		button.targetGraphic = image;
		button.transition = Selectable.Transition.None;
		InventoryElement cell = element;
		button.onClick.AddListener(() =>
		{
			Locks.Toggle(cell.Position);
		});
		return image;
	}

	private static void AddTooltip(GameObject go, InventoryElement element)
	{
		UITooltip tooltip = element.m_tooltip;
		if (!(tooltip == null) && !(tooltip.m_tooltipPrefab == null))
		{
			UITooltip uITooltip = go.AddComponent<UITooltip>();
			uITooltip.m_tooltipPrefab = tooltip.m_tooltipPrefab;
			uITooltip.m_anchor = tooltip.m_anchor;
			uITooltip.m_topic = Localization.instance.Localize("$cdev_es_lock");
			uITooltip.m_text = "";
		}
	}

	private static Sprite Icon()
	{
		if (s_icon != null)
		{
			return s_icon;
		}
		try
		{
			Texture2D texture2D = new Texture2D(64, 64, TextureFormat.RGBA32, mipChain: false)
			{
				filterMode = FilterMode.Bilinear
			};
			Color32[] array = new Color32[4096];
			for (int i = 0; i < 64; i++)
			{
				for (int j = 0; j < 64; j++)
				{
					int num = 0;
					for (int k = 0; k < 4; k++)
					{
						for (int l = 0; l < 4; l++)
						{
							if (Inside(j * 4 + l, i * 4 + k, 256))
							{
								num++;
							}
						}
					}
					byte a = (byte)(255 * num / 16);
					array[i * 64 + j] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, a);
				}
			}
			texture2D.SetPixels32(array);
			texture2D.Apply();
			s_icon = Sprite.Create(texture2D, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f));
			s_icon.name = "CDEV_LockIcon";
		}
		catch (Exception ex)
		{
			Plugin.Log.LogError("Could not draw the lock icon: " + ex);
		}
		return s_icon;
	}

	private static bool Inside(int px, int py, int big)
	{
		float num = (float)px / (float)big;
		float num2 = (float)py / (float)big;
		bool flag = RoundedBox(num, num2, 0.16f, 0.84f, 0.06f, 0.58f, 0.09f);
		float num3 = num - 0.5f;
		float num4 = num2 - 0.58f;
		float num5 = Mathf.Sqrt(num3 * num3 + num4 * num4);
		bool flag2 = num4 >= 0f && num5 <= 0.3f && num5 >= 0.185f;
		bool flag3 = num2 >= 0.46f && num2 <= 0.6f && Mathf.Abs(num3) >= 0.185f && Mathf.Abs(num3) <= 0.3f;
		if (!flag && !flag2 && !flag3)
		{
			return false;
		}
		float num6 = num - 0.5f;
		float num7 = num2 - 0.38f;
		bool flag4 = Mathf.Sqrt(num6 * num6 + num7 * num7) <= 0.075f;
		bool flag5 = num2 >= 0.19f && num2 <= 0.38f && Mathf.Abs(num6) <= 0.032f;
		if (!flag4)
		{
			return !flag5;
		}
		return false;
	}

	private static bool RoundedBox(float u, float v, float left, float right, float bottom, float top, float radius)
	{
		if (u < left || u > right || v < bottom || v > top)
		{
			return false;
		}
		float num = Mathf.Max(left + radius - u, u - (right - radius));
		float num2 = Mathf.Max(bottom + radius - v, v - (top - radius));
		if (num <= 0f || num2 <= 0f)
		{
			return true;
		}
		return num * num + num2 * num2 <= radius * radius;
	}
}
