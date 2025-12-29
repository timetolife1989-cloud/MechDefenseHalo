#if TOOLS
using Godot;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MechDefenseHalo.Editor
{
    /// <summary>
    /// One-Button Deploy System for rapid mobile testing iteration
    /// Provides toolbar button for automated build, deploy, and launch
    /// </summary>
    [Tool]
    public partial class OneButtonDeploy : EditorPlugin
    {
        private Button deployButton;
        
        public override void _EnterTree()
        {
            deployButton = new Button();
            deployButton.Text = "🚀 Deploy to Mobile";
            deployButton.Pressed += OnDeployPressed;
            
            AddControlToContainer(CustomControlContainer.Toolbar, deployButton);
            
            GD.Print("OneButtonDeploy plugin loaded");
        }
        
        public override void _ExitTree()
        {
            if (deployButton != null)
            {
                RemoveControlFromContainer(CustomControlContainer.Toolbar, deployButton);
                deployButton.QueueFree();
            }
            
            GD.Print("OneButtonDeploy plugin unloaded");
        }
        
        private async void OnDeployPressed()
        {
            deployButton.Text = "⏳ Building...";
            deployButton.Disabled = true;
            
            try
            {
                // Step 1: Export APK
                GD.Print("=== EXPORT STARTED ===");
                bool exportSuccess = await ExportAPK();
                
                if (!exportSuccess)
                {
                    GD.PrintErr("Export failed!");
                    return;
                }
                
                deployButton.Text = "⏳ Installing...";
                
                // Step 2: Install to device
                bool installSuccess = await InstallToDevice();
                
                if (!installSuccess)
                {
                    GD.PrintErr("Install failed!");
                    return;
                }
                
                deployButton.Text = "⏳ Launching...";
                
                // Step 3: Launch app
                await LaunchApp();
                
                GD.Print("=== DEPLOY COMPLETE ===");
                deployButton.Text = "✅ Deployed!";
                
                // Reset button after 2 seconds
                await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
                deployButton.Text = "🚀 Deploy to Mobile";
            }
            catch (Exception e)
            {
                GD.PrintErr($"Deploy error: {e.Message}");
                GD.PrintErr($"Stack trace: {e.StackTrace}");
                deployButton.Text = "❌ Failed";
                await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
                deployButton.Text = "🚀 Deploy to Mobile";
            }
            finally
            {
                deployButton.Disabled = false;
            }
        }
        
        private async Task<bool> ExportAPK()
        {
            var exportPath = "build/MechDefenseHalo.apk";
            
            GD.Print($"Exporting to: {exportPath}");
            
            // Note: Godot 4.x EditorExportPlatform API has changed
            // This is a placeholder - actual export would need to use EditorExportPlatform API
            // For now, we'll use external godot command
            var process = new Process();
            process.StartInfo.FileName = "godot";
            process.StartInfo.Arguments = $"--headless --export-release \"Android\" {exportPath}";
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
                    GD.Print("✓ Export complete");
                    return true;
                }
                else
                {
                    GD.PrintErr($"Export failed with exit code: {process.ExitCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Export exception: {ex.Message}");
                return false;
            }
        }
        
        private async Task<bool> InstallToDevice()
        {
            var process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = "install -r build/MechDefenseHalo.apk";
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
                
                GD.Print(output);
                
                if (!string.IsNullOrEmpty(error))
                    GD.PrintErr(error);
                
                if (process.ExitCode == 0)
                {
                    GD.Print("✓ Install complete");
                    return true;
                }
                else
                {
                    GD.PrintErr($"Install failed with exit code: {process.ExitCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Install exception: {ex.Message}");
                GD.PrintErr("Make sure ADB is installed and in PATH");
                return false;
            }
        }
        
        private async Task LaunchApp()
        {
            var process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = "shell am start -n com.mechdefense.halo/.GodotApp";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
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
                    GD.Print("✓ App launched");
                }
                else
                {
                    GD.PrintErr($"Launch failed with exit code: {process.ExitCode}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Launch exception: {ex.Message}");
            }
        }
    }
}
#endif
