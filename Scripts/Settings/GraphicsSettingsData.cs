using System;

namespace MechDefenseHalo.Settings
{
    /// <summary>
    /// Graphics settings data structure
    /// </summary>
    [Serializable]
    public class GraphicsSettingsData
    {
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;
        public bool Fullscreen = true;
        public bool VSync = true;
        public int TargetFPS = 60; // 30, 60, 120, 144, 0=Unlimited
        
        // Quality
        public QualityPreset QualityLevel = QualityPreset.High;
        public int ShadowQuality = 2; // 0=Off, 1=Low, 2=Medium, 3=High
        public int ParticleQuality = 2;
        public bool Bloom = true;
        public bool MotionBlur = false;
        public float RenderScale = 1.0f; // 0.5 to 1.5
    }
}
