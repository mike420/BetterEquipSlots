using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BetterEquipSlots.EquipSlots;

internal static class InfoView
{
	private sealed class Row
	{
		internal GameObject Go;

		internal TMP_Text Label;

		internal TMP_Text Value;
	}

	private struct Entry
	{
		internal bool Header;

		internal string Label;

		internal string Value;

		internal Color Color;
	}

	private const float RowHeight = 22f;

	private const float HeaderHeight = 28f;

	private const float TextSize = 16f;

	private const float HeaderSize = 18f;

	private const float LabelSizeMin = 12f;

	private const float Pad = 8f;

	private const float BarWidth = 6f;

	private const float Interval = 0.25f;

	private static readonly Color LabelColor = new Color(0.86f, 0.82f, 0.72f, 1f);

	private static readonly Color ValueColor = new Color(1f, 0.66f, 0.2f, 1f);

	private static readonly Color Good = new Color(0.55f, 0.9f, 0.45f, 1f);

	private static readonly Color Bad = new Color(1f, 0.45f, 0.4f, 1f);

	private static readonly Color Immune = new Color(0.45f, 0.85f, 1f, 1f);

	private static readonly Color Plain = new Color(0.7f, 0.7f, 0.7f, 1f);

	private static RectTransform s_window;

	private static RectTransform s_viewport;

	private static RectTransform s_content;

	private static ScrollRect s_scroll;

	private static TMP_Text s_template;

	private static Color s_headerColor = new Color(1f, 0.8f, 0.4f, 1f);

	private static readonly List<Row> Rows = new List<Row>();

	private static readonly List<Entry> Entries = new List<Entry>();

	private static float s_next;

	internal static void Build(RectTransform root, TMP_Text template, Image style, TMP_Text headerTemplate)
	{
		s_template = template;
		if (headerTemplate != null)
		{
			s_headerColor = headerTemplate.color;
		}
		Rows.Clear();
		GameObject gameObject = new GameObject("CDEV_Info", typeof(RectTransform));
		s_window = (RectTransform)gameObject.transform;
		s_window.SetParent(root, worldPositionStays: false);
		RectTransform rectTransform = s_window;
		RectTransform rectTransform2 = s_window;
		Vector2 vector = (s_window.pivot = new Vector2(0f, 1f));
		Vector2 anchorMin = (rectTransform2.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		Image image = gameObject.AddComponent<Image>();
		image.raycastTarget = true;
		if (style != null)
		{
			image.sprite = style.sprite;
			image.type = style.type;
			image.material = style.material;
			image.pixelsPerUnitMultiplier = style.pixelsPerUnitMultiplier;
			image.color = style.color;
		}
		else
		{
			image.color = new Color(0f, 0f, 0f, 0.35f);
		}
		GameObject gameObject2 = new GameObject("Viewport", typeof(RectTransform));
		s_viewport = (RectTransform)gameObject2.transform;
		s_viewport.SetParent(s_window, worldPositionStays: false);
		s_viewport.anchorMin = Vector2.zero;
		s_viewport.anchorMax = Vector2.one;
		s_viewport.offsetMin = new Vector2(8f, 4f);
		s_viewport.offsetMax = new Vector2(-16f, -4f);
		gameObject2.AddComponent<RectMask2D>();
		GameObject gameObject3 = new GameObject("Content", typeof(RectTransform));
		s_content = (RectTransform)gameObject3.transform;
		s_content.SetParent(s_viewport, worldPositionStays: false);
		s_content.anchorMin = new Vector2(0f, 1f);
		s_content.anchorMax = new Vector2(1f, 1f);
		s_content.pivot = new Vector2(0.5f, 1f);
		s_content.sizeDelta = Vector2.zero;
		s_scroll = gameObject.AddComponent<ScrollRect>();
		s_scroll.viewport = s_viewport;
		s_scroll.content = s_content;
		s_scroll.horizontal = false;
		s_scroll.vertical = true;
		s_scroll.movementType = ScrollRect.MovementType.Clamped;
		s_scroll.scrollSensitivity = 484f;
		s_scroll.inertia = false;
		s_scroll.verticalScrollbar = Bar(gameObject.transform);
		s_scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
		gameObject.SetActive(value: false);
	}

	private static Scrollbar Bar(Transform parent)
	{
		GameObject gameObject = new GameObject("Scrollbar", typeof(RectTransform));
		RectTransform rectTransform = (RectTransform)gameObject.transform;
		rectTransform.SetParent(parent, worldPositionStays: false);
		rectTransform.anchorMin = new Vector2(1f, 0f);
		rectTransform.anchorMax = new Vector2(1f, 1f);
		rectTransform.pivot = new Vector2(1f, 0.5f);
		rectTransform.sizeDelta = new Vector2(6f, -8f);
		rectTransform.anchoredPosition = new Vector2(-4f, 0f);
		Image image = gameObject.AddComponent<Image>();
		image.color = new Color(0f, 0f, 0f, 0.35f);
		GameObject gameObject2 = new GameObject("Handle", typeof(RectTransform));
		RectTransform rectTransform2 = (RectTransform)gameObject2.transform;
		rectTransform2.SetParent(rectTransform, worldPositionStays: false);
		rectTransform2.anchorMin = Vector2.zero;
		rectTransform2.anchorMax = Vector2.one;
		Vector2 offsetMin = (rectTransform2.offsetMax = Vector2.zero);
		rectTransform2.offsetMin = offsetMin;
		Image image2 = gameObject2.AddComponent<Image>();
		image2.color = new Color(0.85f, 0.6f, 0.2f, 0.85f);
		Scrollbar scrollbar = gameObject.AddComponent<Scrollbar>();
		scrollbar.handleRect = rectTransform2;
		scrollbar.targetGraphic = image2;
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		return scrollbar;
	}

	internal static void Place(Vector2 topLeft, Vector2 size)
	{
		if (!(s_window == null))
		{
			s_window.anchoredPosition = new Vector2(topLeft.x, 0f - topLeft.y);
			s_window.sizeDelta = size;
		}
	}

	internal static void Show(bool show)
	{
		if (!(s_window == null) && s_window.gameObject.activeSelf != show)
		{
			s_window.gameObject.SetActive(show);
			if (show)
			{
				s_next = 0f;
				s_scroll.verticalNormalizedPosition = 1f;
			}
		}
	}

	internal static void Destroy()
	{
		if (s_window != null)
		{
			UnityEngine.Object.Destroy(s_window.gameObject);
		}
		s_window = null;
		Rows.Clear();
	}

	internal static void Scroll(float pages)
	{
		if (!(s_window == null) && s_window.gameObject.activeInHierarchy)
		{
			float num = s_content.rect.height - s_viewport.rect.height;
			if (num > 1f)
			{
				s_scroll.verticalNormalizedPosition = Mathf.Clamp01(s_scroll.verticalNormalizedPosition + pages * s_viewport.rect.height * 0.5f / num);
			}
		}
	}

	internal static void Tick(Player player)
	{
		if (!(s_window == null) && s_window.gameObject.activeInHierarchy && !(player == null) && !(Time.unscaledTime < s_next))
		{
			s_next = Time.unscaledTime + 0.25f;
			Entries.Clear();
			Collect(player);
			Draw();
		}
	}

	private static void Collect(Player player)
	{
		Header("$cdev_es_info_vitals");
		Line("$se_health", $"{Mathf.CeilToInt(player.GetHealth())} / {Mathf.CeilToInt(player.GetMaxHealth())}");
		Line("$se_stamina", $"{Mathf.CeilToInt(player.GetStamina())} / {Mathf.CeilToInt(player.GetMaxStamina())}");
		float maxEitr = player.GetMaxEitr();
		if (maxEitr > 0f)
		{
			Line("$se_eitr", $"{Mathf.CeilToInt(player.GetEitr())} / {Mathf.CeilToInt(maxEitr)}");
		}
		float num = 0f;
		foreach (Player.Food food in player.GetFoods())
		{
			num += food.m_item.m_shared.m_foodRegen;
		}
		float regenMultiplier = 1f;
		player.GetSEMan().ModifyHealthRegen(ref regenMultiplier);
		Line("$se_healthregen", "+" + Number(num * regenMultiplier) + "/10s");
		float staminaMultiplier = 1f;
		player.GetSEMan().ModifyStaminaRegen(ref staminaMultiplier);
		float low = player.m_staminaRegen * staminaMultiplier * Game.m_staminaRegenRate;
		float high = player.m_staminaRegen * (1f + player.m_staminaRegenTimeMultiplier) * staminaMultiplier * Game.m_staminaRegenRate;
		Line("$se_staminaregen", Range(low, high) + "/s");
		if (maxEitr > 0f)
		{
			float eitrMultiplier = 1f;
			player.GetSEMan().ModifyEitrRegen(ref eitrMultiplier);
			eitrMultiplier += player.GetEquipmentEitrRegenModifier();
			Line("$se_eitrregen", Range(player.m_eiterRegen * eitrMultiplier, 2f * player.m_eiterRegen * eitrMultiplier) + "/s");
		}
		Line("$item_weight", $"{Mathf.RoundToInt(player.GetInventory().GetTotalWeight())} / {Mathf.RoundToInt(player.GetMaxCarryWeight())}");
		Line("$cdev_es_info_comfort", player.GetComfortLevel().ToString());
		ItemDrop.ItemData currentWeapon = player.GetCurrentWeapon();
		if (currentWeapon != null)
		{
			Header("$cdev_es_info_attack");
			Skills.SkillType skillType = currentWeapon.m_shared.m_skillType;
			if (currentWeapon.m_shared.m_name.StartsWith("$", StringComparison.Ordinal))
			{
				Line(currentWeapon.m_shared.m_name, (currentWeapon.m_shared.m_maxQuality > 1) ? ("★" + currentWeapon.m_quality) : "");
			}
			string text = Localization.instance.Localize("$skill_" + skillType.ToString().ToLowerInvariant());
			if (text.StartsWith("[", StringComparison.Ordinal))
			{
				text = skillType.ToString();
			}
			Line(text, Mathf.FloorToInt(player.GetSkillLevel(skillType)).ToString(), null, raw: true);
			string[] array = Localization.instance.Localize(currentWeapon.GetDamage().GetTooltipString(skillType)).Split(new char[1] { '\n' });
			foreach (string text2 in array)
			{
				int num2 = text2.IndexOf(": ", StringComparison.Ordinal);
				if (num2 > 0)
				{
					Line(text2.Substring(0, num2), text2.Substring(num2 + 2).Trim(), null, raw: true);
				}
			}
			if (currentWeapon.m_shared.m_attackForce > 0f)
			{
				Line("$item_knockback", Number(currentWeapon.m_shared.m_attackForce));
			}
			if (currentWeapon.m_shared.m_backstabBonus > 1f)
			{
				Line("$item_backstab", Number(currentWeapon.m_shared.m_backstabBonus) + "x");
			}
		}
		Header("$cdev_es_info_defense");
		Line("$item_armor", Number(player.GetBodyArmor()));
		foreach (ItemDrop.ItemData worn in player.GetInventory().m_inventory)
		{
			if (worn.m_equipped && worn.m_shared != null && worn.GetArmor() > 0f && (worn.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Helmet || worn.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Chest || worn.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Legs || worn.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shoulder))
			{
				Line("  " + Localization.instance.Localize(worn.m_shared.m_name), Number(worn.GetArmor()), Plain, raw: true);
			}
		}
		ItemDrop.ItemData currentBlocker = ((Humanoid)player).GetCurrentBlocker();
		if (currentBlocker != null)
		{
			Line("$item_blockarmor", Number(currentBlocker.GetBlockPower(player.GetSkillFactor(Skills.SkillType.Blocking))));
			Line("$item_blockforce", Number(currentBlocker.GetDeflectionForce()));
			if (currentBlocker.m_shared.m_timedBlockBonus > 1f)
			{
				Line("$item_parrybonus", Number(currentBlocker.m_shared.m_timedBlockBonus) + "x");
			}
		}
		int count = Entries.Count;
		string[] s_equipmentModifierTooltips = Player.s_equipmentModifierTooltips;
		int num3 = 0;
		while (s_equipmentModifierTooltips != null && num3 < s_equipmentModifierTooltips.Length)
		{
			float equipmentModifierPlusSE = player.GetEquipmentModifierPlusSE(num3);
			if (!(Mathf.Abs(equipmentModifierPlusSE) < 0.0001f))
			{
				string text3 = ((num3 < 10) ? ((equipmentModifierPlusSE * 100f).ToString("+0;-0") + "%") : Number(equipmentModifierPlusSE));
				string label = s_equipmentModifierTooltips[num3];
				string value = text3;
				Color value2;
				if (num3 >= 10)
				{
					value2 = ValueColor;
				}
				else
				{
					value2 = (IsGoodModifier(s_equipmentModifierTooltips[num3], equipmentModifierPlusSE) ? Good : Bad);
				}
				Line(label, value, value2);
			}
			num3++;
		}
		if (Entries.Count > count)
		{
			Entries.Insert(count, new Entry
			{
				Header = true,
				Label = Localization.instance.Localize("$cdev_es_info_gear")
			});
		}
		CollectSets(player);
		Header("$cdev_es_info_resist");
		HitData.DamageModifiers damageModifiers = player.GetDamageModifiers();
		Resist("$inventory_blunt", damageModifiers.m_blunt);
		Resist("$inventory_slash", damageModifiers.m_slash);
		Resist("$inventory_pierce", damageModifiers.m_pierce);
		Resist("$inventory_fire", damageModifiers.m_fire);
		Resist("$inventory_frost", damageModifiers.m_frost);
		Resist("$inventory_lightning", damageModifiers.m_lightning);
		Resist("$inventory_poison", damageModifiers.m_poison);
		Resist("$inventory_spirit", damageModifiers.m_spirit);
		List<Player.Food> foods = player.GetFoods();
		if (foods.Count > 0)
		{
			Header("$cdev_es_info_food");
			foreach (Player.Food item in foods)
			{
				string text4 = $"<color=#ff8080>{Mathf.CeilToInt(item.m_health)}</color>  <color=#ffff80>{Mathf.CeilToInt(item.m_stamina)}</color>";
				if (item.m_eitr > 0f)
				{
					text4 += $"  <color=#9999ff>{Mathf.CeilToInt(item.m_eitr)}</color>";
				}
				Line(item.m_item.m_shared.m_name, text4 + "   " + Clock(item.m_time));
			}
		}
		int count2 = Entries.Count;
		foreach (StatusEffect statusEffect2 in player.GetSEMan().GetStatusEffects())
		{
			if (!(statusEffect2 == null) && !string.IsNullOrEmpty(statusEffect2.m_name))
			{
				float num4 = ((statusEffect2.m_ttl > 0f) ? (statusEffect2.m_ttl - statusEffect2.m_time) : (-1f));
				Line(statusEffect2.m_name, (num4 > 0f) ? Clock(num4) : "");
			}
		}
		StatusEffect statusEffect = ((!string.IsNullOrEmpty(player.m_guardianPower) && ObjectDB.instance != null) ? ObjectDB.instance.GetStatusEffect(player.m_guardianPowerHash) : null);
		if (statusEffect != null)
		{
			Line(statusEffect.m_name, (player.m_guardianPowerCooldown > 0f) ? Clock(player.m_guardianPowerCooldown) : Localization.instance.Localize("$hud_ready"), (player.m_guardianPowerCooldown > 0f) ? ValueColor : Good);
		}
		if (Entries.Count > count2)
		{
			Entries.Insert(count2, new Entry
			{
				Header = true,
				Label = Localization.instance.Localize("$cdev_es_info_effects")
			});
		}
	}

	// Armour sets: how many pieces of each are worn, of how many it takes for the set bonus
	private static void CollectSets(Player player)
	{
		Dictionary<string, int> counts = new Dictionary<string, int>();
		Dictionary<string, int> sizes = new Dictionary<string, int>();
		foreach (ItemDrop.ItemData item in player.GetInventory().m_inventory)
		{
			if (item.m_equipped && item.m_shared != null && !string.IsNullOrEmpty(item.m_shared.m_setName) && item.m_shared.m_setSize > 1)
			{
				string name = item.m_shared.m_setName;
				counts[name] = (counts.TryGetValue(name, out int have) ? have : 0) + 1;
				sizes[name] = item.m_shared.m_setSize;
			}
		}
		if (counts.Count == 0)
		{
			return;
		}
		Header(Texts.InfoSets);
		foreach (KeyValuePair<string, int> pair in counts)
		{
			bool complete = pair.Value >= sizes[pair.Key];
			Line(Localization.instance.Localize(pair.Key), pair.Value + " / " + sizes[pair.Key], complete ? Good : ValueColor, raw: true);
		}
	}

	private static string Clock(float seconds)
	{
		int num = Mathf.Max(0, Mathf.CeilToInt(seconds));
		if (num < 3600)
		{
			return $"{num / 60}:{num % 60:00}";
		}
		return $"{num / 3600}:{num / 60 % 60:00}:{num % 60:00}";
	}

	private static bool IsGoodModifier(string key, float value)
	{
		if (!(key == "$item_movement_modifier") && !(key == "$item_heat_modifier"))
		{
			return value < 0f;
		}
		return value > 0f;
	}

	private static void Resist(string key, HitData.DamageModifier modifier)
	{
		switch (modifier)
		{
		case HitData.DamageModifier.Normal:
			Line(key, Localization.instance.Localize("$cdev_es_info_normal"), Plain);
			break;
		case HitData.DamageModifier.SlightlyResistant:
			Line(key, Localization.instance.Localize("$inventory_slightlyresistant"), Good);
			break;
		case HitData.DamageModifier.Resistant:
			Line(key, Localization.instance.Localize("$inventory_resistant"), Good);
			break;
		case HitData.DamageModifier.VeryResistant:
			Line(key, Localization.instance.Localize("$inventory_veryresistant"), Good);
			break;
		case HitData.DamageModifier.Immune:
		case HitData.DamageModifier.Ignore:
			Line(key, Localization.instance.Localize("$inventory_immune"), Immune);
			break;
		case HitData.DamageModifier.SlightlyWeak:
			Line(key, Localization.instance.Localize("$inventory_slightlyweak"), Bad);
			break;
		case HitData.DamageModifier.Weak:
			Line(key, Localization.instance.Localize("$inventory_weak"), Bad);
			break;
		case HitData.DamageModifier.VeryWeak:
			Line(key, Localization.instance.Localize("$inventory_veryweak"), Bad);
			break;
		}
	}

	private static string Number(float value)
	{
		return value.ToString((Mathf.Abs(value - Mathf.Round(value)) < 0.05f) ? "0" : "0.#");
	}

	private static string Range(float low, float high)
	{
		if (!(Mathf.Abs(high - low) < 0.05f))
		{
			return Number(low) + "–" + Number(high);
		}
		return Number(low);
	}

	private static void Header(string key)
	{
		Entries.Add(new Entry
		{
			Header = true,
			Label = Localization.instance.Localize(key)
		});
	}

	private static void Line(string label, string value, Color? color = null, bool raw = false)
	{
		Entries.Add(new Entry
		{
			Label = (raw ? label : Localization.instance.Localize(label)),
			Value = value,
			Color = (color ?? ValueColor)
		});
	}

	private static void Draw()
	{
		float num = 0f;
		float width = s_viewport.rect.width;
		for (int i = 0; i < Entries.Count; i++)
		{
			Entry entry = Entries[i];
			Row row = ((i < Rows.Count) ? Rows[i] : NewRow());
			if (!row.Go.activeSelf)
			{
				row.Go.SetActive(value: true);
			}
			if (entry.Header && i > 0)
			{
				num += 4f;
			}
			float num2 = (entry.Header ? 28f : 22f);
			RectTransform rectTransform = (RectTransform)row.Go.transform;
			rectTransform.anchoredPosition = new Vector2(0f, 0f - num);
			rectTransform.sizeDelta = new Vector2(width, num2);
			Set(row.Label, entry.Label);
			float num3 = (entry.Header ? 18f : 16f);
			if (row.Label.fontSizeMax != num3)
			{
				row.Label.fontSizeMax = num3;
			}
			row.Label.color = (entry.Header ? s_headerColor : LabelColor);
			row.Label.alignment = (entry.Header ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.Left);
			Set(row.Value, entry.Header ? "" : entry.Value);
			row.Value.color = entry.Color;
			float num4 = ((entry.Header || entry.Value.Length == 0) ? 0f : Mathf.Min(row.Value.GetPreferredValues(entry.Value).x + 2f, width * 0.6f));
			row.Value.rectTransform.offsetMin = new Vector2(width - num4, 0f);
			row.Label.rectTransform.offsetMax = new Vector2(0f - ((num4 > 0f) ? (num4 + 8f) : 0f), 0f);
			num += num2;
		}
		for (int j = Entries.Count; j < Rows.Count; j++)
		{
			if (Rows[j].Go.activeSelf)
			{
				Rows[j].Go.SetActive(value: false);
			}
		}
		if (Mathf.Abs(s_content.sizeDelta.y - num) > 0.5f)
		{
			s_content.sizeDelta = new Vector2(0f, num);
		}
	}

	private static void Set(TMP_Text text, string value)
	{
		if (text.text != value)
		{
			text.text = value;
		}
	}

	private static Row NewRow()
	{
		Row row = new Row();
		row.Go = new GameObject("Row", typeof(RectTransform));
		Row row2 = row;
		RectTransform rectTransform = (RectTransform)row2.Go.transform;
		rectTransform.SetParent(s_content, worldPositionStays: false);
		Vector2 vector = (rectTransform.pivot = new Vector2(0f, 1f));
		Vector2 anchorMin = (rectTransform.anchorMax = vector);
		rectTransform.anchorMin = anchorMin;
		row2.Label = Text(rectTransform, "Label", TextAlignmentOptions.Left);
		row2.Label.enableAutoSizing = true;
		row2.Label.fontSizeMin = 12f;
		row2.Label.fontSizeMax = 16f;
		row2.Value = Text(rectTransform, "Value", TextAlignmentOptions.Right);
		Rows.Add(row2);
		return row2;
	}

	private static TMP_Text Text(RectTransform parent, string name, TextAlignmentOptions alignment)
	{
		TMP_Text tMP_Text = UnityEngine.Object.Instantiate(s_template, parent, worldPositionStays: false);
		tMP_Text.name = name;
		tMP_Text.gameObject.SetActive(value: true);
		tMP_Text.raycastTarget = false;
		tMP_Text.enableAutoSizing = false;
		tMP_Text.fontSize = 16f;
		tMP_Text.richText = true;
		tMP_Text.alignment = alignment;
		tMP_Text.textWrappingMode = TextWrappingModes.NoWrap;
		tMP_Text.overflowMode = TextOverflowModes.Ellipsis;
		RectTransform rectTransform = tMP_Text.rectTransform;
		rectTransform.anchorMin = Vector2.zero;
		rectTransform.anchorMax = Vector2.one;
		rectTransform.pivot = new Vector2(0.5f, 0.5f);
		Vector2 offsetMin = (rectTransform.offsetMax = Vector2.zero);
		rectTransform.offsetMin = offsetMin;
		return tMP_Text;
	}
}
