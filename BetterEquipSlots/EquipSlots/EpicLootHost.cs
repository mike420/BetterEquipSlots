using System;
using System.Reflection;
using BepInEx.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

internal static class EpicLootHost
{
	internal const string Guid = "randyknapp.mods.epicloot";

	private const string ChildName = "CDEV_MagicItem";

	private static Func<ItemDrop.ItemData, bool> s_isMagic;

	private static Func<ItemDrop.ItemData, Color> s_color;

	private static Func<Sprite> s_sprite;

	private static Func<GameObject, ItemDrop.ItemData, bool> s_icon;

	private static bool s_failed;

	internal static void Setup()
	{
		if (!Chainloader.PluginInfos.TryGetValue("randyknapp.mods.epicloot", out var value) || value.Instance == null)
		{
			return;
		}
		try
		{
			Assembly assembly = value.Instance.GetType().Assembly;
			Type type = assembly.GetType("EpicLoot.ItemDataExtensions");
			MethodInfo methodInfo = type?.GetMethod("UseMagicBackground", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(ItemDrop.ItemData) }, null);
			MethodInfo methodInfo2 = type?.GetMethod("GetRarityColor", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(ItemDrop.ItemData) }, null);
			MethodInfo methodInfo3 = assembly.GetType("EpicLoot.EpicLoot")?.GetMethod("GetMagicItemBgSprite", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
			MethodInfo methodInfo4 = assembly.GetType("EpicLoot.API")?.GetMethod("ApplyMagicItemBackgroundToIcon", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
			{
				typeof(GameObject),
				typeof(ItemDrop.ItemData)
			}, null);
			if (methodInfo == null || methodInfo2 == null || methodInfo3 == null || methodInfo4 == null)
			{
				Plugin.Log.LogWarning($"Epic Loot {value.Metadata.Version}: its item backgrounds were not found, so the equipment " + "panel shows magic items without their rarity colour.");
				return;
			}
			s_isMagic = (Func<ItemDrop.ItemData, bool>)Delegate.CreateDelegate(typeof(Func<ItemDrop.ItemData, bool>), methodInfo);
			s_color = (Func<ItemDrop.ItemData, Color>)Delegate.CreateDelegate(typeof(Func<ItemDrop.ItemData, Color>), methodInfo2);
			s_sprite = (Func<Sprite>)Delegate.CreateDelegate(typeof(Func<Sprite>), methodInfo3);
			s_icon = (Func<GameObject, ItemDrop.ItemData, bool>)Delegate.CreateDelegate(typeof(Func<GameObject, ItemDrop.ItemData, bool>), methodInfo4);
		}
		catch (Exception ex)
		{
			Plugin.Log.LogWarning("Could not bind Epic Loot's item backgrounds: " + ex.Message);
			s_isMagic = null;
		}
	}

	internal static bool IsMagic(ItemDrop.ItemData item)
	{
		if (s_isMagic == null || s_failed || item == null)
		{
			return false;
		}
		try
		{
			return s_isMagic(item);
		}
		catch (Exception ex)
		{
			s_failed = true;
			Plugin.Log.LogWarning("Epic Loot's item background failed, switched off for this session: " + ex.Message);
			return false;
		}
	}

	internal static void Cell(InventoryElement element, ItemDrop.ItemData item)
	{
		if (s_isMagic == null || s_failed || element == null || element.m_equiped == null)
		{
			return;
		}
		try
		{
			bool flag = item != null && s_isMagic(item);
			Transform transform = element.transform.Find("CDEV_MagicItem");
			if (!flag)
			{
				if (transform != null && transform.gameObject.activeSelf)
				{
					transform.gameObject.SetActive(value: false);
				}
				return;
			}
			Image image = ((transform != null) ? transform.GetComponent<Image>() : Create(element.m_equiped));
			int siblingIndex = element.m_icon.transform.GetSiblingIndex();
			if (image.transform.GetSiblingIndex() != siblingIndex - 1)
			{
				image.transform.SetSiblingIndex((image.transform.GetSiblingIndex() < siblingIndex) ? (siblingIndex - 1) : siblingIndex);
			}
			Sprite sprite = s_sprite();
			if (image.sprite != sprite)
			{
				image.sprite = sprite;
			}
			Color color = s_color(item);
			if (image.color != color)
			{
				image.color = color;
			}
			if (!image.gameObject.activeSelf)
			{
				image.gameObject.SetActive(value: true);
			}
		}
		catch (Exception ex)
		{
			s_failed = true;
			Plugin.Log.LogWarning("Epic Loot's item background failed, switched off for this session: " + ex.Message);
		}
	}

	internal static void Icon(Image icon, ItemDrop.ItemData item)
	{
		if (s_icon == null || s_failed || icon == null)
		{
			return;
		}
		try
		{
			s_icon(icon.gameObject, item);
		}
		catch (Exception ex)
		{
			s_failed = true;
			Plugin.Log.LogWarning("Epic Loot's item background failed, switched off for this session: " + ex.Message);
		}
	}

	private static Image Create(Image equipped)
	{
		GameObject gameObject = new GameObject("CDEV_MagicItem", typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(equipped.transform.parent, worldPositionStays: false);
		Vector2 vector = (rectTransform.pivot = new Vector2(0.5f, 0.5f));
		Vector2 anchorMin = (rectTransform.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		rectTransform.sizeDelta = new Vector2(64f, 64f);
		rectTransform.anchoredPosition = Vector2.zero;
		Image image = gameObject.AddComponent<Image>();
		image.type = equipped.type;
		image.pixelsPerUnitMultiplier = equipped.pixelsPerUnitMultiplier;
		image.raycastTarget = false;
		return image;
	}
}
