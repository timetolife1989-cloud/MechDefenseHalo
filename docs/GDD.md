# MechDefenseHalo - Game Design Document

## JÁTÉK KONCEPCIÓ

**MechDefenseHalo** - First-person mech combat + drone-based wave defense

### Alap gameplay:
- A játékos egy **humanoid mech**-et irányít **first-person nézetből**
- Hullámokban jönnek az ellenségek
- **Drónokat** küld ki védekezésre (NEM tornyokat rak le!)
- Saját fegyverekkel is harcol (lőfegyverek + közelharci: kard, kalapács)
- **Boss fight:** First-person aktív célzás, gyengepontok (térd, fej, hát), elemi ellenállások

---

## MECH RENDSZER

### Mech osztályok (PvP/PvM-hez):

| Osztály | Jellemző | Képességek |
|---------|----------|------------|
| **Tank** | Lassú, nehéz páncél | Aggro vonzás, nagy HP, erős pajzs |
| **DPS** | Gyors, gyenge védelem | Nagy sebzés, magas mobilitás |
| **Support** | Közepes stats | Pajzs másokra, javító drónok küldése |

### Testreszabás:
- Skinek (teljes mech skin)
- Színek (egyedi színezés)
- Matricák/decals
- Fegyver skinek

---

## FEGYVERRENDSZER

### Lőfegyverek:

| Fegyver | Típus | Elem |
|---------|-------|------|
| Assault Rifle | Gyors tűzgyorsaság, közepes DMG | Fizikai |
| Plasma Cannon | Lassú, nagy DMG | Tűz |
| Cryo Launcher | Közepes, lassítás effekt | Jég |
| Tesla Coil | Láncvillám több ellenségre | Elektromos |
| Toxic Sprayer | DOT sebzés | Mérgező |

### Közelharci fegyverek:

| Fegyver | Jellemző |
|---------|----------|
| Energy Sword | Gyors ütések, közepes DMG |
| War Hammer | Lassú, hatalmas DMG, AOE |
| Shield Bash | Pajzzsal üt + blokkolás |

---

## DRÓN RENDSZER (Tornyok helyett)

| Drón típus | Funkció | Unlock szint |
|------------|---------|--------------|
| **Attack Drone** | Automatikusan lő az ellenségre | Alap |
| **Shield Drone** | Pajzsot tart a mech-nek vagy társnak | Lvl 5 |
| **Repair Drone** | Gyógyítja a mech-et vagy társat | Lvl 8 |
| **EMP Drone** | Lassítja/bénítja az ellenségeket | Lvl 12 |
| **Bomber Drone** | AOE robbanó támadás | Lvl 15 |
| **Sniper Drone** | Nagy DMG, lassú, nagy range | Lvl 20 |

### Drón mechanika:
- Korlátozott aktív drón szám (pl. max 5)
- Energia rendszer (újratöltődik idővel)
- Drónok fejleszthetőek (DMG, HP, range)
- Drón skinek (cosmetic)

---

## BOSS RENDSZER

### Boss fight mechanika:
- **First-person nézet** marad (aktív célzás kell!)
- **Gyengepontok:** térd, hát, fej, energia mag - extra DMG ha eltalálod
- **Elemi ellenállások:** pl. "Tűzre immun, jégre 2x sebzés"
- **Fázisok:** Boss viselkedése változik HP alapján
- **Rage mode:** alacsony HP-nál agresszívebb

### Példa boss:

```
🤖 FROST TITAN
- HP: 50,000
- Immunitás: Jég
- Gyengeség: Tűz (2x DMG)
- Gyengepontok: Térdek (törés = lassulás), Hát (energia mag = kritikus DMG)
- Fázis 1 (100-50% HP): Lassú, erős ütések
- Fázis 2 (50-25% HP): Jég tornádó AOE
- Fázis 3 (25-0% HP): Rage - gyors támadások, fagyasztó aura
```

---

## HUB ("Drón Szentély")

**NEM klasszikus hangár!** A mech egy futurisztikus térben áll, drónok keringenek körülötte.

### Vizuális koncepció:

```
┌─────────────────────────────────────────┐
│                                         │
│   🤖 ← Mech (te) állsz középen          │
│                                         │
│  ◇ ◇ ◇   Drónok körülötted lebegnek  ◇ ◇│
│                                         │
│ ╔═══════╗  ╔═══════╗  ╔═══════╗         │
│ ║ STATS ║  ║MONSTER║  ║ ARMORY║ ← Holografikus
│ ║ PANEL ║  ║ BOOK  ║  ║       ║   lebegő panelek
│ ╚═══════╝  ╚═══════╝  ╚═══════╝         │
└─────────────────────────────────────────┘
```

### Hub panelek (holografikus, lebegő):

| Panel | Funkció |
|-------|---------|
| **Stats Panel** | Statisztikák: ölések, win rate, játékidő |
| **Monster Book** | Legyőzött ellenségek enciklopédiája, gyengeségek |
| **Armory** | Fegyverváltás, fegyver fejlesztés |
| **Drone Bay** | Drón loadout, drón fejlesztés |
| **Customization** | Skinek, színek, matricák |
| **Mission Select** | Küldetésválasztás, nehézségi szint |
| **Shop** | Cosmetic vásárlás (IAP) |

### Hub vizuális stílus:
- Holografikus kék/cyan panelek
- Sötét háttér (űr vagy sötét tech terem)
- Drónok lassan keringenek a mech körül
- Neon vonalak, futurisztikus UI
- A mech körül lebegő AI interfészek

---

## MULTIPLAYER (Későbbi fázis)

### PvM (Co-op Wave Defense):
- 2-4 játékos együtt védekezik
- Osztályok kombinációja fontos (Tank + Support + DPS)
- Shared drón pool vagy egyéni drónok
- Boss-ok skálázódnak játékosszámra

### PvP (Arena):
- 1v1 Duel
- 3v3 Team Fight
- Célzás + drón mikro menedzsment
- Ranked rendszer + Leaderboard

---

## MONETIZÁCIÓ

| Típus | Mit kap a játékos | P2W? |
|-------|-------------------|------|
| **Rewarded Ads** | Extra energia, bonus loot | ❌ |
| **Cosmetic IAP** | Skinek, színek, matricák, drón skinek | ❌ |
| **Battle Pass** | Szezonális jutalmak (90% cosmetic) | ❌ |
| **Minor Boosts** | XP boost, energy refill | ⚠️ Enyhe |

**NINCS pay-to-win fegyver vagy stat boost!**

---

## VIZUÁLIS STÍLUS

### Mech dizájn referencia:
- Humanoid, ~3 méter magas
- Szürke/fehér alap + fekete ízületek + sárga akcentusok
- Zárt sisak (Halo ODST stílus)
- Látható hidraulika, csövek, részletek
- UNSC / katonai / realisztikus sci-fi

### Hangulat:
- Halo UNSC aesthetic
- Titanfall mech feeling
- Sötét, katonai, funkcionális
- Nem anime, nem túl színes

---

## TECH STACK

- **Engine:** Godot 4.x .NET (C#)
- **Platform:** Android (Google Play) elsődleges, később iOS
- **Backend:** Firebase (Auth, Firestore, Analytics)
- **Ads:** AdMob (rewarded ads)
- **IAP:** Google Play Billing

---

## FEJLESZTÉSI FÁZISOK

| Fázis | Leírás | Státusz |
|-------|--------|---------|
| 1 | Projekt struktúra, GDD | ✅ KÉSZ |
| 2 | Mech mozgás (FPS kamera, WASD) | 🔄 KÖVETKEZŐ |
| 3 | Placeholder mech modell | ⏳ |
| 4 | Egy attack drón | ⏳ |
| 5 | Egy alap ellenség | ⏳ |
| 6 | Wave spawning | ⏳ |
| 7 | Fegyver rendszer (1 lőfegyver) | ⏳ |
| 8 | Boss prototípus | ⏳ |
| 9 | Hub UI prototípus | ⏳ |
| 10 | Teljes gameplay loop | ⏳ |

---

## Kapcsolódó dokumentáció

- [Architecture](../ARCHITECTURE.md) - Kód architektúra és design patternek
- [README](../README.md) - Projekt overview és quick start
