using Godot;
using MechDefenseHalo.Audio;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Audio
{
    /// <summary>
    /// Example implementation of audio event connections.
    /// This shows how to wire up game events to the audio system.
    /// 
    /// In your actual game, you would add these connections in the appropriate
    /// game systems (weapons, enemies, UI, etc.) rather than in a single file.
    /// </summary>
    public partial class AudioEventConnections : Node
    {
        public override void _Ready()
        {
            SetupCombatAudio();
            SetupUIAudio();
            SetupPlayerAudio();
            SetupLootAudio();
            
            GD.Print("Audio event connections initialized");
        }
        
        private void SetupCombatAudio()
        {
            // Weapon fired - play weapon sound
            EventBus.On(EventBus.WeaponFired, Callable.From<object>((data) =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("weapon_fire");
                }
            }));
            
            // Enemy hit - play impact sound at enemy position
            EventBus.On("EnemyHit", Callable.From<object>((data) =>
            {
                if (AudioManager.Instance != null && data != null)
                {
                    // Try to extract position from data
                    var dataType = data.GetType();
                    var positionProp = dataType.GetProperty("Position");
                    if (positionProp != null)
                    {
                        Vector3 position = (Vector3)positionProp.GetValue(data);
                        AudioManager.Instance.PlaySound("enemy_hit", position);
                    }
                    else
                    {
                        AudioManager.Instance.PlaySound("enemy_hit");
                    }
                }
            });
            
            // Enemy killed - play death sound at enemy position
            EventBus.On(EventBus.EntityDied, Callable.From<object>((data) =>
            {
                if (AudioManager.Instance != null && data != null)
                {
                    // Check if the entity is an enemy
                    var dataType = data.GetType();
                    var entityTypeProp = dataType.GetProperty("EntityType");
                    var positionProp = dataType.GetProperty("Position");
                    
                    if (entityTypeProp != null)
                    {
                        string entityType = entityTypeProp.GetValue(data)?.ToString();
                        if (entityType != null && entityType.Contains("Enemy"))
                        {
                            if (positionProp != null)
                            {
                                Vector3 position = (Vector3)positionProp.GetValue(data);
                                AudioManager.Instance.PlaySound("enemy_death", position);
                            }
                            else
                            {
                                AudioManager.Instance.PlaySound("enemy_death");
                            }
                        }
                    }
                }
            });
            
            // Boss roar when boss spawns
            EventBus.On(EventBus.BossSpawned, Callable.From<object>((data) =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("boss_roar");
                }
            });
        }
        
        private void SetupUIAudio()
        {
            // Button click sound
            EventBus.On(EventBus.ButtonClicked, Callable.From<object>((data) =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayUISound("ui_click");
                }
            });
        }
        
        private void SetupPlayerAudio()
        {
            // Player level up sound
            EventBus.On(EventBus.PlayerLeveledUp, Callable.From<object>((data) =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("level_up");
                }
            });
        }
        
        private void SetupLootAudio()
        {
            // Item looted/picked up
            EventBus.On(EventBus.LootPickedUp, Callable.From<object>((data) =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("loot_pickup");
                }
            }));
            
            // Achievement unlocked
            EventBus.On(EventBus.AchievementUnlocked, Callable.From<object>((data) =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound("achievement");
                }
            });
        }
    }
}
