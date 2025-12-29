using Godot;
using System;
using System.Collections.Generic;
using System.IO;

namespace MechDefenseHalo.Editor
{
    public partial class ContentVersionTracker : Node
    {
        private Dictionary<string, AssetVersion> assetVersions = new();
        
        public void TrackAsset(string assetPath)
        {
            string hash = CalculateFileHash(assetPath);
            
            if (assetVersions.ContainsKey(assetPath))
            {
                var version = assetVersions[assetPath];
                
                if (version.Hash != hash)
                {
                    version.Version++;
                    version.Hash = hash;
                    version.LastModified = DateTime.Now;
                    
                    GD.Print($"Asset updated: {assetPath} (v{version.Version})");
                }
            }
            else
            {
                assetVersions[assetPath] = new AssetVersion
                {
                    Path = assetPath,
                    Hash = hash,
                    Version = 1,
                    LastModified = DateTime.Now
                };
            }
            
            SaveVersionData();
        }
        
        private string CalculateFileHash(string filePath)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            using var stream = File.OpenRead(filePath);
            byte[] hashBytes = md5.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
        
        private void SaveVersionData()
        {
            // Save to .godot/asset_versions.json
            string json = Json.Stringify(assetVersions);
            string path = ".godot/asset_versions.json";
            
            using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
            file.StoreString(json);
        }
    }

    public class AssetVersion
    {
        public string Path;
        public string Hash;
        public int Version;
        public DateTime LastModified;
    }
}
