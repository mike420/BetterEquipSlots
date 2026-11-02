using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

internal static class HiddenMark
{
	private const string ChildName = "CDEV_HiddenMark";

	private static readonly Color Red = new Color(0.86f, 0.22f, 0.18f, 0.95f);

	private static Sprite s_icon;

	internal static void Show(Image icon, bool show)
	{
		if (icon == null)
		{
			return;
		}
		Transform transform = ((icon.transform.parent != null) ? icon.transform.parent.Find("CDEV_HiddenMark") : null);
		if (!show)
		{
			if (transform != null && transform.gameObject.activeSelf)
			{
				transform.gameObject.SetActive(value: false);
			}
			return;
		}
		if (transform == null)
		{
			GameObject gameObject = new GameObject("CDEV_HiddenMark", typeof(RectTransform));
			RectTransform rectTransform = (RectTransform)gameObject.transform;
			rectTransform.SetParent(icon.transform.parent, worldPositionStays: false);
			RectTransform rectTransform2 = (RectTransform)icon.transform;
			rectTransform.anchorMin = rectTransform2.anchorMin;
			rectTransform.anchorMax = rectTransform2.anchorMax;
			rectTransform.pivot = rectTransform2.pivot;
			rectTransform.anchoredPosition = rectTransform2.anchoredPosition;
			rectTransform.sizeDelta = rectTransform2.sizeDelta;
			rectTransform.offsetMin = rectTransform2.offsetMin;
			rectTransform.offsetMax = rectTransform2.offsetMax;
			Image image = gameObject.AddComponent<Image>();
			image.sprite = Icon();
			image.preserveAspect = true;
			image.raycastTarget = false;
			transform = rectTransform;
		}
		transform.SetSiblingIndex(icon.transform.GetSiblingIndex() + 1);
		if (!transform.gameObject.activeSelf)
		{
			transform.gameObject.SetActive(value: true);
		}
	}

	private static Sprite Icon()
	{
		if (s_icon != null)
		{
			return s_icon;
		}
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
						float num2 = ((float)j + ((float)l + 0.5f) / 4f) / 64f - 0.5f;
						float num3 = ((float)i + ((float)k + 0.5f) / 4f) / 64f - 0.5f;
						float num4 = Mathf.Sqrt(num2 * num2 + num3 * num3);
						bool flag = num4 <= 0.44f && num4 >= 0.34f;
						bool flag2 = num4 < 0.34f && Mathf.Abs(num2 + num3) * 0.70710677f <= 0.05f;
						if (flag | flag2)
						{
							num++;
						}
					}
				}
				Color red = Red;
				red.a *= (float)num / 16f;
				array[i * 64 + j] = red;
			}
		}
		texture2D.SetPixels32(array);
		texture2D.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		s_icon = Sprite.Create(texture2D, new Rect(0f, 0f, 64f, 64f), new Vector2(0.5f, 0.5f));
		s_icon.name = "CDEV_HiddenIcon";
		return s_icon;
	}
}
