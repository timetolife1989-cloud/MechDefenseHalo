using System.IO;
using System.Collections.Generic;

namespace MechDefenseHalo.Editor
{
    public class NamingConventionEnforcer
    {
        // Naming convention: category_name_variant_lod.ext
        // Examples:
        // - enemy_grunt_v01.blend
        // - weapon_rifle_laser_v02.blend
        // - texture_metal_rusty_01_diffuse.png
        // - audio_sfx_explosion_01.wav
        
        private static readonly HashSet<string> Exceptions = new() 
        { 
            "icon", "thumbnail", "preview" 
        };
        
        private static readonly HashSet<string> ValidCategories = new()
        {
            "enemy", "weapon", "item", "armor", 
            "texture", "audio", "vfx", "ui", "environment"
        };
        
        public bool IsValidName(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            
            // Allow certain exceptions
            if (IsException(fileName))
                return true;
            
            // Check format
            string[] parts = fileName.Split('_');
            
            if (parts.Length < 2)
                return false;
            
            // First part should be category
            string category = parts[0].ToLower();
            if (!IsValidCategory(category))
                return false;
            
            return true;
        }
        
        private bool IsException(string fileName)
        {
            // Allow certain filenames without convention
            return Exceptions.Contains(fileName.ToLower());
        }
        
        private bool IsValidCategory(string category)
        {
            return ValidCategories.Contains(category);
        }
        
        public string SuggestCorrectName(string fileName)
        {
            // AI-based suggestion (placeholder)
            return "category_name_v01";
        }
    }
}
