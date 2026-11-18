# 🛡️ Better Equip Slots

Adds a character sheet next to your inventory, with dedicated armour slots, six quick slots, loadouts, a food bag, a style tab for your gear and your stats at a glance. Worn armour stops taking up space in your backpack, and everything works with a controller.
<br>
<br>

## 💶 Donations: 

If you enjoy using this Valheim mod, please consider making a small donation to help support its development and future updates.

Even £1 or $1 would be greatly appreciated and would help more than you might think!

Thank you for your support, and happy gaming! ⚔️  ...... (A Paypal account is NOT required!)

<a href="https://www.paypal.com/donate/?hosted_button_id=BN8HEWHRMAREU">![](https://imgup.uk/i/n7Sg7WK9.png)</a>
<br>
<br>

## ⚠️ Incompatibilities:

  * Do not use alongside EquipmentAndQuickSlots or CDEV.EquipmentSlots. This mod replaces both and switches itself off if either one is installed.

<br>

## 🔥 Features:

  * **Equipment slots**: helmet, chest, legs, cape, belt, trinket and ammo. Drag an item into a slot to wear it, or drag it out to take it off. Right-clicking also equips and unequips.
  * **Multiple utility items**: wear up to 5 belts and similar items at once (no two of the same kind). Each gets its own cell in a window beside the belt slot, and all of their stats and effects apply. This is built in from Equip Multiple Utility Items, so you can remove that mod; your settings from it carry over.
  * **Six quick slots**: hold anything and bind each slot to any key or key combination (`Z`, `V`, `B` by default, plus the same keys with `Left Alt`). They appear on the HUD next to the Forsaken power, with the stack size (when a stack runs out the next one from your inventory takes its place), and a potion that can't be used yet is dimmed with the time left over it.
  * **Food bag**: a Food section on the Equipment tab with slots (3 by default, up to 6) that take only food. Auto Eat keeps you fed from it: a free food slot is filled, and a food about to run out is eaten again, or another food from the bag. `Left Alt + F` eats from the bag right now. It uses the game's own eating, so a food can be eaten again only after half of its time.
  * **Style tab**: change how your helmet, chest, legs and cape look, using any piece you have ever picked up, or hide a piece entirely. It is purely cosmetic: no armour, weight or effects change. Four buttons under the character save a style and put it back whatever gear you wear (left click uses it, right click saves the current one; with a controller, A and X). Other players see your style even if they don't have the mod.
  * **Info tab**: health, stamina and eitr with their regeneration, carry weight, your current weapon and its damage, armour (per worn piece), armour set bonuses, block, resistances, active food and status effects.
  * **Character preview**: drag to rotate your character and pan up and down, and use the mouse wheel to zoom. Left-click to swing your weapon, right-click to block.
  * **Compact panel**: leave out the slots you don't use (Panel → Hidden Slots) or pick a preset (Panel → Preset: Full, Compact, Minimal). A slot with an item in it is always shown.
  * **Movable panel**: drag the panel by its frame to anywhere on screen and it stays there (Panel → Offset).
  * **Controller support**: on the hotbar, D-pad left / right continues onto the quick slots and D-pad up uses the selected one. In the inventory, RB / LB stop on the panel; use the D-pad to move, A / X / LT / RT work as they do in the inventory, and the right stick switches tabs.
  * **Loadouts**: four saved sets of worn gear and quick slot items. Left click a button under your character on the Equipment tab to put a loadout on, right click to save what you wear now to it (with a controller, move onto a button with the D-pad: A puts it on, X saves). Each also has a key (`Keypad 1` to `Keypad 4` by default; hold `Left Shift` with it to save). A loadout also remembers your weapon and shield, so it swaps weapon sets too. Items are found in your inventory by kind, preferring the same quality and the best repair; slots the loadout leaves empty are emptied, and missing items are reported.
  * **Durability**: worn gear that is nearly broken is tinted red on the panel and you get a warning message once. At a workbench the repair button repairs everything repairable in one press.
  * **Padlocks**: lock an inventory cell so it is never emptied by "stack similar" or by other mods' store and sort buttons.
  * **Ammo**: shows your next ammo and how much is left above a bow, crossbow or fishing rod on the hotbar, and automatically equips more arrows (bolts, bait) when the current stack runs out.
  * **Death**: your worn gear is placed in the top row of your grave, and *Take All* puts everything back where it was and re-equips it.
  * **Easy switching**: items from EquipmentAndQuickSlots are converted the first time you start the game, and CDEV.EquipmentSlots' slots, styles, locks and settings carry over unchanged.

<br>

## 🤝 Works with:

  * Better Archery (quiver goes in the ammo slot), QuickStackStore, Local Craft From Storage Prime, MouseTweaks, Adventure Backpacks, Valheim Plus, BetterUI (quality stars), Epic Loot (rarity colours), Einherjar Bodies (shows your Einherjar body on the character preview), Jewelcrafting (its rings go in the Ring slot, its necklaces in the Amulet slot), Extra Food Slot (the food bag keeps all four food slots fed), and Backpacks and Advanced Backpacks (backpack goes in the Backpack slot).

<br>

## ⚙️ Configuration:

  * Change settings in game (with Configuration Manager or the mod settings menu, F7) or by editing `MagicMike.BetterEquipSlots.cfg`. Defaults are shown in brackets.
  * **General Settings**: **Enable Plugin** (on; local, needs a restart).
  * **Quick Slots**: **Auto Refill** (on), **Key 1** to **Key 6**, **Alt Modifier** (Left Alt: the HUD shows slots 4-6 while held), **Keys Win Over Vanilla** (off), **Gamepad Hotbar** (on).
  * **HUD**: **Show Quick Slots** (on), **Offset**, **Spacing** (8), **Show Cooldown** (on).
  * **Ammo**: **Show On Hotbar** (on), **Auto Equip** (on).
  * **Loadouts**: **Enable Loadouts** (on), **Save Modifier** (Left Shift), **Key 1** to **Key 4** (Keypad 1 to 4), **Include Weapons** (on), **Include Style** (off).
  * **Food Bag**: **Slots** (3, 0 to 6; 0 is no food bag), **Auto Eat** (on), **Refresh Below** (5 percent, 0 is only when a food has run out), **Use Another Food** (on), **Eat Key** (Left Alt + F), **Messages** (on).
  * **Durability**: **Warn Below** (20 percent, 0 is off), **Repair All At Once** (on).
  * **Inventory**: **Cell Locks** (on; enables the padlocks).
  * **Panel**: **Offset**, **Show Character** (on), **Hidden Slots** (none), **Preset** (Custom), **Keep Worn In Inventory** (off), **Show On Key** (off), **Key** (U).
  * **Utility**: **Multiple Utility Items** (on; allows wearing several and shows their window), **Max Utility Items** (5, minimum 2). Both are set by the server.
  * **Jewelry**: **Rings**, **Earrings**, **Amulets**: the item prefab names that go in those slots. Set by the server.

<br>

## 🌐 Multiplayer:

  * Install on clients. If a server also has it installed, every player joining needs it too; servers without it let anyone in.
  * **Before uninstalling**, empty all the slots. Without the mod, the game drops anything left in them at your feet.

<br>

## 📦 Requirements:

  * <a href="https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/">BepInExPack Valheim</a>
  * <a href="https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/">Jötunn</a>

<br>

## 💻 Installation:

### Automatic Install

  * Designed to be installed with r2modman or Gale.
If you downloaded from Nexus, install manually.

### Manual Install

  * Unzip and drag the `plugins` folder into your `BepInEx` folder.

<br>

## 🙏 Credits:

  * Multiple utility items feature built in from <b>Equip Multiple Utility Items</b> by aedenthorn (public domain).

<br>

## 💥 Plugins Created by Magic Mike:
  * <a href="https://valheim.hexium.gg/mods/MagicMike/CoreWoodExtras">Corewood Extras</a>
  * <a href="https://valheim.hexium.gg/mods/MagicMike/CorewoodFortified">Corewood Fortified</a>
  * <a href="https://valheim.hexium.gg/mods/MagicMike/CorewoodAdditions">Corewood Additions</a>
  * <a href="https://valheim.hexium.gg/mods/MagicMike/MyDirtyHoe">My Dirty Hoe</a>
  * <a href="https://valheim.hexium.gg/mods/MagicMike/ChestPreview">Chest Preview</a>
  * <a href="https://valheim.hexium.gg/mods/MagicMike/YabbaDabbaDoo">YabbaDabbaDoo</a>
  * <a href="https://valheim.hexium.gg/mods/MagicMike/BetterBuildMenu">Better Build Menu</a>
  * <a href="https://valheim.hexium.gg/mods/MagicMike/BetterEquipSlots">Better Equip Slots</a>
  * <a href="https://valheim.hexium.gg/mods/MagicMike/ExtraInventorySlots">Extra Inventory Slots</a>
  * <a href="https://valheim.hexium.gg/mods/MagicMike/VillagePlebs">Village Plebs</a>
  * <a href="https://valheim.hexium.gg/mods/MagicMike/EinherjarBodies">Einherjar Bodies</a>

<br>

  
## 💬 Comments/Suggestions/Help: <a href="https://www.nexusmods.com/profile/MagicMikeMods/mods?gameId=3667">Nexus</a>, <a href="https://discord.gg/mnr36G6J4x">Discord</a>.
