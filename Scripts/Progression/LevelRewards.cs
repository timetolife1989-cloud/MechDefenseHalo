using Godot;
using System;
using System.Collections.Generic;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;

namespace MechDefenseHalo.Progression
{
    /// <summary>
    /// Manages level-up rewards including currency, inventory slots, and special unlocks
    /// </summary>
    public static class LevelRewards
    {
        private static Dictionary<int, MilestoneReward> _milestoneRewards;
        private static bool _initialized = false;

        /// <summary>
        /// Initialize reward definitions
        /// </summary>
        private static void Initialize()
        {
            if (_initialized) return;

            _milestoneRewards = new Dictionary<int, MilestoneReward>
            {
                { 10, new MilestoneReward 
                    { 
                        Unlock = "second_weapon_slot",
                        Description = "Second Weapon Slot Unlocked"
                    } 
                },
                { 20, new MilestoneReward 
                    { 
                        Unlock = "third_drone_slot",
                        Description = "Third Drone Slot Unlocked"
                    } 
                },
                { 30, new MilestoneReward 
                    { 
                        Unlock = "crafting_speed_boost_10%",
                        Description = "Crafting Speed +10% Unlocked"
                    } 
                },
                { 50, new MilestoneReward 
                    { 
                        Unlock = "fourth_weapon_slot",
                        Cores = 500,
                        Description = "Fourth Weapon Slot Unlocked + 500 Cores"
                    } 
                },
                { 100, new MilestoneReward 
                    { 
                        Unlock = "prestige_system",
                        LegendaryItems = 1,
                        Description = "Prestige System Unlocked + Legendary Item"
                    } 
                }
            };

            _initialized = true;
            GD.Print("LevelRewards initialized");
        }

        /// <summary>
        /// Grant rewards for reaching a specific level
        /// </summary>
        public static void GrantReward(int level)
        {
            if (!_initialized)
            {
                Initialize();
            }

            // Every level rewards
            int credits = 100;
            int cores = 5;

            // Every 5 levels bonus
            if (level % 5 == 0)
            {
                cores += 25; // Extra 25 cores
                // Note: inventory_slots increase handled separately by inventory system
            }

            // Grant currency
            CurrencyManager.AddCredits(credits, $"Level {level} reward");
            CurrencyManager.AddCores(cores, $"Level {level} reward");

            // Check for milestone rewards
            if (_milestoneRewards.ContainsKey(level))
            {
                var milestone = _milestoneRewards[level];
                GrantMilestoneReward(level, milestone);
            }

            // Emit reward granted event
            EventBus.Emit("level_reward_granted", new LevelRewardData
            {
                Level = level,
                Credits = credits,
                Cores = cores,
                IsMilestone = _milestoneRewards.ContainsKey(level)
            });

            GD.Print($"Granted level {level} rewards: {credits} credits, {cores} cores");
        }

        /// <summary>
        /// Grant milestone-specific rewards
        /// </summary>
        private static void GrantMilestoneReward(int level, MilestoneReward reward)
        {
            if (!string.IsNullOrEmpty(reward.Unlock))
            {
                EventBus.Emit("feature_unlocked", new FeatureUnlockedData
                {
                    FeatureName = reward.Unlock,
                    Level = level,
                    Description = reward.Description
                });
                GD.Print($"Milestone {level}: Unlocked {reward.Unlock}");
            }

            if (reward.Cores > 0)
            {
                CurrencyManager.AddCores(reward.Cores, $"Level {level} milestone");
            }

            if (reward.LegendaryItems > 0)
            {
                EventBus.Emit("legendary_item_reward", new LegendaryItemRewardData
                {
                    Level = level,
                    Count = reward.LegendaryItems
                });
                GD.Print($"Milestone {level}: Granted {reward.LegendaryItems} legendary item(s)");
            }
        }

        /// <summary>
        /// Get milestone reward for a specific level
        /// </summary>
        public static MilestoneReward GetMilestoneReward(int level)
        {
            if (!_initialized)
            {
                Initialize();
            }

            return _milestoneRewards.ContainsKey(level) ? _milestoneRewards[level] : null;
        }

        /// <summary>
        /// Check if a level has a milestone reward
        /// </summary>
        public static bool IsMilestone(int level)
        {
            if (!_initialized)
            {
                Initialize();
            }

            return _milestoneRewards.ContainsKey(level);
        }
    }

    #region Data Structures

    /// <summary>
    /// Milestone reward definition
    /// </summary>
    public class MilestoneReward
    {
        public string Unlock { get; set; }
        public int Cores { get; set; }
        public int LegendaryItems { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Level reward event data
    /// </summary>
    public class LevelRewardData
    {
        public int Level { get; set; }
        public int Credits { get; set; }
        public int Cores { get; set; }
        public bool IsMilestone { get; set; }
    }

    /// <summary>
    /// Feature unlocked event data
    /// </summary>
    public class FeatureUnlockedData
    {
        public string FeatureName { get; set; }
        public int Level { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Legendary item reward event data
    /// </summary>
    public class LegendaryItemRewardData
    {
        public int Level { get; set; }
        public int Count { get; set; }
    }

    #endregion
}
