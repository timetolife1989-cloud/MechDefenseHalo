using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Settings
{
    /// <summary>
    /// Control settings data structure
    /// </summary>
    [Serializable]
    public class ControlSettingsData
    {
        public Dictionary<string, int> KeyBindings = new Dictionary<string, int>();
        public float MouseSensitivity = 1.0f;
        public bool InvertY = false;
        public float ControllerSensitivity = 1.0f;
        public int ControllerDeadzone = 15; // 0-100%
    }
}
