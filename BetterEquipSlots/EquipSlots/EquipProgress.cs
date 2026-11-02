using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

internal static class EquipProgress
{
	private const string ChildName = "CDEV_EquipProgress";

	private static readonly Color RingColor = new Color(1f, 1f, 1f, 1f);

	private static readonly Color TrackColor = new Color(0f, 0f, 0f, 0.75f);

	private static readonly Color ShadeColor = new Color(0f, 0f, 0f, 0.6f);

	private static Sprite s_ring;

	private static readonly Dictionary<InventoryGrid, List<Image>> Shown = new Dictionary<InventoryGrid, List<Image>>();

	private static readonly List<Image> Still = new List<Image>();

	private static Sprite s_outline;

	internal static void Draw(InventoryGrid grid, Player player)
	{
		if (player == null || grid.m_inventory != ((Humanoid)player).m_inventory)
		{
			return;
		}
		Shown.TryGetValue(grid, out var value);
		Still.Clear();
		List<Player.MinorActionData> actionQueue = player.m_actionQueue;
		for (int i = 0; i < actionQueue.Count; i++)
		{
			Player.MinorActionData minorActionData = actionQueue[i];
			if (minorActionData.m_item == null || (minorActionData.m_type != Player.MinorActionData.ActionType.Equip && minorActionData.m_type != Player.MinorActionData.ActionType.Unequip))
			{
				continue;
			}
			InventoryElement inventoryElement = ElementAt(grid, minorActionData.m_item.m_gridPos);
			if (!(inventoryElement == null))
			{
				float num = ((i == 0 && minorActionData.m_duration > 0f) ? Mathf.Clamp01(minorActionData.m_time / minorActionData.m_duration) : 0f);
				Image image = RingOf(inventoryElement);
				image.fillAmount = num;
				Image component = image.transform.parent.parent.GetComponent<Image>();
				component.fillAmount = 1f - num;
				GameObject gameObject = image.transform.parent.parent.gameObject;
				if (!gameObject.activeSelf)
				{
					gameObject.SetActive(value: true);
				}
				Still.Add(image);
			}
		}
		if (value != null)
		{
			foreach (Image item in value)
			{
				if (item != null && !Still.Contains(item))
				{
					item.transform.parent.parent.gameObject.SetActive(value: false);
				}
			}
			value.Clear();
			value.AddRange(Still);
		}
		else if (Still.Count > 0)
		{
			Shown[grid] = new List<Image>(Still);
		}
		Still.Clear();
	}

	private static InventoryElement ElementAt(InventoryGrid grid, Vector2i pos)
	{
		List<InventoryElement> elements = grid.m_elements;
		for (int i = 0; i < elements.Count; i++)
		{
			if (elements[i] != null && elements[i].Position.x == pos.x && elements[i].Position.y == pos.y)
			{
				return elements[i];
			}
		}
		return null;
	}

	private static Image RingOf(InventoryElement element)
	{
		Transform transform = element.transform.Find("CDEV_EquipProgress");
		if (transform != null)
		{
			return transform.GetChild(0).GetChild(0).GetComponent<Image>();
		}
		GameObject gameObject = new GameObject("CDEV_EquipProgress", typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(element.transform, worldPositionStays: false);
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.offsetMin = new Vector2(2f, 2f);
		rectTransform.offsetMax = new Vector2(-2f, -2f);
		Image image = gameObject.AddComponent<Image>();
		image.color = ShadeColor;
		image.raycastTarget = false;
		image.type = Image.Type.Filled;
		image.fillMethod = Image.FillMethod.Radial360;
		image.fillOrigin = 2;
		image.fillClockwise = false;
		image.fillAmount = 1f;
		GameObject gameObject2 = new GameObject("Track", typeof(RectTransform));
		RectTransform rectTransform2 = (RectTransform)gameObject2.transform;
		rectTransform2.SetParent(rectTransform, worldPositionStays: false);
		rectTransform2.anchorMin = Vector2.zero;
		rectTransform2.anchorMax = Vector2.one;
		rectTransform2.offsetMin = new Vector2(3f, 3f);
		rectTransform2.offsetMax = new Vector2(-3f, -3f);
		Image image2 = gameObject2.AddComponent<Image>();
		image2.sprite = Ring(outline: true);
		image2.color = TrackColor;
		image2.raycastTarget = false;
		GameObject gameObject3 = new GameObject("Fill", typeof(RectTransform));
		RectTransform rectTransform3 = (RectTransform)gameObject3.transform;
		rectTransform3.SetParent(rectTransform2, worldPositionStays: false);
		rectTransform3.anchorMin = Vector2.zero;
		rectTransform3.anchorMax = Vector2.one;
		rectTransform3.offsetMin = Vector2.zero;
		rectTransform3.offsetMax = Vector2.zero;
		Image image3 = gameObject3.AddComponent<Image>();
		image3.sprite = Ring(outline: false);
		image3.color = RingColor;
		image3.raycastTarget = false;
		image3.type = Image.Type.Filled;
		image3.fillMethod = Image.FillMethod.Radial360;
		image3.fillOrigin = 2;
		image3.fillClockwise = true;
		image3.fillAmount = 0f;
		gameObject.transform.SetAsLastSibling();
		return image3;
	}

	private static Sprite Ring(bool outline)
	{
		ref Sprite reference = ref outline ? ref s_outline : ref s_ring;
		if (reference != null)
		{
			return reference;
		}
		float num = (outline ? 63f : 59f);
		float num2 = (outline ? 22f : 14f);
		Texture2D texture2D = new Texture2D(128, 128, TextureFormat.RGBA32, mipChain: false)
		{
			name = "CDEV_EquipProgressRing",
			wrapMode = TextureWrapMode.Clamp
		};
		Color32[] array = new Color32[16384];
		float num3 = 63.5f;
		for (int i = 0; i < 128; i++)
		{
			for (int j = 0; j < 128; j++)
			{
				float num4 = Mathf.Sqrt(((float)j - num3) * ((float)j - num3) + ((float)i - num3) * ((float)i - num3));
				float num5 = Mathf.Min(num - num4, num4 - (num - num2));
				byte a = (byte)(Mathf.Clamp01(num5 + 0.5f) * 255f);
				array[i * 128 + j] = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, a);
			}
		}
		texture2D.SetPixels32(array);
		texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		reference = Sprite.Create(texture2D, new Rect(0f, 0f, 128f, 128f), new Vector2(0.5f, 0.5f), 100f);
		reference.name = texture2D.name;
		return reference;
	}
}
