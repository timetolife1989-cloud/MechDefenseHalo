#if TOOLS
using Godot;
using System;
using System.IO;

namespace MechDefenseHalo.Editor
{
    [Tool]
    public partial class AssetImportPipeline : EditorPlugin
    {
        private FileSystemWatcher fileWatcher;
        private NamingConventionEnforcer namingEnforcer;
        private AssetValidator validator;
        
        public override void _EnterTree()
        {
            namingEnforcer = new NamingConventionEnforcer();
            validator = new AssetValidator();
            
            SetupFileWatcher();
            
            GD.Print("Asset Import Pipeline active");
        }
        
        public override void _ExitTree()
        {
            fileWatcher?.Dispose();
        }
        
        private void SetupFileWatcher()
        {
            string projectPath = ProjectSettings.GlobalizePath("res://");
            
            fileWatcher = new FileSystemWatcher(projectPath);
            fileWatcher.Filter = "*.*";
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
            
            fileWatcher.Created += OnFileCreated;
            fileWatcher.Changed += OnFileChanged;
            
            fileWatcher.EnableRaisingEvents = true;
        }
        
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            string extension = Path.GetExtension(e.FullPath).ToLower();
            
            // Check if asset file
            if (IsAssetFile(extension))
            {
                GD.Print($"New asset detected: {e.Name}");
                ProcessNewAsset(e.FullPath);
            }
        }
        
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // Handle asset updates
        }
        
        private bool IsAssetFile(string extension)
        {
            return extension is ".blend" or ".fbx" or ".obj" or ".png" or ".jpg" 
                or ".wav" or ".ogg" or ".mp3" or ".gltf" or ".glb";
        }
        
        private void ProcessNewAsset(string filePath)
        {
            // Validate naming convention
            if (!namingEnforcer.IsValidName(filePath))
            {
                GD.PrintErr($"Invalid naming convention: {filePath}");
                ShowNamingError(filePath);
            }
            
            // Validate asset
            if (!validator.ValidateAsset(filePath))
            {
                GD.PrintErr($"Asset validation failed: {filePath}");
            }
            
            // Apply import preset
            ApplyImportPreset(filePath);
        }
        
        private void ApplyImportPreset(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            
            if (extension == ".blend")
            {
                // Apply model import settings
                // TODO: Set import flags via ResourceImporter
            }
            else if (extension is ".png" or ".jpg")
            {
                // Apply texture import settings
            }
            else if (extension is ".wav" or ".ogg")
            {
                // Apply audio import settings
            }
        }
        
        private void ShowNamingError(string filePath)
        {
            var dialog = new AcceptDialog();
            dialog.DialogText = $"Asset naming convention violated:\n{Path.GetFileName(filePath)}\n\nExpected format: category_name_variant.ext\nExample: enemy_grunt_v01.blend";
            dialog.Title = "Naming Convention Error";
            EditorInterface.Singleton.GetBaseControl().AddChild(dialog);
            dialog.PopupCentered();
        }
    }
}
#endif
