using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Loot
{
    /// <summary>
    /// Manages loot tables and random loot generation
    /// </summary>
    public partial class LootTableManager : Node
    {
        #region Singleton

        private static LootTableManager _instance;

        public static LootTableManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("LootTableManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Private Fields

        private Dictionary<string, LootTable> _lootTables = new();
        private const string LOOT_TABLES_PATH = "res://Data/LootTables/";

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple LootTableManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            LoadAllLootTables();
            GD.Print("LootTableManager initialized");
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
        /// Get a loot table by ID
        /// </summary>
        /// <param name="tableID">Loot table identifier</param>
        /// <returns>The loot table or null if not found</returns>
        public static LootTable GetLootTable(string tableID)
        {
            if (Instance == null) return null;

            return Instance._lootTables.TryGetValue(tableID, out var table) ? table : null;
        }

        /// <summary>
        /// Roll loot from a specific loot table
        /// </summary>
        /// <param name="tableID">Loot table identifier</param>
        /// <param name="luckModifier">Luck multiplier for rare drops</param>
        /// <returns>List of item IDs to drop</returns>
        public static List<string> RollLoot(string tableID, float luckModifier = 1.0f)
        {
            if (Instance == null) return new List<string>();

            var table = GetLootTable(tableID);
            if (table == null)
            {
                GD.PrintErr($"Loot table not found: {tableID}");
                return new List<string>();
            }

            return Instance.RollLootInternal(table, luckModifier);
        }

        #endregion

        #region Private Methods

        private void LoadAllLootTables()
        {
            _lootTables.Clear();

            // Load enemy loot tables
            LoadLootTablesFromDirectory($"{LOOT_TABLES_PATH}enemies/");
            
            // Load boss loot tables
            LoadLootTablesFromDirectory($"{LOOT_TABLES_PATH}bosses/");

            GD.Print($"Loaded {_lootTables.Count} loot tables");
        }

        private void LoadLootTablesFromDirectory(string dirPath)
        {
            using var dir = DirAccess.Open(dirPath);
            
            if (dir == null)
            {
                GD.Print($"Loot tables directory not found: {dirPath}");
                return;
            }

            dir.ListDirBegin();
            string fileName = dir.GetNext();

            while (!string.IsNullOrEmpty(fileName))
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".json"))
                {
                    string filePath = $"{dirPath}{fileName}";
                    LoadLootTable(filePath);
                }
                fileName = dir.GetNext();
            }

            dir.ListDirEnd();
        }

        private void LoadLootTable(string filePath)
        {
            try
            {
                using var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"Failed to open loot table: {filePath}");
                    return;
                }

                string json = file.GetAsText();
                var table = JsonSerializer.Deserialize<LootTable>(json);

                if (table != null && !string.IsNullOrEmpty(table.EnemyType))
                {
                    _lootTables[table.EnemyType] = table;
                    GD.Print($"Loaded loot table: {table.EnemyType}");
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading loot table {filePath}: {e.Message}");
            }
        }

        private List<string> RollLootInternal(LootTable table, float luckModifier)
        {
            var drops = new List<string>();

            // Add guaranteed drops
            if (table.GuaranteedDrops != null)
            {
                drops.AddRange(table.GuaranteedDrops);
            }

            // Determine how many items to drop
            int dropCount = GD.RandRange(table.DropCountRange[0], table.DropCountRange[1]);

            // Roll from loot pools
            for (int i = 0; i < dropCount; i++)
            {
                var pool = SelectRandomPool(table.LootPools);
                if (pool != null && pool.Items != null && pool.Items.Length > 0)
                {
                    string item = pool.Items[GD.RandRange(0, pool.Items.Length - 1)];
                    drops.Add(item);
                }
            }

            return drops;
        }

        private LootPool SelectRandomPool(LootPool[] pools)
        {
            if (pools == null || pools.Length == 0)
                return null;

            // Calculate total weight
            float totalWeight = 0f;
            foreach (var pool in pools)
            {
                totalWeight += pool.Weight;
            }

            // Roll and select pool
            float roll = GD.Randf() * totalWeight;
            float cumulative = 0f;

            foreach (var pool in pools)
            {
                cumulative += pool.Weight;
                if (roll <= cumulative)
                {
                    return pool;
                }
            }

            return pools[pools.Length - 1]; // Fallback
        }

        #endregion
    }

    #region Data Structures

    /// <summary>
    /// Loot table definition
    /// </summary>
    public class LootTable
    {
        [JsonPropertyName("enemy_type")]
        public string EnemyType { get; set; } = "";

        [JsonPropertyName("loot_pools")]
        public LootPool[] LootPools { get; set; } = Array.Empty<LootPool>();

        [JsonPropertyName("guaranteed_drops")]
        public string[] GuaranteedDrops { get; set; } = Array.Empty<string>();

        [JsonPropertyName("drop_count_range")]
        public int[] DropCountRange { get; set; } = new int[] { 1, 3 };
    }

    /// <summary>
    /// Individual loot pool within a table
    /// </summary>
    public class LootPool
    {
        [JsonPropertyName("pool_name")]
        public string PoolName { get; set; } = "";

        [JsonPropertyName("weight")]
        public float Weight { get; set; } = 1.0f;

        [JsonPropertyName("items")]
        public string[] Items { get; set; } = Array.Empty<string>();
    }

    #endregion
}
