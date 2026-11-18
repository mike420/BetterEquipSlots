using System.Collections.Generic;
using System.Text;
using Jotunn.Entities;
using Jotunn.Managers;

namespace BetterEquipSlots.EquipSlots;

/// <summary>
/// The texts of loadouts, the durability warning and the Info tab's sets, in the languages of <see cref="Translations"/>
/// (a language that lacks one shows English). $0, $1 in a text are filled in by <see cref="Get"/>.
/// </summary>
internal static class Texts
{
	internal const string Loadout = "$cdev_es_loadout";

	internal const string LoadoutSaved = "$cdev_es_loadout_saved";

	internal const string LoadoutApplied = "$cdev_es_loadout_applied";

	internal const string LoadoutMissing = "$cdev_es_loadout_missing";

	internal const string LoadoutFull = "$cdev_es_loadout_full";

	internal const string LoadoutEmpty = "$cdev_es_loadout_empty";

	internal const string LoadoutTipUse = "$cdev_es_loadout_tip_use";

	internal const string LoadoutTipEmpty = "$cdev_es_loadout_tip_empty";

	internal const string LoadoutTipSave = "$cdev_es_loadout_tip_save";

	internal const string LoadoutTipKey = "$cdev_es_loadout_tip_key";

	internal const string DurabilityLow = "$cdev_es_durability_low";

	internal const string InfoSets = "$cdev_es_info_sets";

	internal const string Style = "$cdev_es_style";

	internal const string StyleSaved = "$cdev_es_style_saved";

	internal const string StyleApplied = "$cdev_es_style_applied";

	internal const string StyleEmpty = "$cdev_es_style_empty";

	internal const string StyleTipUse = "$cdev_es_style_tip_use";

	internal const string StyleTipSave = "$cdev_es_style_tip_save";

	private static readonly string[] Keys = new string[18]
	{
		"cdev_es_loadout", "cdev_es_loadout_saved", "cdev_es_loadout_applied", "cdev_es_loadout_missing", "cdev_es_loadout_full", "cdev_es_loadout_empty",
		"cdev_es_loadout_tip_use", "cdev_es_loadout_tip_empty", "cdev_es_loadout_tip_save", "cdev_es_loadout_tip_key", "cdev_es_durability_low", "cdev_es_info_sets",
		"cdev_es_style", "cdev_es_style_saved", "cdev_es_style_applied", "cdev_es_style_empty", "cdev_es_style_tip_use", "cdev_es_style_tip_save"
	};

	private static readonly Dictionary<string, string[]> Languages = new Dictionary<string, string[]>
	{
		["English"] = new string[18]
		{
			"Loadout $0", "Loadout $0 saved ($1 items)", "Loadout $0 applied", ", $0 item(s) not found", ", $0 could not be moved (inventory full)", "Loadout $0 is empty: right click its button (or hold $1 with its key) to save the current gear",
			"Left click: put it on.", "Empty.", "Right click: save what you wear and the quick slots now.", "Key: $0, with $1 to save.", "$0 is nearly broken ($1%)", "Set bonuses",
			"Style $0", "Style $0 saved", "Style $0 applied", "Style $0 is empty: right click its button to save your current style", "Left click: use this style.", "Right click: save your current style here."
		},
		["Russian"] = new string[18]
		{
			"Комплект $0", "Комплект $0 сохранён (предметов: $1)", "Комплект $0 надет", ", не найдено предметов: $0", ", не удалось переместить: $0 (инвентарь полон)", "Комплект $0 пуст: щёлкните его кнопку правой кнопкой (или удерживайте $1 и нажмите его клавишу), чтобы сохранить текущую экипировку",
			"ЛКМ: надеть.", "Пусто.", "ПКМ: сохранить то, что надето, и быстрые слоты.", "Клавиша: $0, сохранение - с $1.", "$0 почти сломано ($1%)", "Бонусы комплекта",
			"Стиль $0", "Стиль $0 сохранён", "Стиль $0 применён", "Стиль $0 пуст: щёлкните его кнопку правой кнопкой, чтобы сохранить текущий стиль", "ЛКМ: применить этот стиль.", "ПКМ: сохранить сюда текущий стиль."
		},
		["Ukrainian"] = new string[18]
		{
			"Набір $0", "Набір $0 збережено (предметів: $1)", "Набір $0 одягнено", ", не знайдено предметів: $0", ", не вдалося перемістити: $0 (інвентар повний)", "Набір $0 порожній: клацніть його кнопку правою кнопкою (або утримуйте $1 і натисніть його клавішу), щоб зберегти поточне спорядження",
			"ЛКМ: одягнути.", "Порожньо.", "ПКМ: зберегти те, що одягнено, і швидкі слоти.", "Клавіша: $0, збереження - з $1.", "$0 майже зламано ($1%)", "Бонуси набору",
			"Стиль $0", "Стиль $0 збережено", "Стиль $0 застосовано", "Стиль $0 порожній: клацніть його кнопку правою кнопкою, щоб зберегти поточний стиль", "ЛКМ: застосувати цей стиль.", "ПКМ: зберегти сюди поточний стиль."
		},
		["German"] = new string[18]
		{
			"Set $0", "Set $0 gespeichert ($1 Gegenstände)", "Set $0 angelegt", ", $0 Gegenstand/Gegenstände nicht gefunden", ", $0 konnte(n) nicht verschoben werden (Inventar voll)", "Set $0 ist leer: Rechtsklick auf die Schaltfläche (oder $1 gedrückt halten und die Taste drücken), um die aktuelle Ausrüstung zu speichern",
			"Linksklick: anlegen.", "Leer.", "Rechtsklick: speichert, was du trägst, und die Schnellplätze.", "Taste: $0, zum Speichern mit $1.", "$0 ist fast kaputt ($1%)", "Set-Boni",
			"Stil $0", "Stil $0 gespeichert", "Stil $0 angewendet", "Stil $0 ist leer: Rechtsklick auf die Schaltfläche, um deinen aktuellen Stil zu speichern", "Linksklick: diesen Stil verwenden.", "Rechtsklick: aktuellen Stil hier speichern."
		},
		["French"] = new string[18]
		{
			"Équipement $0", "Équipement $0 enregistré ($1 objets)", "Équipement $0 enfilé", ", $0 objet(s) introuvable(s)", ", $0 n'ont pas pu être déplacés (inventaire plein)", "L'équipement $0 est vide : clic droit sur son bouton (ou maintenez $1 avec sa touche) pour enregistrer la tenue actuelle",
			"Clic gauche : l'enfiler.", "Vide.", "Clic droit : enregistrer ce que vous portez et les emplacements rapides.", "Touche : $0, avec $1 pour enregistrer.", "$0 est presque cassé ($1 %)", "Bonus d'ensemble",
			"Style $0", "Style $0 enregistré", "Style $0 appliqué", "Le style $0 est vide : clic droit sur son bouton pour enregistrer votre style actuel", "Clic gauche : utiliser ce style.", "Clic droit : enregistrer ici votre style actuel."
		},
		["Spanish"] = new string[18]
		{
			"Conjunto $0", "Conjunto $0 guardado ($1 objetos)", "Conjunto $0 puesto", ", $0 objeto(s) no encontrado(s)", ", $0 no se pudieron mover (inventario lleno)", "El conjunto $0 está vacío: clic derecho en su botón (o mantén $1 con su tecla) para guardar el equipo actual",
			"Clic izquierdo: ponérselo.", "Vacío.", "Clic derecho: guardar lo que llevas puesto y las ranuras rápidas.", "Tecla: $0, con $1 para guardar.", "$0 está casi roto ($1 %)", "Bonificaciones de conjunto",
			"Estilo $0", "Estilo $0 guardado", "Estilo $0 aplicado", "El estilo $0 está vacío: clic derecho en su botón para guardar tu estilo actual", "Clic izquierdo: usar este estilo.", "Clic derecho: guardar aquí tu estilo actual."
		},
		["Italian"] = new string[18]
		{
			"Set $0", "Set $0 salvato ($1 oggetti)", "Set $0 indossato", ", $0 oggetto/i non trovato/i", ", $0 non è stato possibile spostare (inventario pieno)", "Il set $0 è vuoto: clic destro sul suo pulsante (o tieni premuto $1 con il suo tasto) per salvare l'equipaggiamento attuale",
			"Clic sinistro: indossalo.", "Vuoto.", "Clic destro: salva ciò che indossi e gli slot rapidi.", "Tasto: $0, con $1 per salvare.", "$0 è quasi rotto ($1%)", "Bonus del set",
			"Stile $0", "Stile $0 salvato", "Stile $0 applicato", "Lo stile $0 è vuoto: clic destro sul suo pulsante per salvare il tuo stile attuale", "Clic sinistro: usa questo stile.", "Clic destro: salva qui il tuo stile attuale."
		},
		["Portuguese_Brazilian"] = new string[18]
		{
			"Conjunto $0", "Conjunto $0 salvo ($1 itens)", "Conjunto $0 equipado", ", $0 item(ns) não encontrado(s)", ", $0 não puderam ser movidos (inventário cheio)", "O conjunto $0 está vazio: clique com o botão direito no botão dele (ou segure $1 com a tecla dele) para salvar o equipamento atual",
			"Clique esquerdo: equipar.", "Vazio.", "Clique direito: salvar o que você veste e os espaços rápidos.", "Tecla: $0, com $1 para salvar.", "$0 está quase quebrado ($1%)", "Bônus de conjunto",
			"Estilo $0", "Estilo $0 salvo", "Estilo $0 aplicado", "O estilo $0 está vazio: clique com o botão direito no botão dele para salvar seu estilo atual", "Clique esquerdo: usar este estilo.", "Clique direito: salvar seu estilo atual aqui."
		},
		["Portuguese_European"] = new string[18]
		{
			"Conjunto $0", "Conjunto $0 guardado ($1 itens)", "Conjunto $0 equipado", ", $0 item(ns) não encontrado(s)", ", $0 não puderam ser movidos (inventário cheio)", "O conjunto $0 está vazio: clique com o botão direito no respetivo botão (ou mantenha $1 com a respetiva tecla) para guardar o equipamento atual",
			"Clique esquerdo: equipar.", "Vazio.", "Clique direito: guardar o que veste e os espaços rápidos.", "Tecla: $0, com $1 para guardar.", "$0 está quase partido ($1%)", "Bónus de conjunto",
			"Estilo $0", "Estilo $0 guardado", "Estilo $0 aplicado", "O estilo $0 está vazio: clique com o botão direito no respetivo botão para guardar o seu estilo atual", "Clique esquerdo: usar este estilo.", "Clique direito: guardar aqui o seu estilo atual."
		},
		["Polish"] = new string[18]
		{
			"Zestaw $0", "Zestaw $0 zapisany (przedmiotów: $1)", "Zestaw $0 założony", ", nie znaleziono przedmiotów: $0", ", nie udało się przenieść: $0 (ekwipunek pełny)", "Zestaw $0 jest pusty: kliknij prawym przyciskiem jego przycisk (lub przytrzymaj $1 i naciśnij jego klawisz), aby zapisać obecny ekwipunek",
			"LPM: załóż.", "Pusty.", "PPM: zapisz to, co masz na sobie, i szybkie sloty.", "Klawisz: $0, zapis z $1.", "$0 jest prawie zepsute ($1%)", "Bonusy zestawu",
			"Styl $0", "Styl $0 zapisany", "Styl $0 zastosowany", "Styl $0 jest pusty: kliknij prawym przyciskiem jego przycisk, aby zapisać obecny styl", "LPM: użyj tego stylu.", "PPM: zapisz tutaj obecny styl."
		},
		["Czech"] = new string[18]
		{
			"Sada $0", "Sada $0 uložena (předmětů: $1)", "Sada $0 nasazena", ", nenalezeno předmětů: $0", ", nepodařilo se přesunout: $0 (inventář je plný)", "Sada $0 je prázdná: klikněte pravým tlačítkem na její tlačítko (nebo podržte $1 a stiskněte její klávesu) a uložte aktuální vybavení",
			"Levé tlačítko: nasadit.", "Prázdné.", "Pravé tlačítko: uložit, co máte na sobě, a rychlé sloty.", "Klávesa: $0, uložení s $1.", "$0 je skoro rozbité ($1 %)", "Bonusy sady",
			"Styl $0", "Styl $0 uložen", "Styl $0 použit", "Styl $0 je prázdný: klikněte pravým tlačítkem na jeho tlačítko a uložte aktuální styl", "Levé tlačítko: použít tento styl.", "Pravé tlačítko: uložit sem aktuální styl."
		},
		["Hungarian"] = new string[18]
		{
			"Készlet $0", "A(z) $0. készlet mentve ($1 tárgy)", "A(z) $0. készlet felvéve", ", $0 tárgy nem található", ", $0 nem helyezhető át (tele a tárgylista)", "A(z) $0. készlet üres: jobb kattintás a gombjára (vagy tartsd lenyomva a(z) $1 billentyűt a gombjával) a jelenlegi felszerelés mentéséhez",
			"Bal kattintás: felvétel.", "Üres.", "Jobb kattintás: elmenti, amit viselsz, és a gyorshelyeket.", "Billentyű: $0, mentéshez $1 mellett.", "$0 majdnem tönkrement ($1%)", "Szettbónuszok",
			"Stílus $0", "A(z) $0. stílus mentve", "A(z) $0. stílus alkalmazva", "A(z) $0. stílus üres: jobb kattintás a gombjára a jelenlegi stílusod mentéséhez", "Bal kattintás: a stílus használata.", "Jobb kattintás: a jelenlegi stílus mentése ide."
		},
		["Dutch"] = new string[18]
		{
			"Set $0", "Set $0 opgeslagen ($1 voorwerpen)", "Set $0 aangedaan", ", $0 voorwerp(en) niet gevonden", ", $0 kon(den) niet worden verplaatst (inventaris vol)", "Set $0 is leeg: klik met rechts op de knop (of houd $1 ingedrukt met de toets) om de huidige uitrusting op te slaan",
			"Linksklik: aandoen.", "Leeg.", "Rechtsklik: sla op wat je draagt en de snelle vakken.", "Toets: $0, met $1 om op te slaan.", "$0 is bijna kapot ($1%)", "Setbonussen",
			"Stijl $0", "Stijl $0 opgeslagen", "Stijl $0 toegepast", "Stijl $0 is leeg: klik met rechts op de knop om je huidige stijl op te slaan", "Linksklik: deze stijl gebruiken.", "Rechtsklik: sla je huidige stijl hier op."
		},
		["Swedish"] = new string[18]
		{
			"Set $0", "Set $0 sparat ($1 föremål)", "Set $0 påtaget", ", $0 föremål hittades inte", ", $0 kunde inte flyttas (inventariet fullt)", "Set $0 är tomt: högerklicka på dess knapp (eller håll $1 med dess tangent) för att spara den nuvarande utrustningen",
			"Vänsterklick: ta på.", "Tomt.", "Högerklick: spara vad du bär och snabbplatserna.", "Tangent: $0, med $1 för att spara.", "$0 är nästan trasigt ($1 %)", "Setbonusar",
			"Stil $0", "Stil $0 sparad", "Stil $0 tillämpad", "Stil $0 är tom: högerklicka på dess knapp för att spara din nuvarande stil", "Vänsterklick: använd den här stilen.", "Högerklick: spara din nuvarande stil här."
		},
		["Norwegian"] = new string[18]
		{
			"Sett $0", "Sett $0 lagret ($1 gjenstander)", "Sett $0 tatt på", ", $0 gjenstand(er) ble ikke funnet", ", $0 kunne ikke flyttes (inventaret er fullt)", "Sett $0 er tomt: høyreklikk på knappen (eller hold $1 med tasten) for å lagre utstyret du har nå",
			"Venstreklikk: ta på.", "Tomt.", "Høyreklikk: lagre det du bærer og hurtigplassene.", "Tast: $0, med $1 for å lagre.", "$0 er nesten ødelagt ($1 %)", "Settbonuser",
			"Stil $0", "Stil $0 lagret", "Stil $0 tatt i bruk", "Stil $0 er tom: høyreklikk på knappen for å lagre den gjeldende stilen din", "Venstreklikk: bruk denne stilen.", "Høyreklikk: lagre den gjeldende stilen din her."
		},
		["Danish"] = new string[18]
		{
			"Sæt $0", "Sæt $0 gemt ($1 genstande)", "Sæt $0 taget på", ", $0 genstand(e) blev ikke fundet", ", $0 kunne ikke flyttes (inventaret er fuldt)", "Sæt $0 er tomt: højreklik på knappen (eller hold $1 med tasten) for at gemme det nuværende udstyr",
			"Venstreklik: tag på.", "Tomt.", "Højreklik: gem det, du bærer, og hurtigpladserne.", "Tast: $0, med $1 for at gemme.", "$0 er næsten ødelagt ($1 %)", "Sætbonusser",
			"Stil $0", "Stil $0 gemt", "Stil $0 anvendt", "Stil $0 er tom: højreklik på knappen for at gemme din nuværende stil", "Venstreklik: brug denne stil.", "Højreklik: gem din nuværende stil her."
		},
		["Finnish"] = new string[18]
		{
			"Setti $0", "Setti $0 tallennettu ($1 esinettä)", "Setti $0 puettu", ", $0 esinettä ei löytynyt", ", $0 ei voitu siirtää (reppu täynnä)", "Setti $0 on tyhjä: napsauta sen painiketta hiiren oikealla (tai pidä $1 pohjassa sen näppäimen kanssa) tallentaaksesi nykyiset varusteet",
			"Vasen napsautus: pue.", "Tyhjä.", "Oikea napsautus: tallenna pukemasi ja pikapaikat.", "Näppäin: $0, tallennus $1 pohjassa.", "$0 on melkein rikki ($1 %)", "Settibonukset",
			"Tyyli $0", "Tyyli $0 tallennettu", "Tyyli $0 otettu käyttöön", "Tyyli $0 on tyhjä: napsauta sen painiketta hiiren oikealla tallentaaksesi nykyisen tyylisi", "Vasen napsautus: käytä tätä tyyliä.", "Oikea napsautus: tallenna nykyinen tyylisi tähän."
		},
		["Turkish"] = new string[18]
		{
			"Set $0", "Set $0 kaydedildi ($1 eşya)", "Set $0 giyildi", ", $0 eşya bulunamadı", ", $0 taşınamadı (envanter dolu)", "Set $0 boş: geçerli ekipmanı kaydetmek için düğmesine sağ tıklayın (veya tuşuyla birlikte $1 tuşunu basılı tutun)",
			"Sol tık: giy.", "Boş.", "Sağ tık: giydiklerinizi ve hızlı yuvaları kaydet.", "Tuş: $0, kaydetmek için $1 ile.", "$0 neredeyse kırık (%$1)", "Set bonusları",
			"Stil $0", "Stil $0 kaydedildi", "Stil $0 uygulandı", "Stil $0 boş: mevcut stilinizi kaydetmek için düğmesine sağ tıklayın", "Sol tık: bu stili kullan.", "Sağ tık: mevcut stilinizi buraya kaydet."
		},
		["Chinese"] = new string[18]
		{
			"装备组 $0", "装备组 $0 已保存（$1 件物品）", "已穿上装备组 $0", "，有 $0 件物品未找到", "，有 $0 件无法移动（背包已满）", "装备组 $0 是空的：右键点击它的按钮（或按住 $1 再按它的按键）以保存当前装备",
			"左键：穿上。", "空。", "右键：保存当前穿戴的装备和快捷栏。", "按键：$0，按住 $1 为保存。", "$0 快要损坏了（$1%）", "套装加成",
			"风格 $0", "风格 $0 已保存", "已应用风格 $0", "风格 $0 是空的：右键点击它的按钮以保存当前风格", "左键：使用此风格。", "右键：将当前风格保存到这里。"
		},
		["Chinese_Trad"] = new string[18]
		{
			"裝備組 $0", "裝備組 $0 已儲存（$1 件物品）", "已穿上裝備組 $0", "，有 $0 件物品未找到", "，有 $0 件無法移動（背包已滿）", "裝備組 $0 是空的：右鍵點擊它的按鈕（或按住 $1 再按它的按鍵）以儲存目前裝備",
			"左鍵：穿上。", "空。", "右鍵：儲存目前穿戴的裝備和快捷欄。", "按鍵：$0，按住 $1 為儲存。", "$0 快要損壞了（$1%）", "套裝加成",
			"風格 $0", "風格 $0 已儲存", "已套用風格 $0", "風格 $0 是空的：右鍵點擊它的按鈕以儲存目前風格", "左鍵：使用此風格。", "右鍵：將目前風格儲存到這裡。"
		},
		["Japanese"] = new string[18]
		{
			"セット $0", "セット $0 を保存しました（$1 個）", "セット $0 を装備しました", "、$0 個のアイテムが見つかりません", "、$0 個を移動できませんでした（インベントリがいっぱい）", "セット $0 は空です：ボタンを右クリック（または $1 を押しながらそのキー）で現在の装備を保存します",
			"左クリック：装備する。", "空。", "右クリック：現在の装備とクイックスロットを保存。", "キー：$0、保存は $1 を押しながら。", "$0 はもうすぐ壊れます（$1%）", "セット効果",
			"スタイル $0", "スタイル $0 を保存しました", "スタイル $0 を適用しました", "スタイル $0 は空です：ボタンを右クリックして現在のスタイルを保存します", "左クリック：このスタイルを使う。", "右クリック：現在のスタイルをここに保存。"
		},
		["Korean"] = new string[18]
		{
			"세트 $0", "세트 $0 저장됨 ($1개)", "세트 $0 착용함", ", $0개 아이템을 찾지 못함", ", $0개를 옮기지 못함 (인벤토리 가득 참)", "세트 $0이(가) 비어 있습니다: 버튼을 우클릭(또는 $1을(를) 누른 채 해당 키)하여 현재 장비를 저장하세요",
			"좌클릭: 착용.", "비어 있음.", "우클릭: 착용 중인 장비와 빠른 슬롯을 저장.", "키: $0, 저장은 $1과(와) 함께.", "$0이(가) 거의 부서졌습니다 ($1%)", "세트 효과",
			"스타일 $0", "스타일 $0 저장됨", "스타일 $0 적용됨", "스타일 $0이(가) 비어 있습니다: 버튼을 우클릭하여 현재 스타일을 저장하세요", "좌클릭: 이 스타일 사용.", "우클릭: 현재 스타일을 여기에 저장."
		}
	};

	internal static void Register(CustomLocalization localization)
	{
		foreach (KeyValuePair<string, string[]> language in Languages)
		{
			if (language.Value.Length != Keys.Length)
			{
				Plugin.Log.LogError($"Texts '{language.Key}' has {language.Value.Length} texts, expected {Keys.Length}; skipped.");
				continue;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int i = 0; i < Keys.Length; i++)
			{
				dictionary[Keys[i]] = language.Value[i];
			}
			localization.AddTranslation(language.Key, dictionary);
		}
	}

	internal static void AddToLoadedLanguage(Localization instance)
	{
		if (!Languages.TryGetValue(instance.GetSelectedLanguage(), out var value) || value.Length != Keys.Length)
		{
			value = Languages["English"];
		}
		for (int i = 0; i < Keys.Length; i++)
		{
			instance.AddWord(Keys[i], value[i]);
		}
	}

	/// <summary>The text of a key in the language of the game, with $0, $1 ... replaced by the words.</summary>
	internal static string Get(string key, params string[] words)
	{
		StringBuilder builder = new StringBuilder((Localization.instance != null) ? Localization.instance.Localize(key) : key);
		for (int i = words.Length - 1; i >= 0; i--)
		{
			builder.Replace("$" + i, words[i]);
		}
		return builder.ToString();
	}
}
