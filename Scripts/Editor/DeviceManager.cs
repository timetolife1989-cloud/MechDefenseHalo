#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MechDefenseHalo.Editor
{
    /// <summary>
    /// Device manager for ADB operations
    /// Handles device detection, app management, and logcat
    /// </summary>
    [Tool]
    public partial class DeviceManager : Node
    {
        /// <summary>
        /// Get list of connected Android devices via ADB
        /// </summary>
        public static async Task<List<string>> GetConnectedDevices()
        {
            var devices = new List<string>();
            
            var process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = "devices";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
            try
            {
                process.Start();
                
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    string[] lines = output.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.Contains("\tdevice"))
                        {
                            string deviceId = line.Split('\t')[0].Trim();
                            if (!string.IsNullOrEmpty(deviceId))
                            {
                                devices.Add(deviceId);
                            }
                        }
                    }
                    
                    GD.Print($"Found {devices.Count} connected device(s)");
                }
                else
                {
                    GD.PrintErr("Failed to get device list");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"GetConnectedDevices exception: {ex.Message}");
                GD.PrintErr("Make sure ADB is installed and in PATH");
            }
            
            return devices;
        }
        
        /// <summary>
        /// Clear app data for specified package
        /// </summary>
        public static async Task<bool> ClearAppData(string packageName)
        {
            var process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = $"shell pm clear {packageName}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
            try
            {
                process.Start();
                
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    GD.Print($"Cleared app data for {packageName}");
                    return true;
                }
                else
                {
                    GD.PrintErr($"Failed to clear app data: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"ClearAppData exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Show logcat output filtered for Godot logs
        /// </summary>
        public static void ShowLogcat()
        {
            var process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = "logcat -s Godot:* GodotEngine:*";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            
            try
            {
                process.Start();
                GD.Print("Logcat started. Monitor console for output.");
                
                // Note: In a real editor plugin, you'd want to handle this output
                // and display it in the editor's output panel
            }
            catch (Exception ex)
            {
                GD.PrintErr($"ShowLogcat exception: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Uninstall app from device
        /// </summary>
        public static async Task<bool> UninstallApp(string packageName)
        {
            var process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = $"uninstall {packageName}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
            try
            {
                process.Start();
                
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    GD.Print($"Uninstalled {packageName}");
                    return true;
                }
                else
                {
                    GD.PrintErr($"Failed to uninstall: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"UninstallApp exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Kill app process on device
        /// </summary>
        public static async Task<bool> KillApp(string packageName)
        {
            var process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = $"shell am force-stop {packageName}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            
            try
            {
                process.Start();
                
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    GD.Print($"Stopped {packageName}");
                    return true;
                }
                else
                {
                    GD.PrintErr($"Failed to stop app");
                    return false;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"KillApp exception: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get device info (model, OS version, etc.)
        /// </summary>
        public static async Task<Dictionary<string, string>> GetDeviceInfo()
        {
            var info = new Dictionary<string, string>();
            
            string[] properties = new string[]
            {
                "ro.product.model",
                "ro.build.version.release",
                "ro.product.manufacturer"
            };
            
            foreach (string prop in properties)
            {
                var process = new Process();
                process.StartInfo.FileName = "adb";
                process.StartInfo.Arguments = $"shell getprop {prop}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                
                try
                {
                    process.Start();
                    
                    string output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode == 0)
                    {
                        info[prop] = output.Trim();
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"Failed to get {prop}: {ex.Message}");
                }
            }
            
            return info;
        }
    }
}
#endif
