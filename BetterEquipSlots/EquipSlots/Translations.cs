using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;

namespace BetterEquipSlots.EquipSlots;

internal static class Translations
{
	private static readonly string[] Keys = new string[51]
	{
		"cdev_es_title", "cdev_es_quick", "cdev_es_quiver", "cdev_es_stash", "cdev_es_slot_helmet", "cdev_es_slot_chest", "cdev_es_slot_legs", "cdev_es_slot_cape", "cdev_es_slot_belt", "cdev_es_slot_trinket",
		"cdev_es_slot_ring", "cdev_es_slot_earring", "cdev_es_slot_amulet", "cdev_es_slot_ammo", "cdev_es_slot_quick", "cdev_es_slot_quiver", "cdev_es_slot_stash", "cdev_es_quick_hint", "cdev_es_nospace", "cdev_es_nojewelry",
		"cdev_es_preview_attack", "cdev_es_preview_block", "cdev_es_preview_attack_icon", "cdev_es_preview_block_icon", "cdev_es_lock", "cdev_es_utility", "cdev_es_look", "cdev_es_look_hint", "cdev_es_look_tip", "cdev_es_look_cell",
		"cdev_es_look_none", "cdev_es_look_none_tip", "cdev_es_look_empty", "cdev_es_keyconflict", "cdev_es_keyconflict_slot", "cdev_es_keyconflict_panel", "cdev_es_keyconflict_panelname", "cdev_es_keyunreadable", "cdev_es_look_hidden", "cdev_es_look_hidden_tip",
		"cdev_es_info", "cdev_es_info_tip", "cdev_es_info_vitals", "cdev_es_info_attack", "cdev_es_info_defense", "cdev_es_info_resist", "cdev_es_info_gear", "cdev_es_info_normal", "cdev_es_info_comfort", "cdev_es_info_food",
		"cdev_es_info_effects"
	};

	private static readonly Dictionary<string, string[]> Languages = new Dictionary<string, string[]>
	{
		["English"] = new string[51]
		{
			"Equipment", "Quick slots", "Quiver", "Did not fit", "Helmet", "Chest", "Legs", "Cape", "Belt", "Trinket",
			"Ring", "Earring", "Amulet", "Ammo", "Quick slot", "Quiver: arrows", "Did not fit", "Any item. Its key uses it like a hotbar key.", "No free space in the inventory", "Needs the jewelry mod CDEV.ExtJewelry.",
			"LMB attack", "RMB block", "attack", "block", "Lock slot", "Utility", "Style", "Looks only", "Choose how your helmet, chest, legs and cape look, from every piece you have ever held. A look is not an item: no armour, no effects, no weight. Your armour on the Equipment tab stays on.", "Left click: choose a look. Right click: remove it. Or drop armour here to take its look.",
			"No look", "Shows what is worn.", "Nothing of this kind has been in your hands yet.", "Quick slot $0 ($1) is also bound to: $2", "quick slot $0", "Equipment window key ($0) is also bound to: $1", "equipment window key", "The game cannot read this key: $0 ($1) - choose another one", "Hidden", "Hides this piece: nothing is drawn here, whatever is worn.",
			"Info", "Your character at a glance: health, stamina, regeneration, damage, armour, resistances and what your gear changes.", "Vitals", "Attack", "Defence", "Resistances", "Gear and effects", "Normal", "Comfort", "Food",
			"Effects"
		},
		["Russian"] = new string[51]
		{
			"Экипировка", "Быстрые слоты", "Колчан", "Не поместилось", "Шлем", "Нагрудник", "Штаны", "Плащ", "Пояс", "Тринкет",
			"Кольцо", "Серьга", "Амулет", "Стрелы", "Быстрый слот", "Колчан: стрелы", "Не поместилось", "Любой предмет. Клавиша слота использует его, как клавиша панели 1-8.", "Нет места в инвентаре", "Нужен мод CDEV.ExtJewelry (бижутерия).",
			"ЛКМ удар", "ПКМ блок", "удар", "блок", "Заблокировать слот", "Утилиты", "Стиль", "Только внешний вид", "Выберите, как выглядят шлем, нагрудник, штаны и плащ, из всего, что когда-либо было у вас в руках. Облик - не вещь: он не даёт брони и эффектов и ничего не весит. Броня на вкладке «Экипировка» остаётся надетой.", "ЛКМ - выбрать облик, ПКМ - убрать. Или перетащите сюда броню, чтобы взять её облик.",
			"Без облика", "Видно то, что надето.", "Таких вещей у вас в руках ещё не было.", "Быстрый слот $0 ($1) уже занят: $2", "быстрый слот $0", "Клавиша окна экипировки ($0) уже занята: $1", "клавиша окна экипировки", "Игра не может прочитать клавишу: $0 ($1) - выберите другую", "Скрыть", "Скрывает эту часть: здесь ничего не видно, что бы ни было надето.",
			"Информация", "Ваш персонаж одним взглядом: здоровье, выносливость, регенерация, урон, броня, сопротивления и что меняет снаряжение.", "Основное", "Атака", "Защита", "Сопротивления", "Снаряжение и эффекты", "Обычное", "Комфорт", "Еда",
			"Эффекты"
		},
		["Ukrainian"] = new string[51]
		{
			"Спорядження", "Швидкі слоти", "Сагайдак", "Не вмістилося", "Шолом", "Нагрудник", "Штани", "Плащ", "Пояс", "Дрібничка",
			"Каблучка", "Сережка", "Амулет", "Стріли", "Швидкий слот", "Сагайдак: стріли", "Не вмістилося", "Будь-який предмет. Клавіша слота використовує його, як клавіша панелі 1-8.", "Немає місця в інвентарі", "Потрібен мод CDEV.ExtJewelry (біжутерія).",
			"ЛКМ удар", "ПКМ блок", "удар", "блок", "Заблокувати слот", "Утиліти", "Стиль", "Лише вигляд", "Оберіть, який вигляд мають шолом, нагрудник, штани й плащ, з усього, що колись було у вас у руках. Образ - не річ: він не дає броні й ефектів і нічого не важить. Броня на вкладці «Спорядження» лишається вдягненою.", "ЛКМ - обрати образ, ПКМ - прибрати. Або перетягніть сюди броню, щоб узяти її образ.",
			"Без образу", "Видно те, що вдягнено.", "Таких речей у вас у руках ще не було.", "Швидкий слот $0 ($1) вже зайнятий: $2", "швидкий слот $0", "Клавіша вікна спорядження ($0) вже зайнята: $1", "клавіша вікна спорядження", "Гра не може прочитати клавішу: $0 ($1) - виберіть іншу", "Сховати", "Ховає цю частину: тут нічого не видно, хоч би що було вдягнено.",
			"Інформація", "Ваш персонаж одним поглядом: здоров'я, витривалість, регенерація, шкода, броня, опір і що змінює спорядження.", "Основне", "Атака", "Захист", "Опір", "Спорядження та ефекти", "Звичайне", "Комфорт", "Їжа",
			"Ефекти"
		},
		["German"] = new string[51]
		{
			"Ausrüstung", "Schnellplätze", "Köcher", "Passte nicht", "Helm", "Brust", "Beine", "Umhang", "Gürtel", "Schmuckstück",
			"Ring", "Ohrring", "Amulett", "Munition", "Schnellplatz", "Köcher: Pfeile", "Passte nicht", "Beliebiger Gegenstand. Seine Taste benutzt ihn wie eine Taste der Schnellleiste.", "Kein freier Platz im Inventar", "Benötigt den Schmuck-Mod CDEV.ExtJewelry.",
			"LMT Angriff", "RMT Blocken", "Angriff", "Blocken", "Platz sperren", "Zubehör", "Stil", "Nur Aussehen", "Wähle das Aussehen von Helm, Brust, Beinen und Umhang aus allem, was du je in der Hand hattest. Ein Aussehen ist kein Gegenstand: keine Rüstung, keine Effekte, kein Gewicht. Deine Rüstung im Ausrüstungs-Tab bleibt angelegt.", "Linksklick: Aussehen wählen. Rechtsklick: entfernen. Oder Rüstung hierher ziehen, um ihr Aussehen zu übernehmen.",
			"Kein Aussehen", "Zeigt, was getragen wird.", "So etwas hattest du noch nie in der Hand.", "Schnellplatz $0 ($1) ist auch belegt durch: $2", "Schnellplatz $0", "Taste des Ausrüstungsfensters ($0) ist auch belegt durch: $1", "Taste des Ausrüstungsfensters", "Das Spiel kann diese Taste nicht lesen: $0 ($1) - wähle eine andere", "Ausblenden", "Blendet dieses Teil aus: hier ist nichts zu sehen, egal was getragen wird.",
			"Info", "Dein Charakter auf einen Blick: Gesundheit, Ausdauer, Regeneration, Schaden, Rüstung, Resistenzen und was deine Ausrüstung ändert.", "Werte", "Angriff", "Verteidigung", "Resistenzen", "Ausrüstung und Effekte", "Normal", "Komfort", "Nahrung",
			"Effekte"
		},
		["French"] = new string[51]
		{
			"Équipement", "Emplacements rapides", "Carquois", "N'a pas pu être rangé", "Casque", "Torse", "Jambes", "Cape", "Ceinture", "Babiole",
			"Anneau", "Boucle d'oreille", "Amulette", "Munitions", "Emplacement rapide", "Carquois : flèches", "N'a pas pu être rangé", "N'importe quel objet. Sa touche l'utilise comme une touche de la barre rapide.", "Plus de place dans l'inventaire", "Nécessite le mod de bijoux CDEV.ExtJewelry.",
			"Clic gauche : attaque", "Clic droit : parade", "attaque", "parade", "Verrouiller la case", "Utilitaires", "Style", "Apparence seule", "Choisissez l'apparence du casque, du torse, des jambes et de la cape parmi tout ce que vous avez déjà tenu. Une apparence n'est pas un objet : ni armure, ni effets, ni poids. Votre armure de l'onglet Équipement reste portée.", "Clic gauche : choisir une apparence. Clic droit : la retirer. Ou déposez ici une armure pour prendre son apparence.",
			"Aucune apparence", "Montre ce qui est porté.", "Vous n'avez encore jamais tenu d'objet de ce type.", "Emplacement rapide $0 ($1) est aussi utilisé par : $2", "emplacement rapide $0", "La touche de la fenêtre d'équipement ($0) est aussi utilisée par : $1", "touche de la fenêtre d'équipement", "Le jeu ne peut pas lire cette touche : $0 ($1) - choisissez-en une autre", "Masqué", "Masque cette pièce : rien n'apparaît ici, quoi que vous portiez.",
			"Infos", "Votre personnage en un coup d'œil : santé, endurance, régénération, dégâts, armure, résistances et ce que change votre équipement.", "Statistiques", "Attaque", "Défense", "Résistances", "Équipement et effets", "Normal", "Confort", "Nourriture",
			"Effets"
		},
		["Spanish"] = new string[51]
		{
			"Equipo", "Ranuras rápidas", "Carcaj", "No cupo", "Casco", "Pecho", "Piernas", "Capa", "Cinturón", "Abalorio",
			"Anillo", "Pendiente", "Amuleto", "Munición", "Ranura rápida", "Carcaj: flechas", "No cupo", "Cualquier objeto. Su tecla lo usa como una tecla de la barra rápida.", "No hay espacio libre en el inventario", "Requiere el mod de joyería CDEV.ExtJewelry.",
			"Clic izq.: atacar", "Clic der.: bloquear", "atacar", "bloquear", "Bloquear casilla", "Utilidades", "Estilo", "Solo apariencia", "Elige el aspecto del casco, el pecho, las piernas y la capa entre todo lo que has tenido alguna vez. Un aspecto no es un objeto: sin armadura, sin efectos, sin peso. Tu armadura de la pestaña Equipo sigue puesta.", "Clic izquierdo: elegir un aspecto. Clic derecho: quitarlo. O suelta aquí una armadura para tomar su aspecto.",
			"Sin aspecto", "Muestra lo que llevas puesto.", "Aún no has tenido nada de este tipo.", "La ranura rápida $0 ($1) también está asignada a: $2", "ranura rápida $0", "La tecla de la ventana de equipo ($0) también está asignada a: $1", "tecla de la ventana de equipo", "El juego no puede leer esta tecla: $0 ($1) - elige otra", "Oculto", "Oculta esta pieza: aquí no se ve nada, lleves lo que lleves.",
			"Información", "Tu personaje de un vistazo: salud, aguante, regeneración, daño, armadura, resistencias y lo que cambia tu equipo.", "Estado", "Ataque", "Defensa", "Resistencias", "Equipo y efectos", "Normal", "Comodidad", "Comida",
			"Efectos"
		},
		["Italian"] = new string[51]
		{
			"Equipaggiamento", "Slot rapidi", "Faretra", "Non c'era spazio", "Elmo", "Torso", "Gambe", "Mantello", "Cintura", "Ninnolo",
			"Anello", "Orecchino", "Amuleto", "Munizioni", "Slot rapido", "Faretra: frecce", "Non c'era spazio", "Qualsiasi oggetto. Il suo tasto lo usa come un tasto della barra rapida.", "Nessuno spazio libero nell'inventario", "Richiede la mod di gioielli CDEV.ExtJewelry.",
			"Clic sin.: attacco", "Clic des.: parata", "attacco", "parata", "Blocca casella", "Utilità", "Stile", "Solo aspetto", "Scegli l'aspetto di elmo, torso, gambe e mantello tra tutto ciò che hai mai avuto. Un aspetto non è un oggetto: niente armatura, effetti o peso. L'armatura della scheda Equipaggiamento resta indossata.", "Clic sinistro: scegli un aspetto. Clic destro: rimuovilo. Oppure trascina qui un'armatura per prenderne l'aspetto.",
			"Nessun aspetto", "Mostra ciò che indossi.", "Non hai ancora avuto niente di questo tipo.", "Lo slot rapido $0 ($1) è assegnato anche a: $2", "slot rapido $0", "Il tasto della finestra equipaggiamento ($0) è assegnato anche a: $1", "tasto della finestra equipaggiamento", "Il gioco non può leggere questo tasto: $0 ($1) - scegline un altro", "Nascosto", "Nasconde questo pezzo: qui non si vede nulla, qualunque cosa indossi.",
			"Info", "Il tuo personaggio a colpo d'occhio: salute, vigore, rigenerazione, danni, armatura, resistenze e cosa cambia il tuo equipaggiamento.", "Parametri", "Attacco", "Difesa", "Resistenze", "Equipaggiamento ed effetti", "Normale", "Comfort", "Cibo",
			"Effetti"
		},
		["Portuguese_Brazilian"] = new string[51]
		{
			"Equipamento", "Espaços rápidos", "Aljava", "Não coube", "Capacete", "Peitoral", "Pernas", "Capa", "Cinto", "Bugiganga",
			"Anel", "Brinco", "Amuleto", "Munição", "Espaço rápido", "Aljava: flechas", "Não coube", "Qualquer item. Sua tecla o usa como uma tecla da barra rápida.", "Sem espaço livre no inventário", "Requer o mod de joias CDEV.ExtJewelry.",
			"Botão esq.: atacar", "Botão dir.: bloquear", "atacar", "bloquear", "Travar espaço", "Utilitários", "Estilo", "Só aparência", "Escolha a aparência do elmo, peitoral, pernas e capa entre tudo o que você já teve. Uma aparência não é um item: sem armadura, efeitos ou peso. Sua armadura da aba Equipamento continua vestida.", "Clique esquerdo: escolher uma aparência. Clique direito: removê-la. Ou solte uma armadura aqui para pegar a aparência dela.",
			"Sem aparência", "Mostra o que está vestido.", "Você ainda não teve nada deste tipo.", "O espaço rápido $0 ($1) também está em: $2", "espaço rápido $0", "A tecla da janela de equipamento ($0) também está em: $1", "tecla da janela de equipamento", "O jogo não consegue ler esta tecla: $0 ($1) - escolha outra", "Oculto", "Oculta esta peça: nada aparece aqui, seja o que for que esteja vestido.",
			"Informações", "Seu personagem num relance: vida, vigor, regeneração, dano, armadura, resistências e o que seu equipamento muda.", "Atributos", "Ataque", "Defesa", "Resistências", "Equipamento e efeitos", "Normal", "Conforto", "Comida",
			"Efeitos"
		},
		["Portuguese_European"] = new string[51]
		{
			"Equipamento", "Espaços rápidos", "Aljava", "Não coube", "Capacete", "Peitoral", "Pernas", "Capa", "Cinto", "Berloque",
			"Anel", "Brinco", "Amuleto", "Munições", "Espaço rápido", "Aljava: flechas", "Não coube", "Qualquer objeto. A sua tecla usa-o como uma tecla da barra rápida.", "Sem espaço livre no inventário", "Requer o mod de joias CDEV.ExtJewelry.",
			"Botão esq.: atacar", "Botão dir.: bloquear", "atacar", "bloquear", "Trancar espaço", "Utilitários", "Estilo", "Só aparência", "Escolhe o aspeto do elmo, peitoral, pernas e capa entre tudo o que já tiveste. Um aspeto não é um objeto: sem armadura, efeitos ou peso. A tua armadura do separador Equipamento continua vestida.", "Clique esquerdo: escolher um aspeto. Clique direito: removê-lo. Ou larga aqui uma armadura para ficar com o aspeto dela.",
			"Sem aspeto", "Mostra o que está vestido.", "Ainda não tiveste nada deste tipo.", "A ranhura rápida $0 ($1) também está em: $2", "ranhura rápida $0", "A tecla da janela de equipamento ($0) também está em: $1", "tecla da janela de equipamento", "O jogo não consegue ler esta tecla: $0 ($1) - escolha outra", "Oculto", "Oculta esta peça: nada aparece aqui, seja o que for que tenha vestido.",
			"Informações", "A tua personagem num relance: vida, vigor, regeneração, dano, armadura, resistências e o que o teu equipamento muda.", "Atributos", "Ataque", "Defesa", "Resistências", "Equipamento e efeitos", "Normal", "Conforto", "Comida",
			"Efeitos"
		},
		["Polish"] = new string[51]
		{
			"Ekwipunek", "Szybkie sloty", "Kołczan", "Nie zmieściło się", "Hełm", "Tors", "Nogi", "Peleryna", "Pas", "Błyskotka",
			"Pierścień", "Kolczyk", "Amulet", "Amunicja", "Szybki slot", "Kołczan: strzały", "Nie zmieściło się", "Dowolny przedmiot. Jego klawisz używa go jak klawisza paska szybkiego dostępu.", "Brak wolnego miejsca w ekwipunku", "Wymaga moda z biżuterią CDEV.ExtJewelry.",
			"LPM atak", "PPM blok", "atak", "blok", "Zablokuj pole", "Użytkowe", "Styl", "Tylko wygląd", "Wybierz wygląd hełmu, napierśnika, nogawic i płaszcza spośród wszystkiego, co kiedykolwiek miałeś. Wygląd to nie przedmiot: bez pancerza, efektów i wagi. Pancerz z karty Ekwipunek pozostaje założony.", "LPM: wybierz wygląd. PPM: usuń go. Albo upuść tu pancerz, by przejąć jego wygląd.",
			"Bez wyglądu", "Widać to, co założone.", "Nie miałeś jeszcze niczego takiego.", "Szybki slot $0 ($1) jest też przypisany do: $2", "szybki slot $0", "Klawisz okna ekwipunku ($0) jest też przypisany do: $1", "klawisz okna ekwipunku", "Gra nie może odczytać tego klawisza: $0 ($1) - wybierz inny", "Ukryty", "Ukrywa tę część: nic tu nie widać, cokolwiek masz na sobie.",
			"Informacje", "Twoja postać w skrócie: zdrowie, wytrzymałość, regeneracja, obrażenia, pancerz, odporności i to, co zmienia ekwipunek.", "Statystyki", "Atak", "Obrona", "Odporności", "Ekwipunek i efekty", "Normalne", "Komfort", "Jedzenie",
			"Efekty"
		},
		["Czech"] = new string[51]
		{
			"Výbava", "Rychlé sloty", "Toulec", "Nevešlo se", "Helma", "Hruď", "Nohy", "Plášť", "Opasek", "Cetka",
			"Prsten", "Náušnice", "Amulet", "Munice", "Rychlý slot", "Toulec: šípy", "Nevešlo se", "Libovolný předmět. Jeho klávesa ho použije jako klávesa rychlé lišty.", "V inventáři není volné místo", "Vyžaduje mod se šperky CDEV.ExtJewelry.",
			"LTM útok", "PTM blok", "útok", "blok", "Zamknout políčko", "Užitkové", "Styl", "Jen vzhled", "Vyber vzhled helmy, hrudi, nohou a pláště ze všeho, co jsi kdy měl. Vzhled není předmět: žádná zbroj, efekty ani váha. Zbroj na kartě Výbava zůstává nasazená.", "Levé tlačítko: vybrat vzhled. Pravé: odebrat. Nebo sem přetáhni zbroj a převezmi její vzhled.",
			"Bez vzhledu", "Je vidět, co máš na sobě.", "Nic takového jsi ještě neměl.", "Rychlý slot $0 ($1) je také přiřazen k: $2", "rychlý slot $0", "Klávesa okna vybavení ($0) je také přiřazena k: $1", "klávesa okna vybavení", "Hra nedokáže přečíst tuto klávesu: $0 ($1) - zvolte jinou", "Skrýt", "Skryje tento kus: nic tu není vidět, ať máš na sobě cokoli.",
			"Informace", "Tvá postava na první pohled: zdraví, výdrž, regenerace, poškození, zbroj, odolnosti a co mění tvé vybavení.", "Základní", "Útok", "Obrana", "Odolnosti", "Vybavení a efekty", "Normální", "Pohodlí", "Jídlo",
			"Efekty"
		},
		["Hungarian"] = new string[51]
		{
			"Felszerelés", "Gyorshelyek", "Tegez", "Nem fért el", "Sisak", "Mellvért", "Lábvért", "Köpeny", "Öv", "Csecsebecse",
			"Gyűrű", "Fülbevaló", "Amulett", "Lőszer", "Gyorshely", "Tegez: nyilak", "Nem fért el", "Bármilyen tárgy. A billentyűje úgy használja, mint a gyorssáv billentyűit.", "Nincs szabad hely a tárgylistában", "Szükséges az ékszer mod: CDEV.ExtJewelry.",
			"Bal klikk: támadás", "Jobb klikk: védés", "támadás", "védés", "Cella zárolása", "Kiegészítők", "Stílus", "Csak megjelenés", "Válaszd ki a sisak, mellvért, lábvért és köpeny kinézetét mindabból, ami valaha nálad volt. A kinézet nem tárgy: nincs páncélja, hatása, súlya. A Felszerelés fül páncélja rajtad marad.", "Bal klikk: kinézet választása. Jobb klikk: eltávolítás. Vagy húzz ide egy páncélt a kinézetéért.",
			"Nincs kinézet", "Azt mutatja, ami rajtad van.", "Ilyen tárgy még nem volt nálad.", "A(z) $0. gyorshely ($1) máshoz is hozzá van rendelve: $2", "$0. gyorshely", "A felszerelés ablak gombja ($0) máshoz is hozzá van rendelve: $1", "felszerelés ablak gombja", "A játék nem tudja olvasni ezt a gombot: $0 ($1) - válassz másikat", "Elrejtve", "Elrejti ezt a darabot: itt semmi sem látszik, bármit viselsz.",
			"Információ", "A karaktered egy pillantással: életerő, állóképesség, regeneráció, sebzés, páncél, ellenállások és amit a felszerelésed változtat.", "Alapértékek", "Támadás", "Védekezés", "Ellenállások", "Felszerelés és hatások", "Normál", "Kényelem", "Étel",
			"Hatások"
		},
		["Dutch"] = new string[51]
		{
			"Uitrusting", "Snelle vakken", "Pijlkoker", "Paste niet", "Helm", "Borst", "Benen", "Cape", "Riem", "Snuisterij",
			"Ring", "Oorbel", "Amulet", "Munitie", "Snel vak", "Pijlkoker: pijlen", "Paste niet", "Elk voorwerp. De toets gebruikt het als een toets van de snelbalk.", "Geen vrije ruimte in de inventaris", "Vereist de sieraden-mod CDEV.ExtJewelry.",
			"LMK: aanval", "RMK: blokkeren", "aanval", "blokkeren", "Vakje vergrendelen", "Hulpmiddelen", "Stijl", "Alleen uiterlijk", "Kies het uiterlijk van helm, borst, benen en mantel uit alles wat je ooit hebt gehad. Een uiterlijk is geen voorwerp: geen bepantsering, effecten of gewicht. Je bepantsering op het tabblad Uitrusting blijft aan.", "Linksklik: kies een uiterlijk. Rechtsklik: verwijder het. Of sleep hier bepantsering heen om het uiterlijk over te nemen.",
			"Geen uiterlijk", "Toont wat je draagt.", "Zoiets heb je nog nooit gehad.", "Snelvak $0 ($1) is ook toegewezen aan: $2", "snelvak $0", "Toets van het uitrustingsvenster ($0) is ook toegewezen aan: $1", "toets van het uitrustingsvenster", "Het spel kan deze toets niet lezen: $0 ($1) - kies een andere", "Verborgen", "Verbergt dit stuk: hier is niets te zien, wat je ook draagt.",
			"Info", "Je personage in één oogopslag: gezondheid, uithoudingsvermogen, regeneratie, schade, pantser, weerstanden en wat je uitrusting verandert.", "Basiswaarden", "Aanval", "Verdediging", "Weerstanden", "Uitrusting en effecten", "Normaal", "Comfort", "Voedsel",
			"Effecten"
		},
		["Swedish"] = new string[51]
		{
			"Utrustning", "Snabbplatser", "Koger", "Fick inte plats", "Hjälm", "Bröst", "Ben", "Mantel", "Bälte", "Prydnad",
			"Ring", "Örhänge", "Amulett", "Ammunition", "Snabbplats", "Koger: pilar", "Fick inte plats", "Valfritt föremål. Dess tangent använder det som en snabbfältstangent.", "Inget ledigt utrymme i inventariet", "Kräver smyckesmodden CDEV.ExtJewelry.",
			"V-klick: attack", "H-klick: blockera", "attack", "blockera", "Lås ruta", "Tillbehör", "Stil", "Bara utseende", "Välj hur hjälm, bröst, ben och mantel ser ut bland allt du någonsin haft. Ett utseende är inget föremål: ingen rustning, inga effekter, ingen vikt. Rustningen på fliken Utrustning sitter kvar.", "Vänsterklick: välj ett utseende. Högerklick: ta bort det. Eller släpp rustning här för att ta dess utseende.",
			"Inget utseende", "Visar det du bär.", "Du har aldrig haft något sådant än.", "Snabbplats $0 ($1) används också av: $2", "snabbplats $0", "Tangenten för utrustningsfönstret ($0) används också av: $1", "tangenten för utrustningsfönstret", "Spelet kan inte läsa den här tangenten: $0 ($1) - välj en annan", "Dold", "Döljer den här delen: inget syns här, vad du än bär.",
			"Info", "Din karaktär i korthet: hälsa, uthållighet, återhämtning, skada, rustning, motstånd och vad din utrustning ändrar.", "Grundvärden", "Anfall", "Försvar", "Motstånd", "Utrustning och effekter", "Normal", "Komfort", "Mat",
			"Effekter"
		},
		["Norwegian"] = new string[51]
		{
			"Utstyr", "Hurtigplasser", "Kogger", "Fikk ikke plass", "Hjelm", "Bryst", "Bein", "Kappe", "Belte", "Pyntegjenstand",
			"Ring", "Ørering", "Amulett", "Ammunisjon", "Hurtigplass", "Kogger: piler", "Fikk ikke plass", "Hvilken som helst gjenstand. Tasten bruker den som en hurtiglinjetast.", "Ingen ledig plass i inventaret", "Krever smykkemodden CDEV.ExtJewelry.",
			"V-klikk: angrep", "H-klikk: blokker", "angrep", "blokker", "Lås rute", "Tilbehør", "Stil", "Bare utseende", "Velg hvordan hjelm, bryst, ben og kappe ser ut blant alt du noen gang har hatt. Et utseende er ingen gjenstand: ingen rustning, effekter eller vekt. Rustningen på fanen Utstyr blir sittende på.", "Venstreklikk: velg et utseende. Høyreklikk: fjern det. Eller slipp rustning her for å ta utseendet.",
			"Intet utseende", "Viser det du har på.", "Du har aldri hatt noe slikt ennå.", "Hurtigplass $0 ($1) brukes også av: $2", "hurtigplass $0", "Tasten for utstyrsvinduet ($0) brukes også av: $1", "tasten for utstyrsvinduet", "Spillet kan ikke lese denne tasten: $0 ($1) - velg en annen", "Skjult", "Skjuler denne delen: ingenting vises her, uansett hva du har på deg.",
			"Info", "Karakteren din på et øyeblikk: helse, utholdenhet, regenerering, skade, rustning, motstand og hva utstyret ditt endrer.", "Grunnverdier", "Angrep", "Forsvar", "Motstand", "Utstyr og effekter", "Normal", "Komfort", "Mat",
			"Effekter"
		},
		["Danish"] = new string[51]
		{
			"Udstyr", "Hurtigpladser", "Kogger", "Var der ikke plads til", "Hjelm", "Bryst", "Ben", "Kappe", "Bælte", "Pyntegenstand",
			"Ring", "Ørering", "Amulet", "Ammunition", "Hurtigplads", "Kogger: pile", "Var der ikke plads til", "Enhver genstand. Tasten bruger den som en hurtigbjælketast.", "Ingen ledig plads i inventaret", "Kræver smykke-moddet CDEV.ExtJewelry.",
			"V-klik: angreb", "H-klik: bloker", "angreb", "bloker", "Lås felt", "Tilbehør", "Stil", "Kun udseende", "Vælg hvordan hjelm, bryst, ben og kappe ser ud blandt alt, du nogensinde har haft. Et udseende er ingen genstand: ingen rustning, effekter eller vægt. Rustningen på fanen Udstyr bliver siddende.", "Venstreklik: vælg et udseende. Højreklik: fjern det. Eller slip rustning her for at tage dets udseende.",
			"Intet udseende", "Viser det, du har på.", "Du har aldrig haft noget lignende endnu.", "Hurtigplads $0 ($1) bruges også af: $2", "hurtigplads $0", "Tasten til udstyrsvinduet ($0) bruges også af: $1", "tasten til udstyrsvinduet", "Spillet kan ikke læse denne tast: $0 ($1) - vælg en anden", "Skjult", "Skjuler denne del: intet vises her, uanset hvad du har på.",
			"Info", "Din karakter på et øjeblik: helbred, udholdenhed, regenerering, skade, rustning, modstand og hvad dit udstyr ændrer.", "Grundværdier", "Angreb", "Forsvar", "Modstand", "Udstyr og effekter", "Normal", "Komfort", "Mad",
			"Effekter"
		},
		["Finnish"] = new string[51]
		{
			"Varusteet", "Pikapaikat", "Viini", "Ei mahtunut", "Kypärä", "Rinta", "Jalat", "Viitta", "Vyö", "Koriste",
			"Sormus", "Korvakoru", "Amuletti", "Ammukset", "Pikapaikka", "Viini: nuolet", "Ei mahtunut", "Mikä tahansa esine. Sen näppäin käyttää sitä kuin pikapalkin näppäin.", "Tavaraluettelossa ei ole tilaa", "Vaatii korumodin CDEV.ExtJewelry.",
			"Vasen: hyökkäys", "Oikea: torjunta", "hyökkäys", "torjunta", "Lukitse ruutu", "Apuvälineet", "Tyyli", "Vain ulkonäkö", "Valitse kypärän, rintapanssarin, housujen ja viitan ulkonäkö kaikesta, mitä sinulla on koskaan ollut. Ulkonäkö ei ole esine: ei panssaria, vaikutuksia eikä painoa. Varusteet-välilehden panssari pysyy päälläsi.", "Vasen klikkaus: valitse ulkonäkö. Oikea klikkaus: poista se. Tai pudota panssari tähän ottaaksesi sen ulkonäön.",
			"Ei ulkonäköä", "Näyttää sen, mitä sinulla on päällä.", "Sinulla ei ole vielä ollut mitään tällaista.", "Pikapaikka $0 ($1) on myös käytössä: $2", "pikapaikka $0", "Varustusikkunan näppäin ($0) on myös käytössä: $1", "varustusikkunan näppäin", "Peli ei voi lukea tätä näppäintä: $0 ($1) - valitse toinen", "Piilotettu", "Piilottaa tämän osan: tässä ei näy mitään, puitpa päällesi mitä tahansa.",
			"Tiedot", "Hahmosi yhdellä silmäyksellä: terveys, kestävyys, palautuminen, vahinko, panssari, vastustuskyvyt ja mitä varusteesi muuttavat.", "Perusarvot", "Hyökkäys", "Puolustus", "Vastustuskyvyt", "Varusteet ja vaikutukset", "Normaali", "Mukavuus", "Ruoka",
			"Vaikutukset"
		},
		["Turkish"] = new string[51]
		{
			"Teçhizat", "Hızlı yuvalar", "Sadak", "Sığmadı", "Miğfer", "Göğüs", "Bacaklar", "Pelerin", "Kemer", "Süs eşyası",
			"Yüzük", "Küpe", "Muska", "Mühimmat", "Hızlı yuva", "Sadak: oklar", "Sığmadı", "Herhangi bir eşya. Tuşu onu hızlı çubuk tuşu gibi kullanır.", "Envanterde boş yer yok", "CDEV.ExtJewelry takı modu gerekli.",
			"Sol tık: saldırı", "Sağ tık: blok", "saldırı", "blok", "Hücreyi kilitle", "Yardımcılar", "Stil", "Yalnızca görünüm", "Kask, göğüs, bacak ve pelerinin görünümünü şimdiye kadar elinde tuttuğun her şey arasından seç. Görünüm bir eşya değildir: zırh, etki veya ağırlık yok. Teçhizat sekmesindeki zırhın üzerinde kalır.", "Sol tık: görünüm seç. Sağ tık: kaldır. Ya da görünümünü almak için buraya bir zırh bırak.",
			"Görünüm yok", "Giydiğin şey görünür.", "Bu türden bir şey henüz eline geçmedi.", "Hızlı yuva $0 ($1) şuna da atanmış: $2", "hızlı yuva $0", "Ekipman penceresi tuşu ($0) şuna da atanmış: $1", "ekipman penceresi tuşu", "Oyun bu tuşu okuyamıyor: $0 ($1) - başka bir tuş seçin", "Gizli", "Bu parçayı gizler: ne giyilirse giyilsin burada hiçbir şey görünmez.",
			"Bilgi", "Karakterine bir bakışta: sağlık, dayanıklılık, yenilenme, hasar, zırh, dirençler ve ekipmanının neyi değiştirdiği.", "Temel", "Saldırı", "Savunma", "Dirençler", "Ekipman ve etkiler", "Normal", "Konfor", "Yiyecek",
			"Etkiler"
		},
		["Chinese"] = new string[51]
		{
			"装备", "快捷栏位", "箭袋", "放不下的物品", "头盔", "胸甲", "腿甲", "披风", "腰带", "饰品",
			"戒指", "耳环", "护身符", "弹药", "快捷栏位", "箭袋：箭矢", "放不下的物品", "任意物品。按下此栏位的按键即可使用，与快捷栏按键相同。", "背包没有空位", "需要首饰模组 CDEV.ExtJewelry。",
			"左键 攻击", "右键 格挡", "攻击", "格挡", "锁定格子", "辅助物品", "风格", "仅外观", "从你拿到过的所有物品中，选择头盔、胸甲、护腿和披风的外观。外观不是物品：没有护甲、效果和重量。装备页上的护甲仍然穿着。", "左键：选择外观。右键：移除。也可以把护甲拖到这里以采用它的外观。",
			"无外观", "显示穿着的装备。", "你还没有拿到过这类物品。", "快捷栏 $0（$1）也被占用：$2", "快捷栏 $0", "装备窗口按键（$0）也被占用：$1", "装备窗口按键", "游戏无法读取此按键：$0（$1）- 请换一个", "隐藏", "隐藏此部位：无论穿戴什么，这里都不显示。",
			"信息", "角色一览：生命、耐力、恢复、伤害、护甲、抗性以及装备带来的变化。", "基础", "攻击", "防御", "抗性", "装备与效果", "正常", "舒适度", "食物",
			"效果"
		},
		["Chinese_Trad"] = new string[51]
		{
			"裝備", "快捷欄位", "箭袋", "放不下的物品", "頭盔", "胸甲", "腿甲", "披風", "腰帶", "飾品",
			"戒指", "耳環", "護身符", "彈藥", "快捷欄位", "箭袋：箭矢", "放不下的物品", "任意物品。按下此欄位的按鍵即可使用，與快捷列按鍵相同。", "背包沒有空位", "需要首飾模組 CDEV.ExtJewelry。",
			"左鍵 攻擊", "右鍵 格擋", "攻擊", "格擋", "鎖定格子", "輔助物品", "風格", "僅外觀", "從你拿到過的所有物品中，選擇頭盔、胸甲、護腿和披風的外觀。外觀不是物品：沒有護甲、效果和重量。裝備頁上的護甲仍然穿著。", "左鍵：選擇外觀。右鍵：移除。也可以把護甲拖到這裡以採用它的外觀。",
			"無外觀", "顯示穿著的裝備。", "你還沒有拿到過這類物品。", "快捷欄 $0（$1）也被佔用：$2", "快捷欄 $0", "裝備視窗按鍵（$0）也被佔用：$1", "裝備視窗按鍵", "遊戲無法讀取此按鍵：$0（$1）- 請換一個", "隱藏", "隱藏此部位：無論穿戴什麼，這裡都不顯示。",
			"資訊", "角色一覽：生命、耐力、恢復、傷害、護甲、抗性以及裝備帶來的變化。", "基礎", "攻擊", "防禦", "抗性", "裝備與效果", "正常", "舒適度", "食物",
			"效果"
		},
		["Japanese"] = new string[51]
		{
			"装備", "クイックスロット", "矢筒", "入りきらなかった物", "兜", "胴", "脚", "マント", "ベルト", "装飾品",
			"指輪", "イヤリング", "アミュレット", "弾薬", "クイックスロット", "矢筒：矢", "入りきらなかった物", "任意のアイテム。スロットのキーでホットバーのキーと同じように使えます。", "インベントリに空きがありません", "装飾品MOD「CDEV.ExtJewelry」が必要です。",
			"左クリック 攻撃", "右クリック ブロック", "攻撃", "ブロック", "枠をロック", "ユーティリティ", "スタイル", "見た目のみ", "これまでに手にしたすべての装備から、兜・胴・脚・マントの見た目を選べます。見た目はアイテムではなく、防具も効果も重さもありません。装備タブの防具は装備されたままです。", "左クリック：見た目を選ぶ。右クリック：外す。防具をここにドロップすると、その見た目になります。",
			"見た目なし", "装備中のものが表示されます。", "この種類のアイテムはまだ手にしたことがありません。", "クイックスロット $0（$1）は次にも割り当てられています: $2", "クイックスロット $0", "装備ウィンドウのキー（$0）は次にも割り当てられています: $1", "装備ウィンドウのキー", "ゲームがこのキーを読み取れません: $0（$1）- 別のキーを選んでください", "非表示", "この部位を非表示にします。何を装備していても表示されません。",
			"情報", "キャラクターの概要：体力、スタミナ、回復、ダメージ、防具、耐性、装備による変化。", "基本", "攻撃", "防御", "耐性", "装備と効果", "通常", "快適度", "食料",
			"効果"
		},
		["Korean"] = new string[51]
		{
			"장비", "빠른 슬롯", "화살통", "들어가지 않은 물건", "투구", "흉갑", "다리", "망토", "허리띠", "장신구",
			"반지", "귀걸이", "부적", "탄약", "빠른 슬롯", "화살통: 화살", "들어가지 않은 물건", "아무 아이템. 슬롯 키로 단축키처럼 사용합니다.", "인벤토리에 빈 공간이 없습니다", "장신구 모드 CDEV.ExtJewelry가 필요합니다.",
			"좌클릭 공격", "우클릭 방어", "공격", "방어", "칸 잠그기", "유틸리티", "스타일", "외형 전용", "지금까지 손에 넣은 모든 장비 중에서 투구, 흉갑, 다리, 망토의 외형을 고르세요. 외형은 아이템이 아니어서 방어력, 효과, 무게가 없습니다. 장비 탭의 방어구는 그대로 착용됩니다.", "왼쪽 클릭: 외형 선택. 오른쪽 클릭: 제거. 방어구를 여기로 끌어다 놓으면 그 외형을 가져옵니다.",
			"외형 없음", "착용한 장비가 보입니다.", "이런 종류의 아이템은 아직 손에 넣은 적이 없습니다.", "빠른 슬롯 $0 ($1)이(가) 다른 곳에도 할당됨: $2", "빠른 슬롯 $0", "장비 창 키 ($0)이(가) 다른 곳에도 할당됨: $1", "장비 창 키", "게임이 이 키를 읽을 수 없습니다: $0 ($1) - 다른 키를 고르세요", "숨김", "이 부위를 숨깁니다. 무엇을 착용하든 표시되지 않습니다.",
			"정보", "캐릭터 한눈에 보기: 체력, 스태미나, 재생, 피해, 방어구, 저항, 장비가 바꾸는 것.", "기본", "공격", "방어", "저항", "장비와 효과", "보통", "안락함", "음식",
			"효과"
		}
	};

	internal static void Register()
	{
		CustomLocalization localization = LocalizationManager.Instance.GetLocalization();
		foreach (KeyValuePair<string, string[]> language in Languages)
		{
			if (language.Value.Length != Keys.Length)
			{
				Plugin.Log.LogError($"Translation '{language.Key}' has {language.Value.Length} texts, expected {Keys.Length}; skipped.");
				continue;
			}
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			for (int i = 0; i < Keys.Length; i++)
			{
				dictionary[Keys[i]] = language.Value[i];
			}
			localization.AddTranslation(language.Key, dictionary);
		}
		AddToLoadedLanguage();
	}

	private static void AddToLoadedLanguage()
	{
		Localization instance = Localization.m_instance;
		if (instance != null)
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
	}
}
