using Godot;
using System;

namespace MechDefenseHalo.DLC
{
    /// <summary>
    /// Handles loading DLC .pck files and registering content
    /// </summary>
    public partial class DLCLoader : Node
    {
        public void LoadContent(string pckPath)
        {
            GD.Print($"Loading DLC content: {pckPath}");
            
            if (!FileAccess.FileExists(pckPath))
            {
                GD.PrintErr($"DLC content not found: {pckPath}");
                return;
            }
            
            bool success = ProjectSettings.LoadResourcePack(pckPath);
            
            if (success)
            {
                GD.Print("DLC content loaded successfully");
                
                // Register DLC scenes
                RegisterDLCScenes(pckPath);
            }
            else
            {
                GD.PrintErr("Failed to load DLC content");
            }
        }
        
        private void RegisterDLCScenes(string pckPath)
        {
            // Auto-register DLC scenes with game systems
            // e.g., new waves, enemies, bosses
            GD.Print($"Registering DLC scenes from: {pckPath}");
        }
    }
}
