using Godot;
using System;
using System.Text.Json;
using System.Collections.Generic;

namespace MechDefenseHalo.Core
{
    /// <summary>
    /// Manages game save/load functionality.
    /// Handles player data persistence.
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

        private const string SaveFileName = "user://save_data.json";
        private const int SaveVersion = 1;

        #endregion

        #region Public Properties

        public PlayerData CurrentPlayerData { get; private set; }

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

            // Load existing save or create new
            LoadGame();

            GD.Print("SaveManager initialized");
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Save current game data
        /// </summary>
        public bool SaveGame()
        {
            try
            {
                var saveData = new SaveData
                {
                    Version = SaveVersion,
                    PlayerData = CurrentPlayerData,
                    SaveTime = DateTime.Now.ToString("o")
                };

                string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });

                using var file = FileAccess.Open(SaveFileName, FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    GD.PrintErr($"Failed to open save file: {FileAccess.GetOpenError()}");
                    return false;
                }

                file.StoreString(json);
                file.Close();

                GD.Print($"Game saved successfully to {SaveFileName}");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to save game: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load game data
        /// </summary>
        public bool LoadGame()
        {
            try
            {
                if (!FileAccess.FileExists(SaveFileName))
                {
                    GD.Print("No save file found, creating new player data");
                    CurrentPlayerData = new PlayerData();
                    return true;
                }

                using var file = FileAccess.Open(SaveFileName, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"Failed to open save file: {FileAccess.GetOpenError()}");
                    CurrentPlayerData = new PlayerData();
                    return false;
                }

                string json = file.GetAsText();
                file.Close();

                var saveData = JsonSerializer.Deserialize<SaveData>(json);

                if (saveData == null || saveData.Version != SaveVersion)
                {
                    GD.PrintErr("Invalid or outdated save file");
                    CurrentPlayerData = new PlayerData();
                    return false;
                }

                CurrentPlayerData = saveData.PlayerData ?? new PlayerData();
                GD.Print($"Game loaded successfully from {SaveFileName}");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load game: {e.Message}");
                CurrentPlayerData = new PlayerData();
                return false;
            }
        }

        /// <summary>
        /// Delete save file
        /// </summary>
        public void DeleteSave()
        {
            if (FileAccess.FileExists(SaveFileName))
            {
                DirAccess.RemoveAbsolute(SaveFileName);
                GD.Print("Save file deleted");
            }

            CurrentPlayerData = new PlayerData();
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Container for save file data
    /// </summary>
    public class SaveData
    {
        public int Version { get; set; }
        public string SaveTime { get; set; }
        public PlayerData PlayerData { get; set; }
    }

    /// <summary>
    /// Player progression and statistics data
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
    }

    #endregion
}
