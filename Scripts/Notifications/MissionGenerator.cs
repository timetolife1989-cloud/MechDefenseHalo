using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MechDefenseHalo.Notifications
{
    /// <summary>
    /// Generates random daily missions based on templates
    /// </summary>
    public partial class MissionGenerator : Node
    {
        private List<MissionTemplate> templates;
        
        public override void _Ready()
        {
            LoadMissionTemplates();
        }
        
        private void LoadMissionTemplates()
        {
            // Initialize with mission templates
            templates = new List<MissionTemplate>
            {
                // Easy missions
                new MissionTemplate
                {
                    Type = MissionType.KillEnemies,
                    Title = "Extermination",
                    Description = "Kill {0} enemies",
                    ProgressRange = new Vector2I(20, 50),
                    Difficulty = Difficulty.Easy,
                    Rewards = new() { ["Credits"] = 200, ["XP"] = 100 }
                },
                new MissionTemplate
                {
                    Type = MissionType.CompleteWaves,
                    Title = "Wave Breaker",
                    Description = "Complete {0} waves",
                    ProgressRange = new Vector2I(3, 5),
                    Difficulty = Difficulty.Easy,
                    Rewards = new() { ["Credits"] = 300, ["XP"] = 150 }
                },
                new MissionTemplate
                {
                    Type = MissionType.KillEnemies,
                    Title = "Bug Hunt",
                    Description = "Eliminate {0} hostiles",
                    ProgressRange = new Vector2I(30, 60),
                    Difficulty = Difficulty.Easy,
                    Rewards = new() { ["Credits"] = 250, ["XP"] = 120 }
                },
                new MissionTemplate
                {
                    Type = MissionType.CollectLoot,
                    Title = "Salvage Operation",
                    Description = "Collect {0} loot items",
                    ProgressRange = new Vector2I(10, 20),
                    Difficulty = Difficulty.Easy,
                    Rewards = new() { ["Credits"] = 200, ["XP"] = 100 }
                },
                new MissionTemplate
                {
                    Type = MissionType.DeployDrones,
                    Title = "Drone Operator",
                    Description = "Deploy {0} drones",
                    ProgressRange = new Vector2I(5, 10),
                    Difficulty = Difficulty.Easy,
                    Rewards = new() { ["Credits"] = 250, ["XP"] = 130 }
                },
                
                // Medium missions
                new MissionTemplate
                {
                    Type = MissionType.DeployDrones,
                    Title = "Support Commander",
                    Description = "Deploy {0} drones",
                    ProgressRange = new Vector2I(10, 20),
                    Difficulty = Difficulty.Medium,
                    Rewards = new() { ["Credits"] = 400, ["XP"] = 200, ["Cores"] = 10 }
                },
                new MissionTemplate
                {
                    Type = MissionType.CraftItems,
                    Title = "Master Craftsman",
                    Description = "Craft {0} items",
                    ProgressRange = new Vector2I(3, 5),
                    Difficulty = Difficulty.Medium,
                    Rewards = new() { ["Credits"] = 500, ["XP"] = 250, ["Cores"] = 15 }
                },
                new MissionTemplate
                {
                    Type = MissionType.KillEnemies,
                    Title = "Heavy Elimination",
                    Description = "Destroy {0} enemies",
                    ProgressRange = new Vector2I(50, 100),
                    Difficulty = Difficulty.Medium,
                    Rewards = new() { ["Credits"] = 450, ["XP"] = 220, ["Cores"] = 12 }
                },
                new MissionTemplate
                {
                    Type = MissionType.CompleteWaves,
                    Title = "Defense Expert",
                    Description = "Survive {0} waves",
                    ProgressRange = new Vector2I(5, 10),
                    Difficulty = Difficulty.Medium,
                    Rewards = new() { ["Credits"] = 500, ["XP"] = 250, ["Cores"] = 15 }
                },
                new MissionTemplate
                {
                    Type = MissionType.DealDamage,
                    Title = "Artillery Strike",
                    Description = "Deal {0} damage",
                    ProgressRange = new Vector2I(25000, 50000),
                    Difficulty = Difficulty.Medium,
                    Rewards = new() { ["Credits"] = 450, ["XP"] = 220, ["Cores"] = 12 }
                },
                new MissionTemplate
                {
                    Type = MissionType.CollectLoot,
                    Title = "Treasure Hunter",
                    Description = "Collect {0} rare items",
                    ProgressRange = new Vector2I(15, 30),
                    Difficulty = Difficulty.Medium,
                    Rewards = new() { ["Credits"] = 400, ["XP"] = 200, ["Cores"] = 10 }
                },
                new MissionTemplate
                {
                    Type = MissionType.SurviveTime,
                    Title = "Endurance Test",
                    Description = "Survive for {0} seconds",
                    ProgressRange = new Vector2I(300, 600),
                    Difficulty = Difficulty.Medium,
                    Rewards = new() { ["Credits"] = 500, ["XP"] = 250, ["Cores"] = 15 }
                },
                
                // Hard missions
                new MissionTemplate
                {
                    Type = MissionType.DefeatBosses,
                    Title = "Titan Slayer",
                    Description = "Defeat {0} bosses",
                    ProgressRange = new Vector2I(1, 2),
                    Difficulty = Difficulty.Hard,
                    Rewards = new() { ["Credits"] = 1000, ["XP"] = 500, ["Cores"] = 50 }
                },
                new MissionTemplate
                {
                    Type = MissionType.DealDamage,
                    Title = "Destruction Expert",
                    Description = "Deal {0} damage",
                    ProgressRange = new Vector2I(50000, 100000),
                    Difficulty = Difficulty.Hard,
                    Rewards = new() { ["Credits"] = 800, ["XP"] = 400, ["Cores"] = 30 }
                },
                new MissionTemplate
                {
                    Type = MissionType.KillEnemies,
                    Title = "Annihilation",
                    Description = "Eliminate {0} hostiles",
                    ProgressRange = new Vector2I(100, 200),
                    Difficulty = Difficulty.Hard,
                    Rewards = new() { ["Credits"] = 900, ["XP"] = 450, ["Cores"] = 40 }
                },
                new MissionTemplate
                {
                    Type = MissionType.CompleteWaves,
                    Title = "Ultimate Defense",
                    Description = "Complete {0} waves",
                    ProgressRange = new Vector2I(10, 15),
                    Difficulty = Difficulty.Hard,
                    Rewards = new() { ["Credits"] = 1000, ["XP"] = 500, ["Cores"] = 50 }
                },
                new MissionTemplate
                {
                    Type = MissionType.DefeatBosses,
                    Title = "Boss Hunter",
                    Description = "Defeat {0} elite bosses",
                    ProgressRange = new Vector2I(2, 3),
                    Difficulty = Difficulty.Hard,
                    Rewards = new() { ["Credits"] = 1200, ["XP"] = 600, ["Cores"] = 60 }
                },
                new MissionTemplate
                {
                    Type = MissionType.CraftItems,
                    Title = "Legendary Craftsman",
                    Description = "Craft {0} legendary items",
                    ProgressRange = new Vector2I(1, 3),
                    Difficulty = Difficulty.Hard,
                    Rewards = new() { ["Credits"] = 1000, ["XP"] = 500, ["Cores"] = 50 }
                },
                new MissionTemplate
                {
                    Type = MissionType.DeployDrones,
                    Title = "Swarm Master",
                    Description = "Deploy {0} drones in battle",
                    ProgressRange = new Vector2I(25, 40),
                    Difficulty = Difficulty.Hard,
                    Rewards = new() { ["Credits"] = 900, ["XP"] = 450, ["Cores"] = 40 }
                },
                new MissionTemplate
                {
                    Type = MissionType.SurviveTime,
                    Title = "Marathon Runner",
                    Description = "Survive for {0} seconds",
                    ProgressRange = new Vector2I(600, 1200),
                    Difficulty = Difficulty.Hard,
                    Rewards = new() { ["Credits"] = 1000, ["XP"] = 500, ["Cores"] = 50 }
                },
                new MissionTemplate
                {
                    Type = MissionType.DealDamage,
                    Title = "Devastator",
                    Description = "Deal {0} total damage",
                    ProgressRange = new Vector2I(100000, 200000),
                    Difficulty = Difficulty.Hard,
                    Rewards = new() { ["Credits"] = 1100, ["XP"] = 550, ["Cores"] = 55 }
                },
                new MissionTemplate
                {
                    Type = MissionType.CollectLoot,
                    Title = "Master Collector",
                    Description = "Collect {0} legendary loot items",
                    ProgressRange = new Vector2I(5, 10),
                    Difficulty = Difficulty.Hard,
                    Rewards = new() { ["Credits"] = 900, ["XP"] = 450, ["Cores"] = 40 }
                }
            };

            GD.Print($"Loaded {templates.Count} mission templates");
        }
        
        /// <summary>
        /// Generate a random mission based on difficulty
        /// </summary>
        public Mission GenerateMission(Difficulty difficulty)
        {
            var validTemplates = templates.Where(t => t.Difficulty == difficulty).ToList();
            
            if (validTemplates.Count == 0)
            {
                GD.PrintErr($"No templates found for difficulty: {difficulty}");
                return null;
            }
            
            var template = validTemplates[GD.RandRange(0, validTemplates.Count - 1)];
            
            int requiredProgress = GD.RandRange(template.ProgressRange.X, template.ProgressRange.Y);
            
            return new Mission
            {
                ID = Guid.NewGuid().ToString(),
                Title = template.Title,
                Description = string.Format(template.Description, requiredProgress),
                Type = template.Type,
                RequiredProgress = requiredProgress,
                CurrentProgress = 0,
                Rewards = new Dictionary<string, int>(template.Rewards),
                IsCompleted = false,
                IsRewardClaimed = false
            };
        }
    }
}
