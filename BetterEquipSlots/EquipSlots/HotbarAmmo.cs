using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

internal static class HotbarAmmo
{
	private const string ChildName = "CDEV_Ammo";

	private const float StripHeight = 34f;

	private static readonly Color Low = new Color(1f, 0.45f, 0.4f, 1f);

	internal static void Update(HotkeyBar bar, Player player)
	{
		bool flag = ModConfig.AmmoOnHotbar.Value && player != null && !player.IsDead() && !BetterArcheryHost.QuiverHudShown && !InventoryGui.IsVisible();
		for (int i = 0; i < bar.m_elements.Count; i++)
		{
			HotkeyBar.ElementData elementData = bar.m_elements[i];
			if (elementData?.m_go == null)
			{
				continue;
			}
			ItemDrop.ItemData itemData = (flag ? ItemAt(bar, i) : null);
			Transform transform = elementData.m_go.transform.Find("CDEV_Ammo");
			if (transform != null && (transform.Find("Box/Icon") == null || transform.Find("Box/Count") == null))
			{
				Object.Destroy(transform.gameObject);
				transform.SetParent(null, worldPositionStays: false);
				transform = null;
			}
			ItemDrop.ItemData ammo = ((itemData != null && !string.IsNullOrEmpty(itemData.m_shared.m_ammoType)) ? NextAmmo(player, itemData) : null);
			if (itemData == null || string.IsNullOrEmpty(itemData.m_shared.m_ammoType) || IsOwnAmmo(itemData, ammo))
			{
				if (transform != null && transform.gameObject.activeSelf)
				{
					transform.gameObject.SetActive(value: false);
				}
				continue;
			}
			Transform transform2 = ((transform != null) ? transform : Create(elementData));
			if (!transform2.gameObject.activeSelf)
			{
				transform2.gameObject.SetActive(value: true);
			}
			Draw(transform2, player, ammo);
		}
	}

	private static bool IsOwnAmmo(ItemDrop.ItemData weapon, ItemDrop.ItemData ammo)
	{
		if ((weapon.IsWeapon() || weapon.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Tool) && !(weapon.m_shared.m_ammoType == weapon.m_shared.m_name))
		{
			if (ammo != null)
			{
				if (ammo != weapon)
				{
					return ammo.m_shared.m_name == weapon.m_shared.m_name;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	internal static void Destroy()
	{
		HotkeyBar hotkeyBar = ((Hud.instance != null) ? Hud.instance.GetComponentInChildren<HotkeyBar>(includeInactive: true) : null);
		if (hotkeyBar == null)
		{
			return;
		}
		foreach (HotkeyBar.ElementData element in hotkeyBar.m_elements)
		{
			Transform transform = ((element?.m_go != null) ? element.m_go.transform.Find("CDEV_Ammo") : null);
			if (transform != null)
			{
				transform.SetParent(null, worldPositionStays: false);
				Object.Destroy(transform.gameObject);
			}
		}
	}

	private static ItemDrop.ItemData ItemAt(HotkeyBar bar, int column)
	{
		for (int i = 0; i < bar.m_items.Count; i++)
		{
			if (bar.m_items[i].m_gridPos.x == column)
			{
				return bar.m_items[i];
			}
		}
		return null;
	}

	internal static ItemDrop.ItemData NextAmmo(Humanoid owner, ItemDrop.ItemData weapon)
	{
		Inventory inventory = owner.GetInventory();
		ItemDrop.ItemData ammoItem = owner.GetAmmoItem();
		if (ammoItem != null && inventory.ContainsItem(ammoItem) && ammoItem.m_shared.m_ammoType == weapon.m_shared.m_ammoType)
		{
			return ammoItem;
		}
		return inventory.GetAmmoItem(weapon.m_shared.m_ammoType);
	}

	private static void Draw(Transform view, Player player, ItemDrop.ItemData ammo)
	{
		Transform transform = view.Find("Box");
		Image component = transform.Find("Icon").GetComponent<Image>();
		TMP_Text component2 = transform.Find("Count").GetComponent<TMP_Text>();
		int num = 0;
		if (ammo != null)
		{
			foreach (ItemDrop.ItemData item in player.GetInventory().m_inventory)
			{
				if (item.m_shared.m_name == ammo.m_shared.m_name)
				{
					num += item.m_stack;
				}
			}
		}
		Sprite sprite = ammo?.GetIcon();
		if (component.sprite != sprite)
		{
			component.sprite = sprite;
		}
		component.enabled = sprite != null;
		string text = num.ToString();
		if (component2.text != text)
		{
			component2.text = text;
		}
		component2.color = ((num > 0) ? Color.white : Low);
	}

	private static Image Background(HotkeyBar.ElementData element, Transform ours)
	{
		Transform transform = element.m_go.transform.Find("bkg");
		Image image = ((transform != null) ? transform.GetComponent<Image>() : null);
		if (image != null && image.sprite != null)
		{
			return image;
		}
		Image[] componentsInChildren = element.m_go.GetComponentsInChildren<Image>(includeInactive: true);
		foreach (Image image2 in componentsInChildren)
		{
			if (!(image2 == null) && !(image2.sprite == null) && !(image2 == element.m_icon) && !image2.transform.IsChildOf(ours) && (!(element.m_equiped != null) || !image2.transform.IsChildOf(element.m_equiped.transform)) && (!(element.m_queued != null) || !image2.transform.IsChildOf(element.m_queued.transform)) && (!(element.m_selection != null) || !image2.transform.IsChildOf(element.m_selection.transform)) && (!(element.m_durability != null) || !image2.transform.IsChildOf(element.m_durability.transform)))
			{
				return image2;
			}
		}
		return null;
	}

	private static Transform Create(HotkeyBar.ElementData element)
	{
		GameObject gameObject = new GameObject("CDEV_Ammo", typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(element.m_go.transform, worldPositionStays: false);
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		Vector2 offsetMin = (rectTransform.offsetMax = Vector2.zero);
		rectTransform.offsetMin = offsetMin;
		gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
		GameObject gameObject2 = new GameObject("Box", typeof(RectTransform));
		RectTransform rectTransform2 = (RectTransform)gameObject2.transform;
		rectTransform2.SetParent(rectTransform, worldPositionStays: false);
		rectTransform2.anchorMin = new Vector2(0f, 1f);
		rectTransform2.anchorMax = new Vector2(1f, 1f);
		rectTransform2.pivot = new Vector2(0.5f, 0f);
		rectTransform2.anchoredPosition = new Vector2(0f, 3f);
		rectTransform2.sizeDelta = new Vector2(0f, 34f);
		Image image = gameObject2.AddComponent<Image>();
		image.raycastTarget = false;
		Image image2 = Background(element, rectTransform);
		if (image2 != null)
		{
			image.sprite = image2.sprite;
			image.type = image2.type;
			Color color = image2.color * image2.canvasRenderer.GetColor();
			image.color = ((color.r + color.g + color.b > 2.7f && color.a > 0.9f) ? new Color(0f, 0f, 0f, 0.3f) : color);
			image.material = image2.material;
			float height = ((RectTransform)element.m_go.transform).rect.height;
			image.pixelsPerUnitMultiplier = image2.pixelsPerUnitMultiplier * Mathf.Max(1f, (height > 1f) ? (height / 34f) : 2f);
		}
		else
		{
			image.color = new Color(0f, 0f, 0f, 0.3f);
		}
		GameObject gameObject3 = new GameObject("Icon", typeof(RectTransform));
		RectTransform rectTransform3 = (RectTransform)gameObject3.transform;
		rectTransform3.SetParent(rectTransform2, worldPositionStays: false);
		offsetMin = (rectTransform3.anchorMax = new Vector2(0f, 0.5f));
		rectTransform3.anchorMin = offsetMin;
		rectTransform3.pivot = new Vector2(0f, 0.5f);
		rectTransform3.anchoredPosition = new Vector2(2f, 0f);
		rectTransform3.sizeDelta = new Vector2(30f, 30f);
		Image image3 = gameObject3.AddComponent<Image>();
		image3.preserveAspect = true;
		image3.raycastTarget = false;
		GameObject gameObject4 = new GameObject("Count", typeof(RectTransform));
		RectTransform rectTransform4 = (RectTransform)gameObject4.transform;
		rectTransform4.SetParent(rectTransform2, worldPositionStays: false);
		offsetMin = (rectTransform4.anchorMax = new Vector2(1f, 0.5f));
		rectTransform4.anchorMin = offsetMin;
		rectTransform4.pivot = new Vector2(1f, 0.5f);
		rectTransform4.anchoredPosition = new Vector2(-4f, 0f);
		rectTransform4.sizeDelta = new Vector2(44f, 30f);
		TextMeshProUGUI textMeshProUGUI = gameObject4.AddComponent<TextMeshProUGUI>();
		TMP_Text amount = element.m_amount;
		if (amount != null)
		{
			textMeshProUGUI.font = amount.font;
			textMeshProUGUI.fontSharedMaterial = amount.fontSharedMaterial;
		}
		textMeshProUGUI.fontSize = 16f;
		textMeshProUGUI.alignment = TextAlignmentOptions.MidlineRight;
		textMeshProUGUI.textWrappingMode = TextWrappingModes.NoWrap;
		textMeshProUGUI.raycastTarget = false;
		textMeshProUGUI.outlineWidth = 0.2f;
		textMeshProUGUI.outlineColor = new Color32(0, 0, 0, 220);
		return rectTransform;
	}
}
