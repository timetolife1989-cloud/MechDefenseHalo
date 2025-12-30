using Godot;
using System;

namespace MechDefenseHalo.Weapons
{
    /// <summary>
    /// Central weapon system orchestrator.
    /// Manages global weapon behavior and interactions.
    /// </summary>
    public partial class WeaponSystem : Node
    {
        private static WeaponSystem _instance;
        
        public static WeaponSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("WeaponSystem accessed before initialization!");
                }
                return _instance;
            }
        }
        
        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple WeaponSystem instances detected! Removing duplicate.");
                QueueFree();
                return;
            }
            
            _instance = this;
            GD.Print("WeaponSystem initialized successfully");
        }
        
        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
