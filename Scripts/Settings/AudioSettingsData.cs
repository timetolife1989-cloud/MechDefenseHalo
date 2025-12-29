using System;

namespace MechDefenseHalo.Settings
{
    /// <summary>
    /// Audio settings data structure
    /// </summary>
    [Serializable]
    public class AudioSettingsData
    {
        public float MasterVolume = 1.0f; // 0.0 to 1.0
        public float MusicVolume = 0.8f;
        public float SFXVolume = 1.0f;
        public float UIVolume = 0.9f;
        public bool MuteMaster = false;
    }
}
