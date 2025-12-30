using System;

namespace MechDefenseHalo.DLC
{
    /// <summary>
    /// Data structure for DLC metadata
    /// </summary>
    public class DLCData
    {
        public string Id;
        public string Name;
        public string Description;
        public string TeaserText;
        public string TeaserVideoPath;
        public string ContentPath;
        public DateTime ReleaseDate;
        public float Price;
        public string[] Features;
    }
}
