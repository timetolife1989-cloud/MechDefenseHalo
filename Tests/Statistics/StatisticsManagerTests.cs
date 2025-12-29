using Godot;
using GdUnit4;
using MechDefenseHalo.Statistics;
using MechDefenseHalo.Items;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Statistics
{
    /// <summary>
    /// Unit tests for Statistics system
    /// </summary>
    [TestSuite]
    public class StatisticsManagerTests
    {
        private CombatStats _combat;
        private EconomyStats _economy;
        private SessionStats _session;

        [Before]
        public void Setup()
        {
            _combat = new CombatStats();
            _economy = new EconomyStats();
            _session = new SessionStats();
        }

        [After]
        public void Teardown()
        {
            _combat = null;
            _economy = null;
            _session = null;
        }

        #region CombatStats Tests

        [TestCase]
        public void CombatStats_RecordKill_IncrementsTotalKills()
        {
            // Arrange
            int initialKills = _combat.TotalKills;

            // Act
            _combat.RecordKill("Grunt");

            // Assert
            AssertInt(_combat.TotalKills).IsEqual(initialKills + 1);
        }

        [TestCase]
        public void CombatStats_RecordKill_TracksKillsByEnemyType()
        {
            // Act
            _combat.RecordKill("Grunt");
            _combat.RecordKill("Grunt");
            _combat.RecordKill("Shooter");

            // Assert
            AssertInt(_combat.KillsByEnemyType["Grunt"]).IsEqual(2);
            AssertInt(_combat.KillsByEnemyType["Shooter"]).IsEqual(1);
        }

        [TestCase]
        public void CombatStats_RecordKill_TracksKillsByWeapon()
        {
            // Act
            _combat.RecordKill("Grunt", "AssaultRifle");
            _combat.RecordKill("Shooter", "AssaultRifle");

            // Assert
            AssertInt(_combat.KillsByWeapon["AssaultRifle"]).IsEqual(2);
        }

        [TestCase]
        public void CombatStats_RecordKill_TracksDroneKills()
        {
            // Act
            _combat.RecordKill("Grunt", "Drone", true);

            // Assert
            AssertInt(_combat.DroneKills).IsEqual(1);
        }

        [TestCase]
        public void CombatStats_UpdateAccuracy_CalculatesCorrectPercentage()
        {
            // Arrange
            _combat.ShotsFired = 100;
            _combat.ShotsHit = 67;

            // Act
            _combat.UpdateAccuracy();

            // Assert
            AssertFloat(_combat.AccuracyPercentage).IsEqual(67.0f, 0.1f);
        }

        [TestCase]
        public void CombatStats_UpdateAccuracy_HandlesZeroShots()
        {
            // Arrange
            _combat.ShotsFired = 0;
            _combat.ShotsHit = 0;

            // Act
            _combat.UpdateAccuracy();

            // Assert
            AssertFloat(_combat.AccuracyPercentage).IsEqual(0.0f);
        }

        #endregion

        #region EconomyStats Tests

        [TestCase]
        public void EconomyStats_RecordItemObtained_IncrementsRarityCount()
        {
            // Act
            _economy.RecordItemObtained(ItemRarity.Rare);
            _economy.RecordItemObtained(ItemRarity.Rare);
            _economy.RecordItemObtained(ItemRarity.Common);

            // Assert
            AssertInt(_economy.ItemsByRarity[ItemRarity.Rare]).IsEqual(2);
            AssertInt(_economy.ItemsByRarity[ItemRarity.Common]).IsEqual(1);
        }

        [TestCase]
        public void EconomyStats_RecordItemObtained_TracksLegendaries()
        {
            // Act
            _economy.RecordItemObtained(ItemRarity.Legendary);
            _economy.RecordItemObtained(ItemRarity.Exotic);
            _economy.RecordItemObtained(ItemRarity.Common);

            // Assert
            AssertInt(_economy.LegendariesObtained).IsEqual(2);
        }

        #endregion

        #region SessionStats Tests

        [TestCase]
        public void SessionStats_StartNewSession_IncrementsTotalSessions()
        {
            // Arrange
            int initialSessions = _session.TotalSessions;

            // Act
            _session.StartNewSession();

            // Assert
            AssertInt(_session.TotalSessions).IsEqual(initialSessions + 1);
        }

        [TestCase]
        public void SessionStats_StartNewSession_ResetsCurrentSessionStats()
        {
            // Arrange
            _session.CurrentSessionKills = 100;
            _session.CurrentSessionWaves = 10;
            _session.CurrentSessionTime = 600f;

            // Act
            _session.StartNewSession();

            // Assert
            AssertInt(_session.CurrentSessionKills).IsEqual(0);
            AssertInt(_session.CurrentSessionWaves).IsEqual(0);
            AssertFloat(_session.CurrentSessionTime).IsEqual(0f);
        }

        [TestCase]
        public void SessionStats_UpdateSession_IncreasesPlaytime()
        {
            // Arrange
            float deltaTime = 1.0f;

            // Act
            _session.UpdateSession(deltaTime);

            // Assert
            AssertFloat(_session.TotalPlaytimeSeconds).IsGreaterEqual(deltaTime);
            AssertFloat(_session.CurrentSessionTime).IsGreaterEqual(deltaTime);
        }

        [TestCase]
        public void SessionStats_UpdateSession_UpdatesLongestSession()
        {
            // Arrange
            _session.LongestSessionSeconds = 100f;

            // Act
            _session.UpdateSession(150f);

            // Assert
            AssertFloat(_session.LongestSessionSeconds).IsGreaterEqual(150f);
        }

        #endregion

        #region SaveHandler Tests

        [TestCase]
        public void SaveHandler_SaveStatistics_CreatesFile()
        {
            // Arrange
            var combat = new CombatStats { TotalKills = 100 };
            var economy = new EconomyStats { ItemsLooted = 50 };
            var session = new SessionStats { TotalSessions = 5 };

            // Act
            bool result = StatisticsSaveHandler.SaveStatistics(combat, economy, session);

            // Assert
            AssertBool(result).IsTrue();
        }

        [TestCase]
        public void SaveHandler_LoadStatistics_ReturnsValidData()
        {
            // Arrange
            var combat = new CombatStats { TotalKills = 100, BossesDefeated = 5 };
            var economy = new EconomyStats { ItemsLooted = 50 };
            var session = new SessionStats { TotalSessions = 5 };
            
            StatisticsSaveHandler.SaveStatistics(combat, economy, session);

            // Act
            bool result = StatisticsSaveHandler.LoadStatistics(
                out var loadedCombat, 
                out var loadedEconomy, 
                out var loadedSession
            );

            // Assert
            AssertBool(result).IsTrue();
            AssertObject(loadedCombat).IsNotNull();
            AssertObject(loadedEconomy).IsNotNull();
            AssertObject(loadedSession).IsNotNull();
            AssertInt(loadedCombat.TotalKills).IsEqual(100);
            AssertInt(loadedCombat.BossesDefeated).IsEqual(5);
        }

        [TestCase]
        public void SaveHandler_ExportStatistics_Json_CreatesFile()
        {
            // Arrange
            var combat = new CombatStats { TotalKills = 100 };
            var economy = new EconomyStats { ItemsLooted = 50 };
            var session = new SessionStats { TotalSessions = 5 };

            // Act
            bool result = StatisticsSaveHandler.ExportStatistics(combat, economy, session, "json");

            // Assert
            AssertBool(result).IsTrue();
        }

        [TestCase]
        public void SaveHandler_ExportStatistics_Csv_CreatesFile()
        {
            // Arrange
            var combat = new CombatStats { TotalKills = 100 };
            var economy = new EconomyStats { ItemsLooted = 50 };
            var session = new SessionStats { TotalSessions = 5 };

            // Act
            bool result = StatisticsSaveHandler.ExportStatistics(combat, economy, session, "csv");

            // Assert
            AssertBool(result).IsTrue();
        }

        #endregion
    }
}
