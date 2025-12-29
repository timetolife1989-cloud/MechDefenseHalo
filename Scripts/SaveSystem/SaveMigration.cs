using Godot;
using System;

namespace MechDefenseHalo.SaveSystem
{
    /// <summary>
    /// Handles save file version migration for backwards compatibility
    /// </summary>
    public static class SaveMigration
    {
        private const string CURRENT_VERSION = "1.0.0";
        
        /// <summary>
        /// Migrate save data to current version
        /// </summary>
        /// <param name="saveData">Save data to migrate</param>
        /// <returns>Migrated save data</returns>
        public static SaveData MigrateToCurrentVersion(SaveData saveData)
        {
            if (saveData == null)
            {
                GD.PrintErr("Cannot migrate null save data");
                return null;
            }
            
            string oldVersion = saveData.Version;
            
            if (oldVersion == CURRENT_VERSION)
            {
                // Already on current version
                return saveData;
            }
            
            GD.Print($"Migrating save from version {oldVersion} to {CURRENT_VERSION}");
            
            // Example migration path
            // if (oldVersion == "0.9.0")
            // {
            //     saveData = MigrateFrom_0_9_0_To_1_0_0(saveData);
            // }
            
            saveData.Version = CURRENT_VERSION;
            return saveData;
        }
        
        /// <summary>
        /// Check if save data version is compatible
        /// </summary>
        /// <param name="version">Version string to check</param>
        /// <returns>True if version can be migrated</returns>
        public static bool IsVersionCompatible(string version)
        {
            if (string.IsNullOrEmpty(version))
                return false;
            
            // For now, all versions are compatible
            // In the future, add logic to check if migration path exists
            return true;
        }
        
        // Example migration method
        // private static SaveData MigrateFrom_0_9_0_To_1_0_0(SaveData saveData)
        // {
        //     // Add new fields with defaults
        //     if (saveData.Player != null)
        //     {
        //         saveData.Player.PrestigeLevel = 0;
        //     }
        //     
        //     return saveData;
        // }
    }
}
