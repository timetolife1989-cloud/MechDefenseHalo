using Godot;

namespace MechDefenseHalo.Settings
{
    /// <summary>
    /// Applies audio settings to the Godot audio server
    /// </summary>
    public static class AudioSettingsApplier
    {
        public static void Apply(AudioSettingsData settings)
        {
            // Master Volume
            int masterBusIndex = AudioServer.GetBusIndex("Master");
            if (masterBusIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(masterBusIndex, LinearToDb(settings.MasterVolume));
                AudioServer.SetBusMute(masterBusIndex, settings.MuteMaster);
            }
            
            // Music Volume
            int musicBusIndex = AudioServer.GetBusIndex("Music");
            if (musicBusIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(musicBusIndex, LinearToDb(settings.MusicVolume));
            }
            
            // SFX Volume
            int sfxBusIndex = AudioServer.GetBusIndex("SFX");
            if (sfxBusIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(sfxBusIndex, LinearToDb(settings.SFXVolume));
            }
            
            // UI Volume
            int uiBusIndex = AudioServer.GetBusIndex("UI");
            if (uiBusIndex >= 0)
            {
                AudioServer.SetBusVolumeDb(uiBusIndex, LinearToDb(settings.UIVolume));
            }
            
            GD.Print($"Applied audio settings: Master={settings.MasterVolume}, Music={settings.MusicVolume}, " +
                     $"SFX={settings.SFXVolume}, UI={settings.UIVolume}, Muted={settings.MuteMaster}");
        }
        
        private static float LinearToDb(float linear)
        {
            return linear > 0 ? Mathf.LinearToDb(linear) : -80f;
        }
    }
}
