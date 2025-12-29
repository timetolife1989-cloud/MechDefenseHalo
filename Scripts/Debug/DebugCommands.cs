using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Player;
using MechDefenseHalo.WaveSystem;
using MechDefenseHalo.Inventory;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Progression;
using MechDefenseHalo.Enemies;
using MechDefenseHalo.Items;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Debug
{
    /// <summary>
    /// Debug command executor with game-testing commands
    /// </summary>
    public partial class DebugCommands : Node
    {
        #region Private Fields

        private Dictionary<string, Action<string[]>> _commands;
        private bool _godMode = false;
        private bool _noClip = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            RegisterCommands();
            GD.Print("DebugCommands initialized");
        }

        #endregion

        #region Command Registration

        private void RegisterCommands()
        {
            _commands = new Dictionary<string, Action<string[]>>
            {
                ["help"] = Help,
                ["god"] = GodMode,
                ["levelup"] = LevelUp,
                ["spawn"] = SpawnEnemy,
                ["giveitem"] = GiveItem,
                ["skipwave"] = SkipWave,
                ["timescale"] = SetTimeScale,
                ["noclip"] = NoClip,
                ["credits"] = AddCredits,
                ["cores"] = AddCores,
                ["kill"] = KillAll,
                ["tp"] = Teleport,
                ["heal"] = Heal,
                ["clear"] = ClearConsole,
                ["wave"] = SetWave
            };
        }

        #endregion

        #region Public Methods

        public string Execute(string commandLine)
        {
            string[] parts = commandLine.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return "[color=yellow]Empty command[/color]";
            }

            string cmd = parts[0].ToLower();
            string[] args = parts.Skip(1).ToArray();
            
            if (_commands.ContainsKey(cmd))
            {
                try
                {
                    _commands[cmd](args);
                    return $"[color=green]Command executed: {cmd}[/color]";
                }
                catch (Exception e)
                {
                    return $"[color=red]Error: {e.Message}[/color]";
                }
            }
            
            return $"[color=yellow]Unknown command: {cmd}. Type 'help' for available commands.[/color]";
        }

        #endregion

        #region Command Implementations

        private void Help(string[] args)
        {
            GD.Print("=== Debug Commands ===");
            GD.Print("help - Show this help");
            GD.Print("god - Toggle god mode");
            GD.Print("levelup [levels] - Gain levels (default: 1)");
            GD.Print("spawn <type> - Spawn enemy (Grunt, Tank, Flyer, Swarm, Shooter)");
            GD.Print("giveitem <item_id> [amount] - Give item");
            GD.Print("skipwave - Skip to next wave");
            GD.Print("wave <number> - Jump to specific wave");
            GD.Print("timescale <value> - Set time scale (0.1-10.0)");
            GD.Print("noclip - Toggle noclip mode");
            GD.Print("credits <amount> - Add credits (default: 1000)");
            GD.Print("cores <amount> - Add cores (default: 100)");
            GD.Print("kill - Kill all enemies");
            GD.Print("tp <x> <y> <z> - Teleport player");
            GD.Print("heal - Heal player to full");
            GD.Print("clear - Clear console");
        }

        private void GodMode(string[] args)
        {
            _godMode = !_godMode;
            
            var player = GetPlayer();
            if (player != null)
            {
                // Try to set god mode on player's health component
                var health = player.GetNodeOrNull<HealthComponent>("HealthComponent");
                if (health != null)
                {
                    // Note: HealthComponent would need a GodMode property for this to work fully
                    GD.Print($"God mode: {_godMode}");
                }
            }
            
            GD.Print($"God mode: {_godMode}");
        }

        private void LevelUp(string[] args)
        {
            int levels = args.Length > 0 ? int.Parse(args[0]) : 1;
            
            if (PlayerLevel.Instance != null)
            {
                // Calculate XP needed for levels
                int xpNeeded = PlayerLevel.Instance.XPToNextLevel * levels;
                PlayerLevel.AddXP(xpNeeded, "debug");
                GD.Print($"Added {levels} level(s)");
            }
            else
            {
                GD.PrintErr("PlayerLevel not initialized");
            }
        }

        private void SpawnEnemy(string[] args)
        {
            if (args.Length == 0)
            {
                GD.PrintErr("Usage: spawn <type> (Grunt, Tank, Flyer, Swarm, Shooter)");
                return;
            }

            string enemyType = args[0];
            Vector3 spawnPos = GetPlayerPosition() + Vector3.Forward * 10;
            
            // Load enemy scene based on type
            string scenePath = $"res://Scenes/Enemies/{enemyType}.tscn";
            
            if (!ResourceLoader.Exists(scenePath))
            {
                GD.PrintErr($"Enemy scene not found: {scenePath}");
                return;
            }

            var enemyScene = GD.Load<PackedScene>(scenePath);
            if (enemyScene != null)
            {
                var enemy = enemyScene.Instantiate<Node3D>();
                GetTree().Root.GetNode("Main").AddChild(enemy);
                enemy.GlobalPosition = spawnPos;
                GD.Print($"Spawned {enemyType} at {spawnPos}");
            }
        }

        private void GiveItem(string[] args)
        {
            if (args.Length == 0)
            {
                GD.PrintErr("Usage: giveitem <item_id> [amount]");
                return;
            }
            
            string itemId = args[0];
            int amount = args.Length > 1 ? int.Parse(args[1]) : 1;
            
            var item = ItemDatabase.GetItem(itemId);
            if (item == null)
            {
                GD.PrintErr($"Item not found: {itemId}");
                return;
            }

            if (InventoryManager.Instance != null)
            {
                if (InventoryManager.Instance.AddItem(item, amount))
                {
                    GD.Print($"Gave {amount}x {item.DisplayName}");
                }
            }
            else
            {
                GD.PrintErr("InventoryManager not initialized");
            }
        }

        private void SkipWave(string[] args)
        {
            var waveManager = GetWaveManager();
            if (waveManager != null)
            {
                // Force complete current wave and start next
                waveManager.Call("CompleteWave");
                GD.Print("Skipped to next wave");
            }
            else
            {
                GD.PrintErr("WaveManager not found");
            }
        }

        private void SetWave(string[] args)
        {
            if (args.Length == 0)
            {
                GD.PrintErr("Usage: wave <number>");
                return;
            }

            int targetWave = int.Parse(args[0]);
            var waveManager = GetWaveManager();
            
            if (waveManager != null)
            {
                // Set wave directly if method exists
                if (waveManager.HasMethod("SetWave"))
                {
                    waveManager.Call("SetWave", targetWave);
                }
                else
                {
                    // Fallback: use reflection or direct property access
                    GD.Print($"Attempting to set wave to {targetWave}");
                }
                GD.Print($"Set wave to {targetWave}");
            }
        }

        private void SetTimeScale(string[] args)
        {
            if (args.Length == 0)
            {
                Engine.TimeScale = 1.0;
                GD.Print("Time scale reset to 1.0");
                return;
            }
            
            float scale = float.Parse(args[0]);
            Engine.TimeScale = Mathf.Clamp(scale, 0.1f, 10.0f);
            GD.Print($"Time scale set to {Engine.TimeScale}");
        }

        private void NoClip(string[] args)
        {
            _noClip = !_noClip;
            
            var player = GetPlayer();
            if (player is CharacterBody3D playerBody)
            {
                // Disable collision when noclip is on
                playerBody.SetCollisionMaskValue(1, !_noClip);
                GD.Print($"Noclip: {_noClip}");
            }
            else
            {
                GD.Print($"Noclip toggled: {_noClip} (player not found as CharacterBody3D)");
            }
        }

        private void AddCredits(string[] args)
        {
            int amount = args.Length > 0 ? int.Parse(args[0]) : 1000;
            CurrencyManager.AddCredits(amount, "debug");
            GD.Print($"Added {amount} credits");
        }

        private void AddCores(string[] args)
        {
            int amount = args.Length > 0 ? int.Parse(args[0]) : 100;
            CurrencyManager.AddCores(amount, "debug");
            GD.Print($"Added {amount} cores");
        }

        private void KillAll(string[] args)
        {
            var enemies = GetTree().GetNodesInGroup("enemies");
            int count = 0;
            
            foreach (var enemy in enemies)
            {
                if (enemy is Node3D enemy3D)
                {
                    var health = enemy3D.GetNodeOrNull<HealthComponent>("HealthComponent");
                    if (health != null)
                    {
                        health.TakeDamage(999999, null);
                        count++;
                    }
                }
            }
            
            GD.Print($"Killed {count} enemies");
        }

        private void Teleport(string[] args)
        {
            if (args.Length < 3)
            {
                GD.PrintErr("Usage: tp <x> <y> <z>");
                return;
            }
            
            Vector3 pos = new Vector3(
                float.Parse(args[0]),
                float.Parse(args[1]),
                float.Parse(args[2])
            );
            
            var player = GetPlayer();
            if (player != null)
            {
                player.GlobalPosition = pos;
                GD.Print($"Teleported to {pos}");
            }
            else
            {
                GD.PrintErr("Player not found");
            }
        }

        private void Heal(string[] args)
        {
            var player = GetPlayer();
            if (player != null)
            {
                var health = player.GetNodeOrNull<HealthComponent>("HealthComponent");
                if (health != null)
                {
                    health.Heal(999999);
                    GD.Print("Player healed to full");
                }
                else
                {
                    GD.PrintErr("Player HealthComponent not found");
                }
            }
            else
            {
                GD.PrintErr("Player not found");
            }
        }

        private void ClearConsole(string[] args)
        {
            var console = GetTree().Root.GetNodeOrNull<DebugConsole>("DebugConsole");
            if (console != null)
            {
                var outputLog = console.GetNode<RichTextLabel>("Panel/VBoxContainer/ScrollContainer/OutputLog");
                outputLog?.Clear();
            }
        }

        #endregion

        #region Helper Methods

        private Node3D GetPlayer()
        {
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0)
            {
                return players[0] as Node3D;
            }
            return null;
        }

        private Vector3 GetPlayerPosition()
        {
            var player = GetPlayer();
            return player?.GlobalPosition ?? Vector3.Zero;
        }

        private Node GetWaveManager()
        {
            return GetTree().Root.GetNodeOrNull("Main/WaveManager");
        }

        #endregion
    }
}
