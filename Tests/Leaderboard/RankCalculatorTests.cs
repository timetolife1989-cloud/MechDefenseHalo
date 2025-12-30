using Godot;
using GdUnit4;
using MechDefenseHalo.Leaderboard;
using System.Collections.Generic;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Leaderboard
{
    /// <summary>
    /// Unit tests for RankCalculator
    /// </summary>
    [TestSuite]
    public class RankCalculatorTests
    {
        private RankCalculator _calculator;
        private List<LeaderboardEntry> _testEntries;

        [Before]
        public void Setup()
        {
            _calculator = new RankCalculator();
            _testEntries = new List<LeaderboardEntry>();
        }

        [After]
        public void Teardown()
        {
            _calculator = null;
            _testEntries = null;
        }

        #region GetPlayerRank Tests

        [TestCase]
        public void GetPlayerRank_WithEmptyList_ReturnsNegativeOne()
        {
            // Act
            int rank = _calculator.GetPlayerRank(_testEntries, "Player1");

            // Assert
            AssertInt(rank).IsEqual(-1);
        }

        [TestCase]
        public void GetPlayerRank_WithNonExistentPlayer_ReturnsNegativeOne()
        {
            // Arrange
            _testEntries.Add(CreateEntry("Player1", 1000));
            _testEntries.Add(CreateEntry("Player2", 900));

            // Act
            int rank = _calculator.GetPlayerRank(_testEntries, "Player3");

            // Assert
            AssertInt(rank).IsEqual(-1);
        }

        [TestCase]
        public void GetPlayerRank_WithTopPlayer_ReturnsOne()
        {
            // Arrange
            _testEntries.Add(CreateEntry("Player1", 1000));
            _testEntries.Add(CreateEntry("Player2", 900));
            _testEntries.Add(CreateEntry("Player3", 800));

            // Act
            int rank = _calculator.GetPlayerRank(_testEntries, "Player1");

            // Assert
            AssertInt(rank).IsEqual(1);
        }

        [TestCase]
        public void GetPlayerRank_WithMiddlePlayer_ReturnsCorrectRank()
        {
            // Arrange
            _testEntries.Add(CreateEntry("Player1", 1000));
            _testEntries.Add(CreateEntry("Player2", 900));
            _testEntries.Add(CreateEntry("Player3", 800));

            // Act
            int rank = _calculator.GetPlayerRank(_testEntries, "Player2");

            // Assert
            AssertInt(rank).IsEqual(2);
        }

        [TestCase]
        public void GetPlayerRank_WithMultipleEntriesForSamePlayer_UsesHighestScore()
        {
            // Arrange
            _testEntries.Add(CreateEntry("Player1", 1000));
            _testEntries.Add(CreateEntry("Player2", 900));
            _testEntries.Add(CreateEntry("Player2", 500)); // Lower score for Player2
            _testEntries.Add(CreateEntry("Player3", 800));

            // Act
            int rank = _calculator.GetPlayerRank(_testEntries, "Player2");

            // Assert
            AssertInt(rank).IsEqual(2); // Should be rank 2 based on best score of 900
        }

        #endregion

        #region GetRankForScore Tests

        [TestCase]
        public void GetRankForScore_WithEmptyList_ReturnsOne()
        {
            // Act
            int rank = _calculator.GetRankForScore(_testEntries, 1000);

            // Assert
            AssertInt(rank).IsEqual(1);
        }

        [TestCase]
        public void GetRankForScore_WithHighScore_ReturnsOne()
        {
            // Arrange
            _testEntries.Add(CreateEntry("Player1", 800));
            _testEntries.Add(CreateEntry("Player2", 700));

            // Act
            int rank = _calculator.GetRankForScore(_testEntries, 1000);

            // Assert
            AssertInt(rank).IsEqual(1);
        }

        [TestCase]
        public void GetRankForScore_WithLowScore_ReturnsCorrectRank()
        {
            // Arrange
            _testEntries.Add(CreateEntry("Player1", 1000));
            _testEntries.Add(CreateEntry("Player2", 900));
            _testEntries.Add(CreateEntry("Player3", 800));

            // Act
            int rank = _calculator.GetRankForScore(_testEntries, 750);

            // Assert
            AssertInt(rank).IsEqual(4);
        }

        #endregion

        #region GetRankTier Tests

        [TestCase]
        public void GetRankTier_WithLowScore_ReturnsUnranked()
        {
            // Act
            var tier = _calculator.GetRankTier(500);

            // Assert
            AssertThat(tier).IsEqual(RankTier.Unranked);
        }

        [TestCase]
        public void GetRankTier_WithBronzeScore_ReturnsBronze()
        {
            // Act
            var tier = _calculator.GetRankTier(1500);

            // Assert
            AssertThat(tier).IsEqual(RankTier.Bronze);
        }

        [TestCase]
        public void GetRankTier_WithSilverScore_ReturnsSilver()
        {
            // Act
            var tier = _calculator.GetRankTier(3000);

            // Assert
            AssertThat(tier).IsEqual(RankTier.Silver);
        }

        [TestCase]
        public void GetRankTier_WithGoldScore_ReturnsGold()
        {
            // Act
            var tier = _calculator.GetRankTier(7500);

            // Assert
            AssertThat(tier).IsEqual(RankTier.Gold);
        }

        [TestCase]
        public void GetRankTier_WithPlatinumScore_ReturnsPlatinum()
        {
            // Act
            var tier = _calculator.GetRankTier(15000);

            // Assert
            AssertThat(tier).IsEqual(RankTier.Platinum);
        }

        [TestCase]
        public void GetRankTier_WithDiamondScore_ReturnsDiamond()
        {
            // Act
            var tier = _calculator.GetRankTier(35000);

            // Assert
            AssertThat(tier).IsEqual(RankTier.Diamond);
        }

        [TestCase]
        public void GetRankTier_WithMasterScore_ReturnsMaster()
        {
            // Act
            var tier = _calculator.GetRankTier(75000);

            // Assert
            AssertThat(tier).IsEqual(RankTier.Master);
        }

        [TestCase]
        public void GetRankTier_WithLegendScore_ReturnsLegend()
        {
            // Act
            var tier = _calculator.GetRankTier(100000);

            // Assert
            AssertThat(tier).IsEqual(RankTier.Legend);
        }

        #endregion

        #region GetPointsToNextTier Tests

        [TestCase]
        public void GetPointsToNextTier_FromUnranked_ReturnsCorrectPoints()
        {
            // Act
            int points = _calculator.GetPointsToNextTier(500);

            // Assert
            AssertInt(points).IsEqual(500); // Need 1000 for Bronze
        }

        [TestCase]
        public void GetPointsToNextTier_FromBronze_ReturnsCorrectPoints()
        {
            // Act
            int points = _calculator.GetPointsToNextTier(1500);

            // Assert
            AssertInt(points).IsEqual(1000); // Need 2500 for Silver
        }

        [TestCase]
        public void GetPointsToNextTier_FromLegend_ReturnsZero()
        {
            // Act
            int points = _calculator.GetPointsToNextTier(150000);

            // Assert
            AssertInt(points).IsEqual(0); // Already at max tier
        }

        #endregion

        #region GetTotalUniquePlayers Tests

        [TestCase]
        public void GetTotalUniquePlayers_WithEmptyList_ReturnsZero()
        {
            // Act
            int count = _calculator.GetTotalUniquePlayers(_testEntries);

            // Assert
            AssertInt(count).IsEqual(0);
        }

        [TestCase]
        public void GetTotalUniquePlayers_WithUniqueEntries_ReturnsCorrectCount()
        {
            // Arrange
            _testEntries.Add(CreateEntry("Player1", 1000));
            _testEntries.Add(CreateEntry("Player2", 900));
            _testEntries.Add(CreateEntry("Player3", 800));

            // Act
            int count = _calculator.GetTotalUniquePlayers(_testEntries);

            // Assert
            AssertInt(count).IsEqual(3);
        }

        [TestCase]
        public void GetTotalUniquePlayers_WithDuplicateEntries_ReturnsUniqueCount()
        {
            // Arrange
            _testEntries.Add(CreateEntry("Player1", 1000));
            _testEntries.Add(CreateEntry("Player2", 900));
            _testEntries.Add(CreateEntry("Player1", 800)); // Duplicate player
            _testEntries.Add(CreateEntry("Player2", 700)); // Duplicate player

            // Act
            int count = _calculator.GetTotalUniquePlayers(_testEntries);

            // Assert
            AssertInt(count).IsEqual(2); // Only 2 unique players
        }

        #endregion

        #region Helper Methods

        private LeaderboardEntry CreateEntry(string playerName, int score)
        {
            return new LeaderboardEntry
            {
                PlayerName = playerName,
                Score = score,
                Wave = 10,
                Kills = 100,
                Timestamp = System.DateTime.Now
            };
        }

        #endregion
    }
}
