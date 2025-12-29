using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MechDefenseHalo.Statistics
{
    /// <summary>
    /// Handles saving and loading of statistics data
    /// </summary>
    public class StatisticsSaveHandler
    {
        private const string LifetimeStatsFile = "user://lifetime_stats.json";
        private const string SessionStatsFile = "user://session_stats.json";
        private const int CurrentVersion = 1;

        /// <summary>
        /// Save all statistics to persistent storage
        /// </summary>
        public static bool SaveStatistics(CombatStats combat, EconomyStats economy, SessionStats session)
        {
            try
            {
                var data = new StatisticsData
                {
                    Version = CurrentVersion,
                    LastUpdated = DateTime.Now.ToString("o"),
                    Combat = combat,
                    Economy = economy,
                    Session = session
                };

                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };
                
                string json = JsonSerializer.Serialize(data, options);

                using var file = FileAccess.Open(LifetimeStatsFile, FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    GD.PrintErr($"Failed to open statistics file: {FileAccess.GetOpenError()}");
                    return false;
                }

                file.StoreString(json);
                file.Close();

                GD.Print($"Statistics saved successfully to {LifetimeStatsFile}");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to save statistics: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Load statistics from persistent storage
        /// </summary>
        public static bool LoadStatistics(out CombatStats combat, out EconomyStats economy, out SessionStats session)
        {
            combat = new CombatStats();
            economy = new EconomyStats();
            session = new SessionStats();

            try
            {
                if (!FileAccess.FileExists(LifetimeStatsFile))
                {
                    GD.Print("No statistics file found, creating new statistics");
                    return true;
                }

                using var file = FileAccess.Open(LifetimeStatsFile, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"Failed to open statistics file: {FileAccess.GetOpenError()}");
                    return false;
                }

                string json = file.GetAsText();
                file.Close();

                var options = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() }
                };

                var data = JsonSerializer.Deserialize<StatisticsData>(json, options);

                if (data == null || data.Version != CurrentVersion)
                {
                    GD.PrintErr("Invalid or outdated statistics file");
                    return false;
                }

                combat = data.Combat ?? new CombatStats();
                economy = data.Economy ?? new EconomyStats();
                session = data.Session ?? new SessionStats();

                GD.Print($"Statistics loaded successfully from {LifetimeStatsFile}");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load statistics: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Export statistics to a file for analysis
        /// </summary>
        public static bool ExportStatistics(CombatStats combat, EconomyStats economy, SessionStats session, string format = "json")
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"user://statistics_export_{timestamp}.{format}";

                if (format.ToLower() == "json")
                {
                    var data = new StatisticsData
                    {
                        Version = CurrentVersion,
                        LastUpdated = DateTime.Now.ToString("o"),
                        Combat = combat,
                        Economy = economy,
                        Session = session
                    };

                    var options = new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        Converters = { new JsonStringEnumConverter() }
                    };
                    
                    string json = JsonSerializer.Serialize(data, options);

                    using var file = FileAccess.Open(fileName, FileAccess.ModeFlags.Write);
                    if (file == null)
                    {
                        GD.PrintErr($"Failed to create export file: {FileAccess.GetOpenError()}");
                        return false;
                    }

                    file.StoreString(json);
                    file.Close();
                }
                else if (format.ToLower() == "csv")
                {
                    string csv = ConvertToCSV(combat, economy, session);
                    
                    using var file = FileAccess.Open(fileName, FileAccess.ModeFlags.Write);
                    if (file == null)
                    {
                        GD.PrintErr($"Failed to create export file: {FileAccess.GetOpenError()}");
                        return false;
                    }

                    file.StoreString(csv);
                    file.Close();
                }

                GD.Print($"Statistics exported successfully to {fileName}");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to export statistics: {e.Message}");
                return false;
            }
        }

        private static string ConvertToCSV(CombatStats combat, EconomyStats economy, SessionStats session)
        {
            var csv = "Category,Stat,Value\n";
            
            // Combat stats
            csv += $"Combat,Total Kills,{combat.TotalKills}\n";
            csv += $"Combat,Bosses Defeated,{combat.BossesDefeated}\n";
            csv += $"Combat,Death Count,{combat.DeathCount}\n";
            csv += $"Combat,Total Damage Dealt,{combat.TotalDamageDealt}\n";
            csv += $"Combat,Total Damage Taken,{combat.TotalDamageTaken}\n";
            csv += $"Combat,Accuracy Percentage,{combat.AccuracyPercentage:F2}\n";
            csv += $"Combat,Highest Wave,{combat.HighestWaveReached}\n";

            // Economy stats
            csv += $"Economy,Credits Earned,{economy.TotalCreditsEarned}\n";
            csv += $"Economy,Credits Spent,{economy.TotalCreditsSpent}\n";
            csv += $"Economy,Cores Earned,{economy.TotalCoresEarned}\n";
            csv += $"Economy,Cores Spent,{economy.TotalCoresSpent}\n";
            csv += $"Economy,Items Looted,{economy.ItemsLooted}\n";
            csv += $"Economy,Items Crafted,{economy.ItemsCrafted}\n";
            csv += $"Economy,Legendaries Obtained,{economy.LegendariesObtained}\n";

            // Session stats
            csv += $"Session,Total Playtime (hours),{session.TotalPlaytimeSeconds / 3600f:F2}\n";
            csv += $"Session,Total Sessions,{session.TotalSessions}\n";
            csv += $"Session,Daily Login Streak,{session.DailyLoginStreak}\n";

            return csv;
        }
    }

    /// <summary>
    /// Container for all statistics data
    /// </summary>
    public class StatisticsData
    {
        public int Version { get; set; }
        public string LastUpdated { get; set; }
        public CombatStats Combat { get; set; }
        public EconomyStats Economy { get; set; }
        public SessionStats Session { get; set; }
    }
}
