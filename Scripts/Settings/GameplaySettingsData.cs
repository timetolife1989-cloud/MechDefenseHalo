using System;

namespace MechDefenseHalo.Settings
{
    /// <summary>
    /// Gameplay settings data structure
    /// </summary>
    [Serializable]
    public class GameplaySettingsData
    {
        public bool AutoPickupItems = true;
        public bool ShowDamageNumbers = true;
        public bool ScreenShake = true;
        public float ScreenShakeIntensity = 1.0f;
        public bool ShowFPSCounter = false;
        public bool ShowPing = false;
        public string Language = "en";
    }
}
