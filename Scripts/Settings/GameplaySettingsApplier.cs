using Godot;

namespace MechDefenseHalo.Settings
{
    /// <summary>
    /// Applies gameplay settings
    /// </summary>
    public static class GameplaySettingsApplier
    {
        // Global storage for runtime values
        public static bool AutoPickupItems { get; set; } = true;
        public static bool ShowDamageNumbers { get; set; } = true;
        public static bool ScreenShake { get; set; } = true;
        public static float ScreenShakeIntensity { get; set; } = 1.0f;
        public static bool ShowFPSCounter { get; set; } = false;
        public static bool ShowPing { get; set; } = false;
        public static string Language { get; set; } = "en";
        
        public static void Apply(GameplaySettingsData settings)
        {
            AutoPickupItems = settings.AutoPickupItems;
            ShowDamageNumbers = settings.ShowDamageNumbers;
            ScreenShake = settings.ScreenShake;
            ScreenShakeIntensity = settings.ScreenShakeIntensity;
            ShowFPSCounter = settings.ShowFPSCounter;
            ShowPing = settings.ShowPing;
            Language = settings.Language;
            
            GD.Print($"Applied gameplay settings: AutoPickup={settings.AutoPickupItems}, " +
                     $"DamageNumbers={settings.ShowDamageNumbers}, ScreenShake={settings.ScreenShake}, " +
                     $"Language={settings.Language}");
        }
    }
}
