using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.Audio
{
    /// <summary>
    /// Central registry of all game sounds.
    /// 
    /// USAGE:
    /// string soundPath = SoundLibrary.GetSound(SoundID.WeaponAssaultRifleFire);
    /// AudioManager.Instance.PlaySFX(soundPath, position);
    /// </summary>
    public static class SoundLibrary
    {
        private static Dictionary<SoundID, string> _sounds = new();

        static SoundLibrary()
        {
            RegisterAllSounds();
        }

        private static void RegisterAllSounds()
        {
            // WEAPON SOUNDS
            Register(SoundID.WeaponAssaultRifleFire, "res://Assets/Audio/SFX/Weapons/assault_rifle_fire.ogg");
            Register(SoundID.WeaponAssaultRifleReload, "res://Assets/Audio/SFX/Weapons/assault_rifle_reload.ogg");
            Register(SoundID.WeaponPlasmaFire, "res://Assets/Audio/SFX/Weapons/plasma_cannon_fire.ogg");
            Register(SoundID.WeaponCryoFire, "res://Assets/Audio/SFX/Weapons/cryo_launcher_fire.ogg");
            Register(SoundID.WeaponTeslaFire, "res://Assets/Audio/SFX/Weapons/tesla_coil_fire.ogg");
            Register(SoundID.WeaponSwordSwing, "res://Assets/Audio/SFX/Weapons/energy_sword_swing.ogg");
            Register(SoundID.WeaponHammerSwing, "res://Assets/Audio/SFX/Weapons/war_hammer_swing.ogg");

            // IMPACT SOUNDS
            Register(SoundID.ImpactMetal, "res://Assets/Audio/SFX/Impacts/impact_metal.ogg");
            Register(SoundID.ImpactFlesh, "res://Assets/Audio/SFX/Impacts/impact_flesh.ogg");
            Register(SoundID.ImpactEnergy, "res://Assets/Audio/SFX/Impacts/impact_energy.ogg");

            // EXPLOSION SOUNDS
            Register(SoundID.ExplosionSmall, "res://Assets/Audio/SFX/Explosions/explosion_small.ogg");
            Register(SoundID.ExplosionMedium, "res://Assets/Audio/SFX/Explosions/explosion_medium.ogg");
            Register(SoundID.ExplosionLarge, "res://Assets/Audio/SFX/Explosions/explosion_large.ogg");

            // UI SOUNDS
            Register(SoundID.UIButtonClick, "res://Assets/Audio/SFX/UI/button_click.ogg");
            Register(SoundID.UIButtonHover, "res://Assets/Audio/SFX/UI/button_hover.ogg");
            Register(SoundID.UIItemPickup, "res://Assets/Audio/SFX/UI/item_pickup.ogg");
            Register(SoundID.UIItemEquip, "res://Assets/Audio/SFX/UI/item_equip.ogg");
            Register(SoundID.UICraftComplete, "res://Assets/Audio/SFX/UI/craft_complete.ogg");
            Register(SoundID.UIPurchase, "res://Assets/Audio/SFX/UI/purchase.ogg");

            // ENEMY SOUNDS
            Register(SoundID.EnemyGruntAttack, "res://Assets/Audio/SFX/Enemies/grunt_attack.ogg");
            Register(SoundID.EnemyShooterFire, "res://Assets/Audio/SFX/Enemies/shooter_fire.ogg");
            Register(SoundID.EnemyDeath, "res://Assets/Audio/SFX/Enemies/death.ogg");

            // DRONE SOUNDS
            Register(SoundID.DroneDeploy, "res://Assets/Audio/SFX/Drones/deploy.ogg");
            Register(SoundID.DroneAttackFire, "res://Assets/Audio/SFX/Drones/attack_fire.ogg");
            Register(SoundID.DroneShieldActivate, "res://Assets/Audio/SFX/Drones/shield_activate.ogg");
            Register(SoundID.DroneRepairLoop, "res://Assets/Audio/SFX/Drones/repair_loop.ogg");

            // MUSIC
            Register(SoundID.MusicMainMenu, "res://Assets/Audio/Music/main_menu.ogg");
            Register(SoundID.MusicCombat, "res://Assets/Audio/Music/combat.ogg");
            Register(SoundID.MusicBoss, "res://Assets/Audio/Music/boss_fight.ogg");
            Register(SoundID.MusicVictory, "res://Assets/Audio/Music/victory.ogg");
        }

        private static void Register(SoundID id, string path)
        {
            _sounds[id] = path;
        }

        public static string GetSound(SoundID id)
        {
            if (_sounds.TryGetValue(id, out string path))
                return path;
            
            GD.PrintErr($"Sound not found: {id}");
            return string.Empty;
        }

        public static bool HasSound(SoundID id) => _sounds.ContainsKey(id);
    }

    /// <summary>
    /// Enum of all sound IDs for type-safe access
    /// </summary>
    public enum SoundID
    {
        // Weapons
        WeaponAssaultRifleFire,
        WeaponAssaultRifleReload,
        WeaponPlasmaFire,
        WeaponCryoFire,
        WeaponTeslaFire,
        WeaponSwordSwing,
        WeaponHammerSwing,

        // Impacts
        ImpactMetal,
        ImpactFlesh,
        ImpactEnergy,

        // Explosions
        ExplosionSmall,
        ExplosionMedium,
        ExplosionLarge,

        // UI
        UIButtonClick,
        UIButtonHover,
        UIItemPickup,
        UIItemEquip,
        UICraftComplete,
        UIPurchase,

        // Enemies
        EnemyGruntAttack,
        EnemyShooterFire,
        EnemyDeath,

        // Drones
        DroneDeploy,
        DroneAttackFire,
        DroneShieldActivate,
        DroneRepairLoop,

        // Music
        MusicMainMenu,
        MusicCombat,
        MusicBoss,
        MusicVictory
    }
}
