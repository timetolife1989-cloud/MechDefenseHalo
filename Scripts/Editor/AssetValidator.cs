using Godot;
using System.IO;

namespace MechDefenseHalo.Editor
{
    public class AssetValidator
    {
        public bool ValidateAsset(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            
            return extension switch
            {
                ".blend" => ValidateBlendFile(filePath),
                ".png" or ".jpg" or ".jpeg" or ".tga" => ValidateTexture(filePath),
                ".wav" or ".ogg" => ValidateAudio(filePath),
                _ => true
            };
        }
        
        private bool ValidateBlendFile(string filePath)
        {
            // Check file size
            var fileInfo = new FileInfo(filePath);
            
            if (fileInfo.Length > 50 * 1024 * 1024) // 50MB
            {
                GD.PrintErr($"Blend file too large: {fileInfo.Length / 1024 / 1024}MB");
                return false;
            }
            
            return true;
        }
        
        private bool ValidateTexture(string filePath)
        {
            // Check texture resolution
            var image = Image.LoadFromFile(filePath);
            
            if (image == null)
                return false;
            
            int width = image.GetWidth();
            int height = image.GetHeight();
            
            // Check power of 2
            if (!IsPowerOfTwo(width) || !IsPowerOfTwo(height))
            {
                GD.PrintErr($"Texture not power of 2: {width}x{height}");
                return false;
            }
            
            // Check max size
            if (width > 4096 || height > 4096)
            {
                GD.PrintErr($"Texture too large: {width}x{height}");
                return false;
            }
            
            return true;
        }
        
        private bool ValidateAudio(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            
            // Check file size
            if (fileInfo.Length > 10 * 1024 * 1024) // 10MB
            {
                GD.PrintErr($"Audio file too large: {fileInfo.Length / 1024 / 1024}MB");
                return false;
            }
            
            return true;
        }
        
        private bool IsPowerOfTwo(int value)
        {
            return value > 0 && (value & (value - 1)) == 0;
        }
    }
}
