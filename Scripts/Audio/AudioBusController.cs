using Godot;
using System;

namespace MechDefenseHalo.Audio
{
    /// <summary>
    /// Manages audio bus configuration and ensures proper bus hierarchy.
    /// </summary>
    public partial class AudioBusController : Node
    {
        public override void _Ready()
        {
            SetupAudioBuses();
            GD.Print("AudioBusController initialized successfully");
        }
        
        private void SetupAudioBuses()
        {
            // Ensure buses exist: Master → Music, SFX, UI
            EnsureBusExists("Music", "Master", 1);
            EnsureBusExists("SFX", "Master", 2);
            EnsureBusExists("UI", "Master", 3);
            
            GD.Print("Audio buses configured: Master, Music, SFX, UI");
        }
        
        private void EnsureBusExists(string busName, string parentBusName, int preferredIndex)
        {
            int busIndex = AudioServer.GetBusIndex(busName);
            
            if (busIndex == -1)
            {
                // Bus doesn't exist, create it
                int busCount = AudioServer.BusCount;
                
                // Add bus at preferred index if possible, otherwise at the end
                if (preferredIndex < busCount)
                {
                    AudioServer.AddBus(preferredIndex);
                }
                else
                {
                    AudioServer.AddBus(busCount);
                }
                
                busIndex = AudioServer.GetBusIndex(busName);
                if (busIndex == -1)
                {
                    // If still not found, get the last added bus
                    busIndex = AudioServer.BusCount - 1;
                }
                
                AudioServer.SetBusName(busIndex, busName);
                AudioServer.SetBusSend(busIndex, parentBusName);
                
                GD.Print($"Created audio bus: {busName} -> {parentBusName}");
            }
            else
            {
                // Bus exists, ensure parent is correct
                string currentParent = AudioServer.GetBusSend(busIndex);
                if (currentParent != parentBusName)
                {
                    AudioServer.SetBusSend(busIndex, parentBusName);
                    GD.Print($"Updated audio bus parent: {busName} -> {parentBusName}");
                }
            }
        }
        
        /// <summary>
        /// Get the index of an audio bus by name.
        /// </summary>
        public int GetBusIndex(string busName)
        {
            return AudioServer.GetBusIndex(busName);
        }
        
        /// <summary>
        /// Set the volume of a specific bus in decibels.
        /// </summary>
        public void SetBusVolume(string busName, float volumeDb)
        {
            int busIndex = AudioServer.GetBusIndex(busName);
            if (busIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(busIndex, volumeDb);
            }
            else
            {
                GD.PrintErr($"Audio bus not found: {busName}");
            }
        }
        
        /// <summary>
        /// Get the volume of a specific bus in decibels.
        /// </summary>
        public float GetBusVolume(string busName)
        {
            int busIndex = AudioServer.GetBusIndex(busName);
            if (busIndex >= 0)
            {
                return AudioServer.GetBusVolumeDb(busIndex);
            }
            
            GD.PrintErr($"Audio bus not found: {busName}");
            return 0f;
        }
        
        /// <summary>
        /// Mute or unmute a specific bus.
        /// </summary>
        public void SetBusMute(string busName, bool mute)
        {
            int busIndex = AudioServer.GetBusIndex(busName);
            if (busIndex >= 0)
            {
                AudioServer.SetBusMute(busIndex, mute);
            }
            else
            {
                GD.PrintErr($"Audio bus not found: {busName}");
            }
        }
    }
}
