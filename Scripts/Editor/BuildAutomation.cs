#if TOOLS
using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MechDefenseHalo.Editor
{
    /// <summary>
    /// Build automation system for multi-platform exports
    /// Supports automated builds for Android, Windows, and Linux
    /// </summary>
    [Tool]
    public partial class BuildAutomation : Node
    {
        /// <summary>
        /// Automatically export project for multiple platforms
        /// </summary>
        public static async Task<bool> AutoExport()
        {
            string[] platforms = new string[] 
            { 
                "Android", 
                "Windows Desktop", 
                "Linux/X11" 
            };
            
            bool allSucceeded = true;
            
            foreach (string platform in platforms)
            {
                string platformSafe = platform.Replace("/", "_").Replace(" ", "_");
                string outputPath = $"builds/{platformSafe}/MechDefenseHalo";
                
                GD.Print($"Exporting for {platform}...");
                
                bool success = await ExportPlatform(platform, outputPath);
                
                if (success)
                {
                    GD.Print($"✓ {platform} export complete");
                }
                else
                {
                    GD.PrintErr($"✗ {platform} export failed");
                    allSucceeded = false;
                }
            }
            
            return allSucceeded;
        }
        
        /// <summary>
        /// Export project for a specific platform
        /// </summary>
        private static async Task<bool> ExportPlatform(string platform, string outputPath)
        {
            var process = new Process();
            process.StartInfo.FileName = GetGodotExecutable();
            process.StartInfo.Arguments = $"--headless --export-release \"{platform}\" {outputPath}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = ProjectSettings.GlobalizePath("res://");
            
            try
            {
                process.Start();
                
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();
                
                if (!string.IsNullOrEmpty(output))
                    GD.Print(output);
                
                if (!string.IsNullOrEmpty(error))
                    GD.PrintErr(error);
                
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Export exception for {platform}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Export APK for Android with debug or release configuration
        /// </summary>
        public static async Task<bool> ExportAndroid(bool debug = true)
        {
            string buildType = debug ? "debug" : "release";
            string outputPath = System.IO.Path.Combine("build", $"MechDefenseHalo_{buildType}.apk");
            string exportArg = debug ? "--export-debug" : "--export-release";
            
            GD.Print($"Exporting Android ({buildType})...");
            
            var process = new Process();
            process.StartInfo.FileName = GetGodotExecutable();
            process.StartInfo.Arguments = $"--headless {exportArg} \"Android\" {outputPath}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = ProjectSettings.GlobalizePath("res://");
            
            try
            {
                process.Start();
                
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();
                
                if (!string.IsNullOrEmpty(output))
                    GD.Print(output);
                
                if (!string.IsNullOrEmpty(error))
                    GD.PrintErr(error);
                
                if (process.ExitCode == 0)
                {
                    GD.Print($"✓ Android {buildType} export complete: {outputPath}");
                    return true;
                }
                else
                {
                    GD.PrintErr($"✗ Android {buildType} export failed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Export exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Clean build directories
        /// </summary>
        public static void CleanBuilds()
        {
            string buildPath = ProjectSettings.GlobalizePath("res://build");
            string buildsPath = ProjectSettings.GlobalizePath("res://builds");
            
            try
            {
                if (System.IO.Directory.Exists(buildPath))
                {
                    System.IO.Directory.Delete(buildPath, true);
                    GD.Print("✓ Cleaned build directory");
                }
                
                if (System.IO.Directory.Exists(buildsPath))
                {
                    System.IO.Directory.Delete(buildsPath, true);
                    GD.Print("✓ Cleaned builds directory");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Clean failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get the correct Godot executable name based on platform
        /// </summary>
        private static string GetGodotExecutable()
        {
            if (OS.HasFeature("windows"))
            {
                return "godot.exe";
            }
            return "godot";
        }
    }
}
#endif
