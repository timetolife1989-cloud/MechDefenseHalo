using Godot;
using GdUnit4;
using MechDefenseHalo.Leaderboard;
using System.Collections.Generic;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Leaderboard
{
    /// <summary>
    /// Unit tests for LeaderboardManager
    /// Note: These are focused on data structure tests.
    /// Full integration tests require EventBus and game scene.
    /// </summary>
    [TestSuite]
    public class LeaderboardManagerTests
    {
        [TestCase]
        public void LeaderboardEntry_CanBeCreated()
        {
            // Arrange & Act
            var entry = new LeaderboardEntry
            {
                PlayerName = "TestPlayer",
                Score = 1000,
                Wave = 10,
                Kills = 50,
                Timestamp = System.DateTime.Now
            };

            // Assert
            AssertString(entry.PlayerName).IsEqual("TestPlayer");
            AssertInt(entry.Score).IsEqual(1000);
            AssertInt(entry.Wave).IsEqual(10);
            AssertInt(entry.Kills).IsEqual(50);
        }

        [TestCase]
        public void LeaderboardSaveData_CanBeCreated()
        {
            // Arrange & Act
            var saveData = new LeaderboardSaveData
            {
                Entries = new List<LeaderboardEntry>(),
                LastSaved = System.DateTime.Now
            };

            // Assert
            AssertObject(saveData.Entries).IsNotNull();
            AssertThat(saveData.LastSaved).IsNotNull();
        }

        [TestCase]
        public void TimePeriod_HasAllValues()
        {
            // Verify all enum values exist
            var daily = TimePeriod.Daily;
            var weekly = TimePeriod.Weekly;
            var monthly = TimePeriod.Monthly;
            var allTime = TimePeriod.AllTime;

            AssertThat(daily).IsEqual(TimePeriod.Daily);
            AssertThat(weekly).IsEqual(TimePeriod.Weekly);
            AssertThat(monthly).IsEqual(TimePeriod.Monthly);
            AssertThat(allTime).IsEqual(TimePeriod.AllTime);
        }
    }
}
