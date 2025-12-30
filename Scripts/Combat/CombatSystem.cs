using Godot;
using System;

namespace MechDefenseHalo.Combat
{
    /// <summary>
    /// Central combat system manager.
    /// Handles combat coordination and state management.
    /// </summary>
    public partial class CombatSystem : Node
    {
        private static CombatSystem _instance;
        public static CombatSystem Instance => _instance;
        
        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple CombatSystem instances detected!");
                QueueFree();
                return;
            }
            _instance = this;
            GD.Print("CombatSystem initialized");
        }
    }
}
