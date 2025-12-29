# MechDefenseHalo

**Tagline:** A realistic mech tower defense game with Halo UNSC military sci-fi aesthetic

## 📖 Documentation

- [Game Design Document](docs/GDD.md) - Teljes játékvízió
- [Architecture](ARCHITECTURE.md) - Kód architektúra
- [Project Analysis (Hungarian)](PROJECT_ANALYSIS_HU.md) - Részletes projekt elemzés és javítási javaslatok
- [Improvement Guide](IMPROVEMENT_GUIDE.md) - Quick-start guide for improvements

## Game Concept

MechDefenseHalo combines immersive first-person cockpit gameplay with strategic top-down tower defense mechanics. Players experience the thrill of piloting a UNSC-style mech from a first-person cockpit view, then transition to a tactical top-down perspective to strategically defend against waves of enemies.

**Core Gameplay Loop:**
- Start in first-person cockpit view for immersive mech piloting
- Transition to top-down strategic view for tower defense gameplay
- Deploy and command multiple mechs and defensive units
- Survive increasingly challenging enemy waves

## Visual Style

The game embraces the **Halo UNSC realistic military aesthetic**:
- Military grey and green color palette
- Functional, industrial design language
- Worn, battle-tested textures and materials
- Grounded sci-fi technology (no fantasy elements)
- Tactical UI with military HUD elements

## Tech Stack

- **Engine:** Godot 4.x with .NET support
- **Language:** C# (no GDScript)
- **Rendering:** Forward+ (Vulkan)
- **Backend:** Firebase (authentication, leaderboards, cloud saves)
- **Platform:** Android (Google Play Store)
- **Resolution:** 1080x2400 (mobile portrait/landscape support)

## Development Workflow

This project follows the **"Vibe Developer" methodology**:
- AI-assisted development with GitHub Copilot
- PR-based code reviews for quality assurance
- Iterative feature development
- Continuous integration and testing

## Monetization Strategy

**Revenue Streams:**
1. **Rewarded Ads (AdMob):** Players watch ads for in-game rewards
2. **In-App Purchases:**
   - Cosmetic mech skins (visual customization)
   - Minor gameplay boosts (non-pay-to-win)
   - Premium currency for cosmetic unlocks

## Firebase Integration

The game will integrate Firebase for:
- User authentication and profiles
- Cloud save synchronization
- Global leaderboards
- Analytics and crash reporting
- Remote configuration for balance updates

*(Implementation planned for future milestones)*

## Project Status

**Current Phase:** Layer 0 - Foundation
- ✅ Project structure established
- ✅ Core configuration files created
- ✅ Basic scene setup
- ✅ First-person mech controller (PC + Mobile)
- ✅ Virtual joystick and touch controls
- ✅ Mobile UI with HUD elements
- ⏳ Gameplay systems (upcoming)
- ⏳ Firebase integration (upcoming)
- ⏳ Monetization implementation (upcoming)

## 🎮 Controls

### PC (Development/Testing):
- **WASD**: Movement
- **Mouse**: Camera look (captured automatically)
- **Shift**: Sprint
- **ESC**: Release/Capture mouse
- **Click**: Recapture mouse

### HUD Elements:
- **Top Left**: Health (red) + Energy (green) bars
- **Top Right**: Visor info (UNSC MECH-01)
- **Center**: Crosshair
- **FPS Counter**: Top left (performance monitoring)
- **Visor Overlay**: Cyan cockpit frame effect

### Mobilon:
- **Bal oldali joystick**: Mozgás (érintéssel aktiválható)
- **Jobb oldali érintés + húzás**: Kamera forgás
- **Action gombok**: Fire 🔫, Shield 🛡️, Ability ⚡, Weapon Switch 🔄
- **HUD**: Health bar (piros), Energy bar (sárga), Crosshair (középen)

### APK Build:
1. Project → Export → Android
2. Telepítsd a telefonra
3. Tesztelj!

### Jelenlegi funkciók:
- ✅ First-person mech mozgás gravitációval
- ✅ Kamera forgás pitch/yaw limitekkel
- ✅ Platform detektálás (PC vs Mobile)
- ✅ Virtual joystick implementáció
- ✅ Touch camera implementáció
- ✅ Multi-touch támogatás
- ✅ HUD megjelenítés (HP, Energy, Crosshair)

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed technical documentation on the project's modular architecture, design patterns, and folder structure.

## Getting Started

### Prerequisites
- Godot 4.x with .NET support
- .NET SDK 6.0 or higher
- Android SDK (for mobile builds)

### Opening the Project
1. Clone this repository
2. Open Godot 4.x
3. Import project by selecting the `project.godot` file
4. The project will load with C# scripting enabled

## Contributing

This project uses AI-assisted development. All contributions should:
- Follow the established architecture patterns
- Maintain the UNSC military aesthetic
- Use C# (not GDScript)
- Include appropriate documentation

## License

*(License to be determined)*
