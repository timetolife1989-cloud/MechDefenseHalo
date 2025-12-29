using Godot;
using GdUnit4;
using MechDefenseHalo.Hangar;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Hangar
{
    /// <summary>
    /// Unit tests for EnemyCodex system
    /// </summary>
    [TestSuite]
    public class EnemyCodexTests
    {
        private EnemyCodex _codex;

        [Before]
        public void Setup()
        {
            _codex = new EnemyCodex();
            _codex._Ready();
        }

        [After]
        public void Teardown()
        {
            _codex._ExitTree();
            _codex = null;
        }

        [TestCase]
        public void GetEnemyData_WithValidId_ShouldReturnData()
        {
            // Act
            var enemyData = _codex.GetEnemyData("Grunt");

            // Assert
            AssertObject(enemyData).IsNotNull();
            AssertString(enemyData.Name).IsEqual("Grunt");
            AssertString(enemyData.Id).IsEqual("Grunt");
        }

        [TestCase]
        public void GetEnemyData_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var enemyData = _codex.GetEnemyData("NonExistent");

            // Assert
            AssertObject(enemyData).IsNull();
        }

        [TestCase]
        public void GetAllEntries_ShouldReturnAllEnemies()
        {
            // Act
            var entries = _codex.GetAllEntries();

            // Assert
            AssertObject(entries).IsNotNull();
            AssertInt(entries.Count).IsGreaterEqual(5); // At least 5 enemy types
        }

        [TestCase]
        public void GetUnlockedEntries_Initially_ShouldBeEmpty()
        {
            // Act
            var unlockedEntries = _codex.GetUnlockedEntries();

            // Assert
            AssertObject(unlockedEntries).IsNotNull();
            AssertInt(unlockedEntries.Count).IsEqual(0);
        }

        [TestCase]
        public void EnemyData_ShouldHaveCorrectDefaultValues()
        {
            // Act
            var gruntData = _codex.GetEnemyData("Grunt");

            // Assert
            AssertInt(gruntData.HP).IsGreater(0);
            AssertInt(gruntData.Damage).IsGreater(0);
            AssertFloat(gruntData.Speed).IsGreater(0);
            AssertString(gruntData.Description).IsNotEmpty();
            AssertBool(gruntData.IsUnlocked).IsFalse();
            AssertInt(gruntData.KillCount).IsEqual(0);
        }

        [TestCase]
        public void EnemyTypes_ShouldHaveDifferentStats()
        {
            // Arrange & Act
            var grunt = _codex.GetEnemyData("Grunt");
            var tank = _codex.GetEnemyData("Tank");

            // Assert - Tank should have more HP than Grunt
            AssertInt(tank.HP).IsGreater(grunt.HP);
        }
    }
}
