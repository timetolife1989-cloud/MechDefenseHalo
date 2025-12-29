using Godot;

namespace MechDefenseHalo.Settings
{
    /// <summary>
    /// Applies graphics settings to the Godot engine
    /// </summary>
    public static class GraphicsSettingsApplier
    {
        public static void Apply(GraphicsSettingsData settings)
        {
            // Resolution
            if (settings.Fullscreen)
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            }
            else
            {
                DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
                DisplayServer.WindowSetSize(new Vector2I(settings.ResolutionWidth, settings.ResolutionHeight));
            }
            
            // VSync
            DisplayServer.WindowSetVsyncMode(
                settings.VSync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled
            );
            
            // FPS Limit
            Engine.MaxFps = settings.TargetFPS == 0 ? 0 : settings.TargetFPS;
            
            // Quality Preset
            ApplyQualityPreset(settings.QualityLevel);
            
            // Individual settings
            if (settings.ShadowQuality >= 0)
            {
                RenderingServer.EnvironmentSetSsaoQuality(
                    settings.ShadowQuality >= 2 
                        ? RenderingServer.EnvironmentSsaoQuality.High 
                        : RenderingServer.EnvironmentSsaoQuality.Low
                );
            }
            
            GD.Print($"Applied graphics settings: {settings.ResolutionWidth}x{settings.ResolutionHeight}, " +
                     $"Fullscreen={settings.Fullscreen}, VSync={settings.VSync}, Quality={settings.QualityLevel}");
        }
        
        private static void ApplyQualityPreset(QualityPreset preset)
        {
            switch (preset)
            {
                case QualityPreset.Low:
                    SetQualitySetting("rendering/quality/shadows/soft_shadow_quality", 0);
                    SetQualitySetting("rendering/quality/reflections/max_reflection_probes", 8);
                    break;
                case QualityPreset.Medium:
                    SetQualitySetting("rendering/quality/shadows/soft_shadow_quality", 1);
                    SetQualitySetting("rendering/quality/reflections/max_reflection_probes", 16);
                    break;
                case QualityPreset.High:
                    SetQualitySetting("rendering/quality/shadows/soft_shadow_quality", 2);
                    SetQualitySetting("rendering/quality/reflections/max_reflection_probes", 32);
                    break;
                case QualityPreset.Ultra:
                    SetQualitySetting("rendering/quality/shadows/soft_shadow_quality", 3);
                    SetQualitySetting("rendering/quality/reflections/max_reflection_probes", 64);
                    break;
            }
        }
        
        private static void SetQualitySetting(string setting, int value)
        {
            if (ProjectSettings.HasSetting(setting))
            {
                ProjectSettings.SetSetting(setting, value);
            }
        }
    }
}
