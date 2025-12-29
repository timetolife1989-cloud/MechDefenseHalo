using Godot;
using GdUnit4;
using MechDefenseHalo.Progression;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Progression
{
    /// <summary>
    /// Unit tests for XPCurve system
    /// Tests XP calculations and level requirements
    /// </summary>
    [TestSuite]
    public class XPCurveTests
    {
        [TestCase]
        public void Level1_RequiresZeroXP()
        {
            // Act
            int xp = XPCurve.GetXPRequired(1);

            // Assert
            AssertInt(xp).IsEqual(0);
        }

        [TestCase]
        public void Level2_Requires100XP()
        {
            // Act
            int xp = XPCurve.GetXPRequired(2);

            // Assert
            AssertInt(xp).IsEqual(100);
        }

        [TestCase]
        public void Level10_LinearProgression()
        {
            // Level 1-10 is linear at 100 XP per level
            // Level 10 = 9 * 100 = 900 XP
            // Act
            int xp = XPCurve.GetXPRequired(10);

            // Assert
            AssertInt(xp).IsEqual(900);
        }

        [TestCase]
        public void GetXPForNextLevel_Level1_Returns100()
        {
            // Act
            int xp = XPCurve.GetXPForNextLevel(1);

            // Assert
            AssertInt(xp).IsEqual(100);
        }

        [TestCase]
        public void GetXPForNextLevel_Level2_Returns100()
        {
            // Act
            int xp = XPCurve.GetXPForNextLevel(2);

            // Assert
            AssertInt(xp).IsEqual(100);
        }

        [TestCase]
        public void GetXPForNextLevel_Level100_ReturnsZero()
        {
            // Act
            int xp = XPCurve.GetXPForNextLevel(100);

            // Assert
            AssertInt(xp).IsEqual(0);
        }

        [TestCase]
        public void GetXPRequired_Level11_IsGreaterThan1000()
        {
            // Level 11 starts exponential growth
            // Act
            int xp = XPCurve.GetXPRequired(11);

            // Assert
            AssertInt(xp).IsGreater(1000);
        }

        [TestCase]
        public void GetXPRequired_Level100_IsPositive()
        {
            // Act
            int xp = XPCurve.GetXPRequired(100);

            // Assert
            AssertInt(xp).IsGreater(0);
        }

        [TestCase]
        public void GetXPRequired_Ascending_IncreasesMonotonically()
        {
            // Each level should require more total XP than previous
            // Act & Assert
            for (int level = 2; level <= 100; level++)
            {
                int currentXP = XPCurve.GetXPRequired(level);
                int previousXP = XPCurve.GetXPRequired(level - 1);
                
                AssertInt(currentXP).IsGreater(previousXP);
            }
        }

        [TestCase]
        public void GetLevelFromXP_ZeroXP_ReturnsLevel1()
        {
            // Act
            int level = XPCurve.GetLevelFromXP(0);

            // Assert
            AssertInt(level).IsEqual(1);
        }

        [TestCase]
        public void GetLevelFromXP_100XP_ReturnsLevel2()
        {
            // Act
            int level = XPCurve.GetLevelFromXP(100);

            // Assert
            AssertInt(level).IsEqual(2);
        }

        [TestCase]
        public void GetLevelFromXP_900XP_ReturnsLevel10()
        {
            // Act
            int level = XPCurve.GetLevelFromXP(900);

            // Assert
            AssertInt(level).IsEqual(10);
        }

        [TestCase]
        public void GetLevelFromXP_LargeAmount_ReturnsCorrectLevel()
        {
            // Act
            int level50XP = XPCurve.GetXPRequired(50);
            int level = XPCurve.GetLevelFromXP(level50XP);

            // Assert
            AssertInt(level).IsEqual(50);
        }

        [TestCase]
        public void GetXPForNextLevel_ConsecutiveLevels_MatchesDifference()
        {
            // The XP for next level should match the difference between level requirements
            for (int level = 1; level < 100; level++)
            {
                int xpForNext = XPCurve.GetXPForNextLevel(level);
                int totalToCurrent = XPCurve.GetXPRequired(level);
                int totalToNext = XPCurve.GetXPRequired(level + 1);
                int difference = totalToNext - totalToCurrent;

                AssertInt(xpForNext).IsEqual(difference);
            }
        }
    }
}
