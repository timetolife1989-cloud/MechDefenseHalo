using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.SaveSystem
{
    /// <summary>
    /// Main save data container
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public string Version { get; set; } = "1.0.0";
        public DateTime LastSaved { get; set; }
        public float TotalPlaytime { get; set; }
        
        // Player Progress
        public PlayerSaveData Player { get; set; }
        public InventorySaveData Inventory { get; set; }
        public EquipmentSaveData Equipment { get; set; }
        public CurrencySaveData Currency { get; set; }
        
        // Game State
        public int CurrentWave { get; set; }
        public List<string> UnlockedContent { get; set; }
        public List<string> CompletedAchievements { get; set; }
        
        // Statistics
        public StatisticsSaveData Statistics { get; set; }
        
        // Settings
        public SettingsSaveData Settings { get; set; }
    }

    /// <summary>
    /// Player progression data
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public int Level { get; set; }
        public int CurrentXP { get; set; }
        public int PrestigeLevel { get; set; }
        public float LastPositionX { get; set; }
        public float LastPositionY { get; set; }
        public float LastPositionZ { get; set; }
        public float MaxHP { get; set; }
        public float CurrentHP { get; set; }
    }

    /// <summary>
    /// Inventory save data
    /// </summary>
    [Serializable]
    public class InventorySaveData
    {
        public int MaxSlots { get; set; }
        public List<ItemSaveData> Items { get; set; }
    }

    /// <summary>
    /// Individual item save data
    /// </summary>
    [Serializable]
    public class ItemSaveData
    {
        public string ItemID { get; set; }
        public int Quantity { get; set; }
        public int SlotIndex { get; set; }
        public Dictionary<string, float> Stats { get; set; }
    }

    /// <summary>
    /// Equipment save data
    /// </summary>
    [Serializable]
    public class EquipmentSaveData
    {
        public Dictionary<string, string> EquippedItems { get; set; } // slot -> itemID
        public int CurrentLoadoutID { get; set; }
        public Dictionary<int, Dictionary<string, string>> Loadouts { get; set; }
    }

    /// <summary>
    /// Currency save data
    /// </summary>
    [Serializable]
    public class CurrencySaveData
    {
        public int Credits { get; set; }
        public int Cores { get; set; }
    }

    /// <summary>
    /// Statistics save data
    /// </summary>
    [Serializable]
    public class StatisticsSaveData
    {
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public int WavesCompleted { get; set; }
        public int BossesDefeated { get; set; }
        public float DamageDealt { get; set; }
        public float DamageTaken { get; set; }
    }

    /// <summary>
    /// Settings save data
    /// </summary>
    [Serializable]
    public class SettingsSaveData
    {
        public float MasterVolume { get; set; }
        public float MusicVolume { get; set; }
        public float SFXVolume { get; set; }
        public bool FullScreen { get; set; }
        public int ResolutionWidth { get; set; }
        public int ResolutionHeight { get; set; }
    }
}
