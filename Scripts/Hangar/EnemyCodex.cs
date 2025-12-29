using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Hangar
{
    /// <summary>
    /// Enemy data for display in hangar viewer
    /// </summary>
    public class EnemyData
    {
        public string Id;
        public string Name;
        public string ModelPath;
        public string AnimationPath;
        public bool IsUnlocked;
        public int KillCount;
        public int HP;
        public int Damage;
        public float Speed;
        public string Description;
    }

    /// <summary>
    /// Manages enemy codex entries and unlock progression
    /// </summary>
    public partial class EnemyCodex : Node
    {
        private Dictionary<string, EnemyData> codexEntries = new();
        
        public override void _Ready()
        {
            LoadCodexData();
            
            EventBus.On("EnemyKilled", OnEnemyKilledHandler);
        }

        public override void _ExitTree()
        {
            EventBus.Off("EnemyKilled", OnEnemyKilledHandler);
        }

        private void OnEnemyKilledHandler(object data)
        {
            if (data is string enemyType)
            {
                OnEnemyKilled(enemyType);
            }
        }
        
        private void LoadCodexData()
        {
            // Load all enemy data
            var enemyTypes = new[] { "Grunt", "Shooter", "Tank", "Swarm", "Flyer" };
            
            foreach (string type in enemyTypes)
            {
                var data = new EnemyData
                {
                    Id = type,
                    Name = type,
                    ModelPath = $"res://Entities/Enemies/{type}.tscn",
                    AnimationPath = "idle",
                    IsUnlocked = false,
                    KillCount = 0,
                    HP = GetDefaultHP(type),
                    Damage = GetDefaultDamage(type),
                    Speed = GetDefaultSpeed(type),
                    Description = GetDefaultDescription(type)
                };
                
                codexEntries[type] = data;
            }
            
            LoadProgress();
        }
        
        private int GetDefaultHP(string enemyType)
        {
            return enemyType switch
            {
                "Grunt" => 100,
                "Shooter" => 80,
                "Tank" => 300,
                "Swarm" => 50,
                "Flyer" => 70,
                _ => 100
            };
        }
        
        private int GetDefaultDamage(string enemyType)
        {
            return enemyType switch
            {
                "Grunt" => 10,
                "Shooter" => 15,
                "Tank" => 25,
                "Swarm" => 5,
                "Flyer" => 12,
                _ => 10
            };
        }
        
        private float GetDefaultSpeed(string enemyType)
        {
            return enemyType switch
            {
                "Grunt" => 3.0f,
                "Shooter" => 2.5f,
                "Tank" => 1.5f,
                "Swarm" => 5.0f,
                "Flyer" => 4.0f,
                _ => 3.0f
            };
        }
        
        private string GetDefaultDescription(string enemyType)
        {
            return enemyType switch
            {
                "Grunt" => "Standard infantry unit. Basic threat.",
                "Shooter" => "Ranged attacker with moderate accuracy.",
                "Tank" => "Heavily armored unit. High HP, slow movement.",
                "Swarm" => "Fast, weak unit. Dangerous in groups.",
                "Flyer" => "Airborne unit with evasive capabilities.",
                _ => "Unknown enemy type."
            };
        }
        
        private void OnEnemyKilled(string enemyType)
        {
            if (codexEntries.ContainsKey(enemyType))
            {
                var entry = codexEntries[enemyType];
                entry.KillCount++;
                
                if (!entry.IsUnlocked)
                {
                    entry.IsUnlocked = true;
                    ShowUnlockNotification(entry);
                }
                
                SaveProgress();
            }
        }
        
        public EnemyData GetEnemyData(string enemyId)
        {
            return codexEntries.GetValueOrDefault(enemyId);
        }
        
        public List<EnemyData> GetAllEntries()
        {
            return codexEntries.Values.ToList();
        }
        
        public List<EnemyData> GetUnlockedEntries()
        {
            return codexEntries.Values.Where(e => e.IsUnlocked).ToList();
        }
        
        private void ShowUnlockNotification(EnemyData entry)
        {
            EventBus.Emit("CodexEntryUnlocked", entry.Name);
        }
        
        private void SaveProgress()
        {
            var saveData = new Dictionary<string, Godot.Collections.Dictionary>();
            
            foreach (var entry in codexEntries)
            {
                var entryDict = new Godot.Collections.Dictionary
                {
                    ["unlocked"] = entry.Value.IsUnlocked,
                    ["kills"] = entry.Value.KillCount
                };
                saveData[entry.Key] = entryDict;
            }
            
            SaveManager.SetDict("enemy_codex", saveData);
        }
        
        private void LoadProgress()
        {
            var saveData = SaveManager.GetDict("enemy_codex");
            
            if (saveData == null || saveData.Count == 0) return;
            
            foreach (var kvp in saveData)
            {
                if (codexEntries.ContainsKey(kvp.Key))
                {
                    var entryData = kvp.Value as Godot.Collections.Dictionary;
                    if (entryData != null)
                    {
                        codexEntries[kvp.Key].IsUnlocked = (bool)entryData["unlocked"];
                        // Safe conversion from Variant to int, handling both int and long
                        codexEntries[kvp.Key].KillCount = Convert.ToInt32(entryData["kills"]);
                    }
                }
            }
        }
    }
}
