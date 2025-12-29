using Godot;
using GdUnit4;
using MechDefenseHalo.SaveSystem;
using static GdUnit4.Assertions;
using System;

namespace MechDefenseHalo.Tests.SaveSystem
{
    /// <summary>
    /// Unit tests for SaveMigration
    /// Tests version compatibility and migration functionality
    /// </summary>
    [TestSuite]
    public class SaveMigrationTests
    {
        [TestCase]
        public void MigrateToCurrentVersion_WithCurrentVersion_ShouldReturnSameData()
        {
            // Arrange
            var saveData = new SaveData
            {
                Version = "1.0.0",
                Player = new PlayerSaveData { Level = 5 }
            };

            // Act
            var migrated = SaveMigration.MigrateToCurrentVersion(saveData);

            // Assert
            AssertThat(migrated).IsNotNull();
            AssertThat(migrated.Version).IsEqual("1.0.0");
            AssertThat(migrated.Player.Level).IsEqual(5);
        }

        [TestCase]
        public void MigrateToCurrentVersion_WithNullData_ShouldReturnNull()
        {
            // Act
            var migrated = SaveMigration.MigrateToCurrentVersion(null);

            // Assert
            AssertThat(migrated).IsNull();
        }

        [TestCase]
        public void IsVersionCompatible_WithValidVersion_ShouldReturnTrue()
        {
            // Act
            bool compatible = SaveMigration.IsVersionCompatible("1.0.0");

            // Assert
            AssertThat(compatible).IsTrue();
        }

        [TestCase]
        public void IsVersionCompatible_WithNullVersion_ShouldReturnFalse()
        {
            // Act
            bool compatible = SaveMigration.IsVersionCompatible(null);

            // Assert
            AssertThat(compatible).IsFalse();
        }

        [TestCase]
        public void IsVersionCompatible_WithEmptyVersion_ShouldReturnFalse()
        {
            // Act
            bool compatible = SaveMigration.IsVersionCompatible("");

            // Assert
            AssertThat(compatible).IsFalse();
        }

        [TestCase]
        public void MigrateToCurrentVersion_ShouldPreservePlayerData()
        {
            // Arrange
            var saveData = new SaveData
            {
                Version = "1.0.0",
                Player = new PlayerSaveData
                {
                    Level = 10,
                    CurrentXP = 500,
                    MaxHP = 1500f,
                    CurrentHP = 1200f
                }
            };

            // Act
            var migrated = SaveMigration.MigrateToCurrentVersion(saveData);

            // Assert
            AssertThat(migrated.Player).IsNotNull();
            AssertThat(migrated.Player.Level).IsEqual(10);
            AssertThat(migrated.Player.CurrentXP).IsEqual(500);
            AssertThat(migrated.Player.MaxHP).IsEqual(1500f);
            AssertThat(migrated.Player.CurrentHP).IsEqual(1200f);
        }

        [TestCase]
        public void MigrateToCurrentVersion_ShouldPreserveCurrencyData()
        {
            // Arrange
            var saveData = new SaveData
            {
                Version = "1.0.0",
                Currency = new CurrencySaveData
                {
                    Credits = 1000,
                    Cores = 50
                }
            };

            // Act
            var migrated = SaveMigration.MigrateToCurrentVersion(saveData);

            // Assert
            AssertThat(migrated.Currency).IsNotNull();
            AssertThat(migrated.Currency.Credits).IsEqual(1000);
            AssertThat(migrated.Currency.Cores).IsEqual(50);
        }

        [TestCase]
        public void MigrateToCurrentVersion_ShouldUpdateVersion()
        {
            // Arrange
            var saveData = new SaveData
            {
                Version = "0.9.0"
            };

            // Act
            var migrated = SaveMigration.MigrateToCurrentVersion(saveData);

            // Assert
            AssertThat(migrated.Version).IsEqual("1.0.0");
        }
    }
}
