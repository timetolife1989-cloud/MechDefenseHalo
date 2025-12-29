using Godot;
using GdUnit4;
using MechDefenseHalo.Notifications;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Notifications
{
    /// <summary>
    /// Unit tests for Daily Mission System
    /// </summary>
    [TestSuite]
    public class DailyMissionTests
    {
        private MissionGenerator _generator;

        [Before]
        public void Setup()
        {
            _generator = new MissionGenerator();
            _generator._Ready();
        }

        [After]
        public void Teardown()
        {
            _generator = null;
        }

        [TestCase]
        public void GenerateMission_Easy_ShouldCreateValidMission()
        {
            // Act
            var mission = _generator.GenerateMission(Difficulty.Easy);

            // Assert
            AssertThat(mission).IsNotNull();
            AssertThat(mission.ID).IsNotEmpty();
            AssertThat(mission.Title).IsNotEmpty();
            AssertThat(mission.Description).IsNotEmpty();
            AssertThat(mission.RequiredProgress).IsGreater(0);
            AssertThat(mission.CurrentProgress).IsEqual(0);
            AssertThat(mission.IsCompleted).IsFalse();
            AssertThat(mission.IsRewardClaimed).IsFalse();
            AssertThat(mission.Rewards).IsNotEmpty();
        }

        [TestCase]
        public void GenerateMission_Medium_ShouldHaveCoreReward()
        {
            // Act
            var mission = _generator.GenerateMission(Difficulty.Medium);

            // Assert
            AssertThat(mission).IsNotNull();
            AssertThat(mission.Rewards.ContainsKey("Cores")).IsTrue();
            AssertThat(mission.Rewards["Cores"]).IsGreater(0);
        }

        [TestCase]
        public void GenerateMission_Hard_ShouldHaveHigherRewards()
        {
            // Act
            var mission = _generator.GenerateMission(Difficulty.Hard);

            // Assert
            AssertThat(mission).IsNotNull();
            AssertThat(mission.Rewards.ContainsKey("Credits")).IsTrue();
            AssertThat(mission.Rewards["Credits"]).IsGreaterEqual(800);
        }

        [TestCase]
        public void Mission_GetProgressPercentage_ShouldCalculateCorrectly()
        {
            // Arrange
            var mission = new Mission
            {
                CurrentProgress = 50,
                RequiredProgress = 100
            };

            // Act
            float percentage = mission.GetProgressPercentage();

            // Assert
            AssertThat(percentage).IsEqual(50f);
        }

        [TestCase]
        public void Mission_GetProgressPercentage_OverComplete_ShouldCapAt100()
        {
            // Arrange
            var mission = new Mission
            {
                CurrentProgress = 150,
                RequiredProgress = 100
            };

            // Act
            float percentage = mission.GetProgressPercentage();

            // Assert
            AssertThat(percentage).IsEqual(100f);
        }

        [TestCase]
        public void Mission_IsExpired_FutureDateShouldBeFalse()
        {
            // Arrange
            var mission = new Mission
            {
                ExpirationDate = System.DateTime.Now.AddHours(1)
            };

            // Act & Assert
            AssertThat(mission.IsExpired()).IsFalse();
        }

        [TestCase]
        public void Mission_IsExpired_PastDateShouldBeTrue()
        {
            // Arrange
            var mission = new Mission
            {
                ExpirationDate = System.DateTime.Now.AddHours(-1)
            };

            // Act & Assert
            AssertThat(mission.IsExpired()).IsTrue();
        }

        [TestCase]
        public void GenerateMission_ShouldCreateDifferentMissions()
        {
            // Act
            var mission1 = _generator.GenerateMission(Difficulty.Easy);
            var mission2 = _generator.GenerateMission(Difficulty.Easy);
            var mission3 = _generator.GenerateMission(Difficulty.Easy);

            // Assert - At least some should be different
            bool hasDifferentTitles = mission1.Title != mission2.Title || 
                                      mission2.Title != mission3.Title ||
                                      mission1.Title != mission3.Title;
            AssertThat(hasDifferentTitles).IsTrue();
        }

        [TestCase]
        public void Mission_Rewards_ShouldAlwaysHaveCreditsAndXP()
        {
            // Act
            var easyMission = _generator.GenerateMission(Difficulty.Easy);
            var mediumMission = _generator.GenerateMission(Difficulty.Medium);
            var hardMission = _generator.GenerateMission(Difficulty.Hard);

            // Assert
            AssertThat(easyMission.Rewards.ContainsKey("Credits")).IsTrue();
            AssertThat(easyMission.Rewards.ContainsKey("XP")).IsTrue();
            AssertThat(mediumMission.Rewards.ContainsKey("Credits")).IsTrue();
            AssertThat(mediumMission.Rewards.ContainsKey("XP")).IsTrue();
            AssertThat(hardMission.Rewards.ContainsKey("Credits")).IsTrue();
            AssertThat(hardMission.Rewards.ContainsKey("XP")).IsTrue();
        }
    }
}
