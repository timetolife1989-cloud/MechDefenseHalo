# Economy System Documentation

## Overview
Complete loot, crafting, inventory, equipment, and shop system for MechDefenseHalo with 250+ items, 40+ blueprints, 10 armor sets, and 20+ loot tables.

## Quick Start Guide

### UI Controls
- **I** - Toggle Inventory (500 slots)
- **E** - Toggle Equipment (15 slots + 5 loadouts)
- **C** - Toggle Crafting (3 concurrent jobs)
- **S** - Toggle Shop (3 categories)
- **ESC** - Close any UI

### Currency
- **Credits** 💰 - Earned through gameplay
- **Cores** 💎 - Premium currency

---

## System Architecture

### Autoloaded Managers
```
EventBus → Core event system
GameManager → Game state
ItemDatabase → 250+ items
LootTableManager → 20+ loot tables
CurrencyManager → Credits & Cores
InventoryManager → 500 slot inventory
EquipmentManager → 15 slots + loadouts
CraftingManager → Blueprint + queue
ShopManager → 3 category shop
SetManager → Set bonus tracking
```

---

## Items (250+ Total)

### Rarities (7 Tiers)
| Rarity | Color | Drop Rate | Description |
|--------|-------|-----------|-------------|
| Common | Gray | 60% | Basic items |
| Uncommon | Green | 25% | Improved stats |
| Rare | Blue | 10% | Good stats |
| Epic | Purple | 4% | Great stats, sets |
| Legendary | Orange | 0.9% | Excellent stats, abilities |
| Exotic | Yellow | 0.1% | Rare, powerful |
| Mythic | Red | 0.01% | Ultimate endgame |

### Mech Parts (80 items)
- **Heads** (20) - HP, Shield, Crit, Detection
- **Torsos** (20) - HP, Shield, Resist, Regen
- **Arms** (20) - Damage, FireRate, Crit
- **Legs** (20) - Speed, Dodge, Jump

### Weapon Mods (60 items)
- **Barrels** (20) - Range, Accuracy, Damage
- **Magazines** (20) - Ammo, Reload, Special
- **Optics** (20) - Accuracy, Crit, Vision

### Drone Chips (20 items)
- Combat, Defense, Support, Utility
- Swarm, Hunter, Stealth, EMP, etc.

### Consumables (15 items)
- Repair Kits (50-400 HP)
- Shield Cells (50-200 Shield)
- Buffs (Damage, Speed, Crit, Resist)
- Utility (Ammo, Energy, Ultimate)

### Materials (10 tiers)
Scrap Metal → Circuits → Alloy Plates → Power Cells → Plasma Core → Nanofibers → Quantum Chips → Dark Matter → Void Crystal → Aether Fragment

---

## Crafting System

### Blueprints (40+ total)
- **Weapons** (20) - Rifles, Cannons, Launchers, Melee
- **Armor** (20) - Full mech part sets

### Crafting Queue
- 3 concurrent crafts maximum
- Real-time countdown
- Instant finish (Cores, cost scales with time)
- Cancel for partial refund

### Example Blueprint
```json
{
  "blueprint_id": "bp_legendary_cannon",
  "display_name": "Plasma Devastator",
  "required_materials": {
    "void_crystal": 10,
    "quantum_chips": 15,
    "plasma_core": 25
  },
  "credit_cost": 10000,
  "crafting_time_seconds": 3600,
  "required_player_level": 30
}
```

---

## Set Bonus System (10 Sets)

### Armor Sets
1. **Juggernaut Bulwark** - Tank (+HP, +Resist, CC Immunity)
2. **Inferno Warlord** - Fire damage
3. **Cryo Guardian** - Ice/Defense
4. **Volt Striker** - Lightning/Speed
5. **Phantom Assassin** - Stealth/Crit
6. **Phoenix Rebirth** ⭐ - Resurrection (Legendary)
7. **Void Walker** 🌟 - Reality manipulation (Exotic)
8. **Sentinel Prime** 🌟 - Ultimate defense (Exotic)
9. **Berserker's Rage** - High risk/reward
10. **Combat Medic** - Healing/Support

### Set Bonuses
- **2-piece**: Moderate stat bonuses
- **4-piece**: Major bonuses + special ability

Example: Juggernaut 2/4: +500 HP, +10% Resist | 4/4: +1000 HP, +20% Resist, CC Immune

---

## Shop System

### Categories
1. **Cosmetics** (Cores only) - Skins, Emotes, Effects, Trails
2. **Convenience** (Cores only) - Inventory Expansion, Boosts, Tokens
3. **Materials** (Credits only) - Material bundles, daily rotation

### Featured Items
- 3 random items daily
- Mix of all categories
- Spotlight display

---

## Loot System

### Drop Tables (20+ enemies, 10+ bosses)
Each enemy has custom loot pools with weighted drops.

### Bad Luck Protection
- Pity timer: 100 kills
- +1% rare chance per kill after threshold
- Resets on successful rare drop

### Global Modifiers
- Luck stat (+1% per point)
- Difficulty scaling
- Clan bonus
- Battle pass boost

---

## Equipment System

### Slots (15 total)
- 1x Head, Torso, Arms, Legs
- 4x Weapons
- 5x Drones
- 2x Accessories

### Loadouts
- 5 preset slots (expandable)
- Save/Load instantly
- Full configuration stored

---

## API Reference

### ItemDatabase
```csharp
ItemBase item = ItemDatabase.GetItem("item_id");
List<ItemBase> heads = ItemDatabase.GetItemsByCategory("head");
List<MechPartItem> parts = ItemDatabase.GetItemsByType<MechPartItem>();
List<ItemBase> legendaries = ItemDatabase.GetItemsByRarity(ItemRarity.Legendary);
```

### InventoryManager
```csharp
bool added = inventory.AddItem(item, 5);
bool removed = inventory.RemoveItem("item_id", 2);
bool has = inventory.HasItem("item_id", 10);
int qty = inventory.GetItemQuantity("item_id");
inventory.SortInventory(SortType.Rarity);
```

### EquipmentManager
```csharp
ItemBase prev = equipment.EquipItem(EquipmentSlot.Head, item);
ItemBase item = equipment.UnequipItem(EquipmentSlot.Head);
Dictionary<StatType, float> stats = equipment.GetTotalStats();
equipment.SaveLoadout(0);
equipment.LoadLoadout(0);
```

### CraftingManager
```csharp
bool started = crafting.TryStartCraft("bp_id", inventory, playerLevel);
bool finished = crafting.InstantFinishCraft("job_id");
List<CraftingJob> jobs = crafting.GetActiveJobs();
```

### ShopManager
```csharp
bool purchased = shop.PurchaseItem("shop_item_id", inventory);
List<ShopItem> cosmetics = shop.GetItemsByCategory(ShopCategory.Cosmetic);
List<string> featured = shop.GetFeaturedItems();
```

### CurrencyManager
```csharp
int credits = CurrencyManager.Credits;
int cores = CurrencyManager.Cores;
CurrencyManager.AddCredits(1000, "reward");
bool spent = CurrencyManager.SpendCredits(500, "purchase");
bool canAfford = CurrencyManager.HasCredits(1000);
```

---

## JSON Data Structure

### Item Definition
```json
{
  "item_id": "head_legendary_aegis",
  "display_name": "Aegis Command Helm",
  "description": "Elite command helmet",
  "rarity": "Legendary",
  "item_level": 35,
  "sell_value": 8000,
  "part_type": "Head",
  "primary_stats": {"HP": 600, "Shield": 300},
  "secondary_stats": {"CritChance": 0.15, "CritDamage": 0.25},
  "special_ability_id": "tactical_overlay",
  "tags": ["equipment", "mech_part", "head"]
}
```

### Set Definition
```json
{
  "set_id": "juggernaut_bulwark",
  "set_name": "Juggernaut Bulwark",
  "required_item_ids": ["head_jugg", "torso_jugg", "arms_jugg", "legs_jugg"],
  "two_piece_bonus": {
    "bonus_name": "Fortified",
    "stat_bonuses": {"HP": 500, "PhysicalResist": 0.10}
  },
  "four_piece_bonus": {
    "bonus_name": "Immovable",
    "special_ability_id": "immovable",
    "stat_bonuses": {"HP": 1000, "AllResist": 0.10}
  }
}
```

---

## File Locations

### Items
- `Data/Items/MechParts/` - heads, torsos, arms, legs
- `Data/Items/WeaponMods/` - barrels, magazines, optics
- `Data/Items/DroneChips/` - ai_chips
- `Data/Items/Consumables/` - consumables
- `Data/Items/Materials/` - materials

### Blueprints
- `Data/Blueprints/weapons.json` - 20 weapon blueprints
- `Data/Blueprints/armor.json` - 20 armor blueprints

### Sets
- `Data/Sets/armor_sets.json` - 10 armor sets

### Shop
- `Data/Shop/cosmetics.json` - 15 cosmetic items
- `Data/Shop/convenience.json` - 10 convenience items
- `Data/Shop/materials.json` - 10 material bundles

### Loot Tables
- `Data/LootTables/enemies/` - 15 enemy types
- `Data/LootTables/bosses/` - 10 boss encounters

---

## Usage Examples

### Craft a Legendary Weapon
```csharp
var bp = craftingManager.GetBlueprint("bp_legendary_cannon");
if (bp.CanCraft(inventory, playerLevel, CurrencyManager.Credits))
{
    craftingManager.TryStartCraft("bp_legendary_cannon", inventory, playerLevel);
}
```

### Equip Full Set
```csharp
var juggHead = ItemDatabase.GetItem("head_juggernaut_epic");
var juggTorso = ItemDatabase.GetItem("torso_juggernaut_epic");
equipmentManager.EquipItem(EquipmentSlot.Head, juggHead);
equipmentManager.EquipItem(EquipmentSlot.Torso, juggTorso);
// ... equip arms and legs
// Check SetManager for active bonuses
```

### Purchase Shop Item
```csharp
var skin = shopManager.GetShopItem("shop_skin_obsidian");
if (CurrencyManager.HasCores(skin.PriceCores))
{
    shopManager.PurchaseItem("shop_skin_obsidian", inventory);
}
```

---

## Troubleshooting

**Items not loading**: Check JSON syntax, verify ItemDatabase autoload  
**Crafting fails**: Verify materials, credits, level, queue space  
**Set bonuses not working**: All items must be equipped, match set_id  
**Shop purchase fails**: Check currency type (Credits vs Cores), level requirement

---

## Performance

- Item lookups: O(1) via Dictionary
- Inventory sort: O(n log n)
- Set calculation: O(k) where k = number of sets
- Loot generation: O(n) where n = pool size

---

**Version**: 2.0.0 | **Last Updated**: December 2024  
**Total Content**: 250+ items, 40+ blueprints, 10 sets, 20+ loot tables
