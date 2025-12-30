using Godot;
using System;
using System.Text.Json;
using System.Collections.Generic;
using MechDefenseHalo.Notifications;

namespace MechDefenseHalo.Core
{
    /// <summary>
    /// Manages game save/load functionality.
    /// Handles player data persistence with encryption and backup support.
    /// </summary>
    public partial class SaveManager : Node
    {
        #region Singleton

        private static SaveManager _instance;

        public static SaveManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("SaveManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Constants

        private const string SAVE_FILE_NAME = "player_save.dat";
        private const string BACKUP_FILE_NAME = "player_save_backup.dat";

        #endregion

        #region Public Properties

        public PlayerData CurrentPlayerData { get; private set; }
        public SaveSystem.SaveData CurrentSaveData { get; private set; }

        #endregion

        #region Private Fields

        private LocalSaveHandler _localHandler;
        private AutoSaveController _autoSave;
        private float _sessionStartTime;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple SaveManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            _sessionStartTime = Time.GetTicksMsec() / 1000f;

            // Initialize save handlers
            _localHandler = new LocalSaveHandler();
            
            // Get or create AutoSaveController
            _autoSave = GetNodeOrNull<AutoSaveController>("AutoSaveController");
            if (_autoSave == null)
            {
                _autoSave = new AutoSaveController();
                _autoSave.Name = "AutoSaveController";
                AddChild(_autoSave);
            }

            // Load existing save or create new
            LoadGame();

            GD.Print("SaveManager initialized with comprehensive save system");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                // Auto-save on exit
                SaveGame();
                _instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Save current game data with encryption and backup
        /// </summary>
        public bool SaveGame()
        {
            try
            {
                // Collect save data from all systems
                CurrentSaveData = CollectSaveData();
                CurrentSaveData.LastSaved = DateTime.Now;

                // Update total playtime
                UpdateTotalPlaytime();

                // Serialize to JSON
                string json = JsonSerializer.Serialize(CurrentSaveData, new JsonSerializerOptions { WriteIndented = true });

                // Create backup before saving
                CreateBackup();

                // Encrypt and save
                string encrypted = SaveEncryption.Encrypt(json);

                if (!_localHandler.WriteSave(SAVE_FILE_NAME, encrypted))
                {
                    GD.PrintErr("Failed to write save file");
                    return false;
                }

                // Emit save event
                EventBus.Emit(EventBus.GameSaved, null);
                GD.Print($"Game saved at {CurrentSaveData.LastSaved}");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to save game: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Load game data with decryption and migration support
        /// </summary>
        public bool LoadGame()
        {
            if (!_localHandler.SaveExists(SAVE_FILE_NAME))
            {
                GD.Print("No save file found, creating new save");
                CreateNewSave();
                return true;
            }

            try
            {
                string encrypted = _localHandler.ReadSave(SAVE_FILE_NAME);
                if (string.IsNullOrEmpty(encrypted))
                {
                    GD.PrintErr("Save file is empty");
                    return TryLoadBackup();
                }

                string json = SaveEncryption.Decrypt(encrypted);
                CurrentSaveData = JsonSerializer.Deserialize<SaveSystem.SaveData>(json);

                if (CurrentSaveData == null)
                {
                    GD.PrintErr("Failed to deserialize save data");
                    return TryLoadBackup();
                }

                // Migrate if needed
                if (!SaveMigration.IsVersionCompatible(CurrentSaveData.Version))
                {
                    GD.PrintErr($"Save version {CurrentSaveData.Version} is not compatible");
                    return TryLoadBackup();
                }

                CurrentSaveData = SaveMigration.MigrateToCurrentVersion(CurrentSaveData);

                // Apply save data to all systems
                ApplySaveData(CurrentSaveData);

                // Reset session timer
                _sessionStartTime = Time.GetTicksMsec() / 1000f;

                EventBus.Emit(EventBus.GameLoaded, null);
                GD.Print($"Game loaded successfully (version: {CurrentSaveData.Version})");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load game: {e.Message}\n{e.StackTrace}");
                return TryLoadBackup();
            }
        }

        /// <summary>
        /// Delete save file and backup
        /// </summary>
        public void DeleteSave()
        {
            _localHandler.DeleteSave(SAVE_FILE_NAME);
            _localHandler.DeleteSave(BACKUP_FILE_NAME);
            CreateNewSave();
            GD.Print("Save files deleted");
        }

        #region Private Methods

        /// <summary>
        /// Create a new save with default values
        /// </summary>
        private void CreateNewSave()
        {
            CurrentSaveData = new SaveSystem.SaveData
            {
                Version = "1.0.0",
                LastSaved = DateTime.Now,
                TotalPlaytime = 0f,
                Player = new PlayerSaveData
                {
                    Level = 1,
                    CurrentXP = 0,
                    PrestigeLevel = 0,
                    MaxHP = 1000f,
                    CurrentHP = 1000f
                },
                Inventory = new InventorySaveData
                {
                    MaxSlots = 500,
                    Items = new List<ItemSaveData>()
                },
                Equipment = new EquipmentSaveData
                {
                    EquippedItems = new Dictionary<string, string>(),
                    CurrentLoadoutID = 0,
                    Loadouts = new Dictionary<int, Dictionary<string, string>>()
                },
                Currency = new CurrencySaveData
                {
                    Credits = 0,
                    Cores = 0
                },
                CurrentWave = 0,
                UnlockedContent = new List<string>(),
                CompletedAchievements = new List<string>(),
                Statistics = new StatisticsSaveData
                {
                    TotalKills = 0,
                    TotalDeaths = 0,
                    WavesCompleted = 0,
                    BossesDefeated = 0,
                    DamageDealt = 0f,
                    DamageTaken = 0f
                },
                Settings = new SettingsSaveData
                {
                    MasterVolume = 1f,
                    MusicVolume = 0.7f,
                    SFXVolume = 1f,
                    FullScreen = true,
                    ResolutionWidth = 1920,
                    ResolutionHeight = 1080
                }
            };

            // Initialize legacy player data for backwards compatibility
            CurrentPlayerData = new PlayerData();
            
            _sessionStartTime = Time.GetTicksMsec() / 1000f;
            GD.Print("New save created");
        }

        /// <summary>
        /// Update total playtime with current session time
        /// </summary>
        private void UpdateTotalPlaytime()
        {
            float currentTime = Time.GetTicksMsec() / 1000f;
            CurrentSaveData.TotalPlaytime += currentTime - _sessionStartTime;
            _sessionStartTime = currentTime;
        }

        /// <summary>
        /// Create backup of current save file
        /// </summary>
        private void CreateBackup()
        {
            if (_localHandler.SaveExists(SAVE_FILE_NAME))
            {
                _localHandler.CopySave(SAVE_FILE_NAME, BACKUP_FILE_NAME);
            }
        }

        /// <summary>
        /// Try to load backup save if main save fails
        /// </summary>
        private bool TryLoadBackup()
        {
            if (!_localHandler.SaveExists(BACKUP_FILE_NAME))
            {
                GD.Print("No backup save found, creating new save");
                CreateNewSave();
                return false;
            }

            GD.Print("Attempting to load backup save...");

            try
            {
                string encrypted = _localHandler.ReadSave(BACKUP_FILE_NAME);
                string json = SaveEncryption.Decrypt(encrypted);
                CurrentSaveData = JsonSerializer.Deserialize<SaveSystem.SaveData>(json);

                if (CurrentSaveData != null)
                {
                    ApplySaveData(CurrentSaveData);
                    GD.Print("Backup save loaded successfully");
                    
                    // Restore the backup as the main save
                    SaveGame();
                    return true;
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load backup save: {e.Message}");
            }

            CreateNewSave();
            return false;
        }

        /// <summary>
        /// Collect save data from all game systems
        /// </summary>
        private SaveSystem.SaveData CollectSaveData()
        {
            var saveData = new SaveSystem.SaveData
            {
                Version = "1.0.0",
                Player = CollectPlayerData(),
                Inventory = CollectInventoryData(),
                Equipment = CollectEquipmentData(),
                Currency = CollectCurrencyData(),
                CurrentWave = CollectWaveData(),
                UnlockedContent = CollectUnlockedContent(),
                CompletedAchievements = CollectCompletedAchievements(),
                Statistics = CollectStatisticsData(),
                Settings = CollectSettingsData()
            };

            return saveData;
        }

        private PlayerSaveData CollectPlayerData()
        {
            // For now, return default values
            // TODO: Integrate with actual player systems when they exist
            return CurrentSaveData?.Player ?? new PlayerSaveData
            {
                Level = 1,
                CurrentXP = 0,
                PrestigeLevel = 0,
                MaxHP = 1000f,
                CurrentHP = 1000f
            };
        }

        private InventorySaveData CollectInventoryData()
        {
            var inventoryManager = InventoryManager.Instance;
            if (inventoryManager != null)
            {
                return inventoryManager.GetSaveData();
            }

            return CurrentSaveData?.Inventory ?? new InventorySaveData
            {
                MaxSlots = 500,
                Items = new List<ItemSaveData>()
            };
        }

        private EquipmentSaveData CollectEquipmentData()
        {
            var equipmentManager = EquipmentManager.Instance;
            if (equipmentManager != null)
            {
                return equipmentManager.GetSaveData();
            }

            return CurrentSaveData?.Equipment ?? new EquipmentSaveData
            {
                EquippedItems = new Dictionary<string, string>(),
                CurrentLoadoutID = 0,
                Loadouts = new Dictionary<int, Dictionary<string, string>>()
            };
        }

        private CurrencySaveData CollectCurrencyData()
        {
            var currencyManager = CurrencyManager.Instance;
            if (currencyManager != null)
            {
                return currencyManager.GetSaveData();
            }

            return CurrentSaveData?.Currency ?? new CurrencySaveData
            {
                Credits = 0,
                Cores = 0
            };
        }

        private int CollectWaveData()
        {
            // TODO: Get from WaveManager when available
            return CurrentSaveData?.CurrentWave ?? 0;
        }

        private List<string> CollectUnlockedContent()
        {
            // TODO: Get from UnlockManager when available
            return CurrentSaveData?.UnlockedContent ?? new List<string>();
        }

        private List<string> CollectCompletedAchievements()
        {
            // TODO: Get from AchievementManager when available
            return CurrentSaveData?.CompletedAchievements ?? new List<string>();
        }

        private StatisticsSaveData CollectStatisticsData()
        {
            // TODO: Get from StatisticsManager when available
            return CurrentSaveData?.Statistics ?? new StatisticsSaveData
            {
                TotalKills = 0,
                TotalDeaths = 0,
                WavesCompleted = 0,
                BossesDefeated = 0,
                DamageDealt = 0f,
                DamageTaken = 0f
            };
        }

        private SettingsSaveData CollectSettingsData()
        {
            // TODO: Get from SettingsManager when available
            return CurrentSaveData?.Settings ?? new SettingsSaveData
            {
                MasterVolume = 1f,
                MusicVolume = 0.7f,
                SFXVolume = 1f,
                FullScreen = true,
                ResolutionWidth = 1920,
                ResolutionHeight = 1080
            };
        }

        /// <summary>
        /// Apply loaded save data to all game systems
        /// </summary>
        private void ApplySaveData(SaveSystem.SaveData data)
        {
            if (data == null)
            {
                GD.PrintErr("Cannot apply null save data");
                return;
            }

            // Apply player data
            if (data.Player != null)
            {
                // TODO: Apply to PlayerLevel, PlayerStats, etc. when available
            }

            // Apply inventory data
            if (data.Inventory != null)
            {
                var inventoryManager = InventoryManager.Instance;
                if (inventoryManager != null)
                {
                    inventoryManager.LoadFromSave(data.Inventory);
                }
            }

            // Apply equipment data
            if (data.Equipment != null)
            {
                var equipmentManager = EquipmentManager.Instance;
                if (equipmentManager != null)
                {
                    equipmentManager.LoadFromSave(data.Equipment);
                }
            }

            // Apply currency data
            if (data.Currency != null)
            {
                var currencyManager = CurrencyManager.Instance;
                if (currencyManager != null)
                {
                    CurrencyManager.LoadFromSave(data.Currency);
                }
            }

            // Apply game state
            // TODO: Apply CurrentWave, UnlockedContent, CompletedAchievements when managers available

            // Apply statistics
            // TODO: Apply to StatisticsManager when available

            // Apply settings
            // TODO: Apply to SettingsManager when available

            // Update legacy player data for backwards compatibility
            UpdateLegacyPlayerData(data);

            GD.Print("Save data applied to all systems");
        }

        /// <summary>
        /// Update legacy PlayerData for backwards compatibility
        /// </summary>
        private void UpdateLegacyPlayerData(SaveSystem.SaveData data)
        {
            CurrentPlayerData = new PlayerData
            {
                Level = data.Player?.Level ?? 1,
                Experience = data.Player?.CurrentXP ?? 0,
                Currency = data.Currency?.Credits ?? 0,
                TotalKills = data.Statistics?.TotalKills ?? 0,
                TotalDeaths = data.Statistics?.TotalDeaths ?? 0,
                TotalPlayTime = data.TotalPlaytime,
                WavesCompleted = data.Statistics?.WavesCompleted ?? 0,
                BossesDefeated = data.Statistics?.BossesDefeated ?? 0,
                UnlockedWeapons = new List<string>(),
                UnlockedDrones = new List<string>(),
                UnlockedSkins = new List<string>(),
                EquippedWeapons = new List<string>(),
                EquippedDrones = new List<string>(),
                EquippedSkin = "default",
                MasterVolume = data.Settings?.MasterVolume ?? 1f,
                MusicVolume = data.Settings?.MusicVolume ?? 0.7f,
                SFXVolume = data.Settings?.SFXVolume ?? 1f
            };
        }
        
        /// <summary>
        /// Get a boolean value from player data
        /// </summary>
        /// <param name="key">Key to retrieve</param>
        /// <returns>Boolean value</returns>
        public static bool GetBool(string key)
        {
            if (Instance?.CurrentPlayerData == null) return false;
            
            return key switch
            {
                "tutorial_completed" => Instance.CurrentPlayerData.TutorialCompleted,
                "is_first_launch" => Instance.CurrentPlayerData.IsFirstLaunch,
                _ => false
            };
        }
        
        /// <summary>
        /// Set a boolean value in player data
        /// </summary>
        /// <param name="key">Key to set</param>
        /// <param name="value">Value to set</param>
        public static void SetBool(string key, bool value)
        {
            if (Instance?.CurrentPlayerData == null) return;
            
            switch (key)
            {
                case "tutorial_completed":
                    Instance.CurrentPlayerData.TutorialCompleted = value;
                    break;
                case "is_first_launch":
                    Instance.CurrentPlayerData.IsFirstLaunch = value;
                    break;
            }
            
            Instance.SaveGame();
        }

        #endregion

        #region Daily Missions Methods

        /// <summary>
        /// Get daily missions from save data
        /// </summary>
        public static List<Mission> GetDailyMissions()
        {
            return Instance?.CurrentPlayerData?.DailyMissions ?? new List<Mission>();
        }

        /// <summary>
        /// Set daily missions in save data
        /// </summary>
        public static void SetDailyMissions(List<Mission> missions)
        {
            if (Instance?.CurrentPlayerData != null)
            {
                Instance.CurrentPlayerData.DailyMissions = missions;
                Instance.SaveGame();
            }
        }

        /// <summary>
        /// Get a DateTime value from storage
        /// </summary>
        public static DateTime GetDateTime(string key)
        {
            if (Instance?.CurrentPlayerData?.DateTimeStore != null && 
                Instance.CurrentPlayerData.DateTimeStore.ContainsKey(key))
            {
                if (DateTime.TryParse(Instance.CurrentPlayerData.DateTimeStore[key], out DateTime result))
                {
                    return result;
                }
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// Set a DateTime value in storage
        /// </summary>
        public static void SetDateTime(string key, DateTime value)
        {
            if (Instance?.CurrentPlayerData?.DateTimeStore != null)
            {
                Instance.CurrentPlayerData.DateTimeStore[key] = value.ToString("o");
                Instance.SaveGame();
            }
        }

        #endregion

        #region Generic Dictionary Storage Methods

        /// <summary>
        /// Get a dictionary from player data storage
        /// </summary>
        public static Dictionary<string, Godot.Collections.Dictionary> GetDict(string key)
        {
            if (Instance?.CurrentPlayerData == null) 
                return new Dictionary<string, Godot.Collections.Dictionary>();

            // Initialize DictStore if it's null
            if (Instance.CurrentPlayerData.DictStore == null)
            {
                Instance.CurrentPlayerData.DictStore = new Dictionary<string, Dictionary<string, object>>();
                return new Dictionary<string, Godot.Collections.Dictionary>();
            }
            
            if (!Instance.CurrentPlayerData.DictStore.ContainsKey(key))
                return new Dictionary<string, Godot.Collections.Dictionary>();

            var storedData = Instance.CurrentPlayerData.DictStore[key];
            var result = new Dictionary<string, Godot.Collections.Dictionary>();
            
            foreach (var kvp in storedData)
            {
                var godotDict = new Godot.Collections.Dictionary();
                if (kvp.Value is Dictionary<string, object> innerDict)
                {
                    foreach (var innerKvp in innerDict)
                    {
                        godotDict[innerKvp.Key] = innerKvp.Value;
                    }
                }
                result[kvp.Key] = godotDict;
            }
            
            return result;
        }

        /// <summary>
        /// Set a dictionary in player data storage
        /// </summary>
        public static void SetDict(string key, Dictionary<string, Godot.Collections.Dictionary> value)
        {
            if (Instance?.CurrentPlayerData == null) return;

            // Initialize DictStore if it's null
            if (Instance.CurrentPlayerData.DictStore == null)
            {
                Instance.CurrentPlayerData.DictStore = new Dictionary<string, Dictionary<string, object>>();
            }

            var storeData = new Dictionary<string, object>();
            foreach (var kvp in value)
            {
                var innerDict = new Dictionary<string, object>();
                foreach (var innerKvp in kvp.Value)
                {
                    innerDict[innerKvp.Key.ToString()] = innerKvp.Value;
                }
                storeData[kvp.Key] = innerDict;
            }

            Instance.CurrentPlayerData.DictStore[key] = storeData;
            Instance.SaveGame();
        #region Generic Key-Value Methods

        /// <summary>
        /// Get a string value from storage
        /// </summary>
        public static string GetString(string key, string defaultValue = "")
        {
            if (Instance?.CurrentPlayerData?.KeyValueStore != null && 
                Instance.CurrentPlayerData.KeyValueStore.ContainsKey(key))
            {
                return Instance.CurrentPlayerData.KeyValueStore[key];
            }
            return defaultValue;
        }

        /// <summary>
        /// Set a string value in storage
        /// </summary>
        public static void SetValue(string key, string value)
        {
            if (Instance?.CurrentPlayerData != null)
            {
                if (Instance.CurrentPlayerData.KeyValueStore == null)
                {
                    Instance.CurrentPlayerData.KeyValueStore = new Dictionary<string, string>();
                }
                Instance.CurrentPlayerData.KeyValueStore[key] = value;
                Instance.SaveGame();
            }
        }

        #endregion
    }

    #region Legacy Data Structures (for backwards compatibility)

    /// <summary>
    /// Container for save file data (LEGACY - kept for backwards compatibility)
    /// </summary>
    public class SaveData
    {
        public int Version { get; set; }
        public string SaveTime { get; set; }
        public PlayerData PlayerData { get; set; }
    }

    /// <summary>
    /// Player progression and statistics data (LEGACY - kept for backwards compatibility)
    /// </summary>
    public class PlayerData
    {
        // Progression
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public int Currency { get; set; } = 0;

        // Statistics
        public int TotalKills { get; set; } = 0;
        public int TotalDeaths { get; set; } = 0;
        public float TotalPlayTime { get; set; } = 0f;
        public int WavesCompleted { get; set; } = 0;
        public int BossesDefeated { get; set; } = 0;

        // Unlocks
        public List<string> UnlockedWeapons { get; set; } = new List<string>();
        public List<string> UnlockedDrones { get; set; } = new List<string>();
        public List<string> UnlockedSkins { get; set; } = new List<string>();

        // Loadout
        public List<string> EquippedWeapons { get; set; } = new List<string>();
        public List<string> EquippedDrones { get; set; } = new List<string>();
        public string EquippedSkin { get; set; } = "default";

        // Settings
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 0.7f;
        public float SFXVolume { get; set; } = 1f;

        // Achievements
        public Dictionary<string, MechDefenseHalo.Achievements.AchievementProgress> AchievementProgress { get; set; } = new Dictionary<string, MechDefenseHalo.Achievements.AchievementProgress>();
        
        // Tutorial
        public bool TutorialCompleted { get; set; } = false;
        public bool IsFirstLaunch { get; set; } = true;
        public int LastCompletedTutorialStep { get; set; } = 0;

        // Daily Missions
        public List<Mission> DailyMissions { get; set; } = new List<Mission>();

        // Generic storage for dictionary data
        public Dictionary<string, Dictionary<string, object>> DictStore { get; set; } = new Dictionary<string, Dictionary<string, object>>();

        // DateTime storage
        
        // Daily Missions
        public List<MechDefenseHalo.Notifications.Mission> DailyMissions { get; set; } = new List<MechDefenseHalo.Notifications.Mission>();
        
        // Generic Storage
        public Dictionary<string, string> KeyValueStore { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> DateTimeStore { get; set; } = new Dictionary<string, string>();
    }

    #endregion
}
