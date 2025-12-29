using System;

namespace MechDefenseHalo.Settings
{
    /// <summary>
    /// Main game settings container
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        public GraphicsSettingsData Graphics = new GraphicsSettingsData();
        public AudioSettingsData Audio = new AudioSettingsData();
        public ControlSettingsData Controls = new ControlSettingsData();
        public GameplaySettingsData Gameplay = new GameplaySettingsData();
    }
}
