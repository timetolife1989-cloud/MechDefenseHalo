using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.DLC
{
    /// <summary>
    /// Manages DLC content system with teaser/hype building
    /// </summary>
    public partial class DLCManager : Node
    {
        #region Singleton
        
        private static DLCManager instance;
        public static DLCManager Instance => instance;
        
        #endregion
        
        #region Private Fields
        
        private Dictionary<string, DLCData> availableDLCs = new();
        private HashSet<string> unlockedDLCs = new();
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            instance = this;
            
            RegisterDLCs();
            LoadUnlockedDLCs();
            
            GD.Print("DLCManager initialized with 3 DLCs");
        }
        
        public override void _ExitTree()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        #endregion
        
        #region DLC Registration
        
        private void RegisterDLCs()
        {
            // DLC 1: Hell Descent
            availableDLCs["hell_descent"] = new DLCData
            {
                Id = "hell_descent",
                Name = "Hell Descent",
                Description = "Enter the vortex portal and descend into the underworld. Face corrupted mechanized demons in a hellish landscape.",
                TeaserText = "The portal opens... Are you ready to face what lies beyond?",
                TeaserVideoPath = "res://DLC/DLC1_HellDescent/teaser_trailer.mp4",
                ContentPath = "res://DLC/DLC1_HellDescent/content.pck",
                ReleaseDate = new DateTime(2026, 6, 15),
                Price = 9.99f,
                Features = new[]
                {
                    "10 new Hell-themed waves",
                    "5 new demon enemy types",
                    "2 new boss fights",
                    "Hell environment with lava and fire",
                    "Dead Robot player skin",
                    "Cursed weapon tier"
                }
            };
            
            // DLC 2: Void Nexus
            availableDLCs["void_nexus"] = new DLCData
            {
                Id = "void_nexus",
                Name = "Void Nexus",
                Description = "Journey into the cosmic void where reality breaks down. Face eldritch horrors from beyond.",
                TeaserText = "The void calls... Will you answer?",
                TeaserVideoPath = "res://DLC/DLC2_VoidNexus/teaser_trailer.mp4",
                ContentPath = "res://DLC/DLC2_VoidNexus/content.pck",
                ReleaseDate = new DateTime(2026, 9, 20),
                Price = 9.99f,
                Features = new[]
                {
                    "Cosmic horror themed content",
                    "Reality-bending mechanics",
                    "5 void creature types",
                    "2 eldritch bosses",
                    "Void mech skin",
                    "Quantum weapons"
                }
            };
            
            // DLC 3: Apex Protocol
            availableDLCs["apex_protocol"] = new DLCData
            {
                Id = "apex_protocol",
                Name = "Apex Protocol",
                Description = "PvP arena mode. Prove your superiority against other mechs in the Apex combat tournament.",
                TeaserText = "Only the strongest survive the Protocol.",
                TeaserVideoPath = "res://DLC/DLC3_ApexProtocol/teaser_trailer.mp4",
                ContentPath = "res://DLC/DLC3_ApexProtocol/content.pck",
                ReleaseDate = new DateTime(2026, 12, 10),
                Price = 12.99f,
                Features = new[]
                {
                    "PvP arena mode",
                    "Ranked matchmaking",
                    "5 arena maps",
                    "Tournament system",
                    "Exclusive PvP rewards",
                    "Spectator mode"
                }
            };
        }
        
        #endregion
        
        #region Public Methods
        
        public bool IsDLCUnlocked(string dlcId)
        {
            return unlockedDLCs.Contains(dlcId);
        }
        
        public void UnlockDLC(string dlcId)
        {
            if (!unlockedDLCs.Contains(dlcId))
            {
                unlockedDLCs.Add(dlcId);
                SaveUnlockedDLCs();
                
                // Load DLC content
                LoadDLCContent(dlcId);
                
                EventBus.Emit(EventBus.DLCUnlocked, dlcId);
                GD.Print($"DLC unlocked: {dlcId}");
            }
        }
        
        public DLCData GetDLCData(string dlcId)
        {
            return availableDLCs.GetValueOrDefault(dlcId);
        }
        
        public List<DLCData> GetAllDLCs()
        {
            return availableDLCs.Values.ToList();
        }
        
        public List<DLCData> GetUpcomingDLCs()
        {
            return availableDLCs.Values
                .Where(dlc => !IsDLCUnlocked(dlc.Id) && dlc.ReleaseDate > DateTime.Now)
                .OrderBy(dlc => dlc.ReleaseDate)
                .ToList();
        }
        
        #endregion
        
        #region Private Methods
        
        private void LoadDLCContent(string dlcId)
        {
            if (availableDLCs.TryGetValue(dlcId, out var dlc))
            {
                var loader = GetNodeOrNull<DLCLoader>("DLCLoader");
                if (loader != null)
                {
                    loader.LoadContent(dlc.ContentPath);
                }
                else
                {
                    GD.PrintErr("DLCLoader not found as child of DLCManager");
                }
            }
        }
        
        private void SaveUnlockedDLCs()
        {
            SaveManager.SetValue("unlocked_dlcs", string.Join(",", unlockedDLCs));
        }
        
        private void LoadUnlockedDLCs()
        {
            string data = SaveManager.GetString("unlocked_dlcs", "");
            
            if (!string.IsNullOrEmpty(data))
            {
                unlockedDLCs = new HashSet<string>(data.Split(','));
                GD.Print($"Loaded {unlockedDLCs.Count} unlocked DLCs");
            }
        }
        
        #endregion
    }
}
