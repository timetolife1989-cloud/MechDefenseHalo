using Godot;
using System;

namespace MechDefenseHalo.Combat
{
    /// <summary>
    /// Handles hit detection logic for projectiles and raycasts.
    /// </summary>
    public partial class HitDetection : Node
    {
        private static HitDetection _instance;
        public static HitDetection Instance => _instance;
        
        public override void _Ready()
        {
            _instance = this;
            GD.Print("HitDetection initialized");
        }
    }
}
