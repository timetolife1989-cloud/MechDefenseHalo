using Godot;
using GdUnit4;
using MechDefenseHalo.Core;
using MechDefenseHalo.SaveSystem;
using static GdUnit4.Assertions;
using System;

namespace MechDefenseHalo.Tests.SaveSystem
{
    /// <summary>
    /// Integration tests for SaveManager
    /// Tests complete save/load cycle
    /// </summary>
    [TestSuite]
    public class SaveManagerTests
    {
        private SaveManager _saveManager;

        [Before]
        public void Setup()
        {
            _saveManager = new SaveManager();
            _saveManager._Ready();
        }

        [After]
        public void Teardown()
        {
            if (_saveManager != null)
            {
                _saveManager.QueueFree();
                _saveManager = null;
            }
        }

        [TestCase]
        public void SaveManager_AfterReady_ShouldHaveInstance()
        {
            // Assert
            AssertThat(SaveManager.Instance).IsNotNull();
        }

        [TestCase]
        public void SaveGame_ShouldReturnTrue()
        {
            // Act
            bool result = _saveManager.SaveGame();

            // Assert
            AssertThat(result).IsTrue();
        }

        [TestCase]
        public void LoadGame_AfterSave_ShouldRestoreData()
        {
            // Arrange
            _saveManager.SaveGame();

            // Act
            bool result = _saveManager.LoadGame();

            // Assert
            AssertThat(result).IsTrue();
            AssertThat(_saveManager.CurrentSaveData).IsNotNull();
        }

        [TestCase]
        public void SaveLoad_ShouldPreservePlayerData()
        {
            // Arrange - Create save data with specific values
            var saveData = new MechDefenseHalo.SaveSystem.SaveData
            {
                Version = "1.0.0",
                Player = new PlayerSaveData
                {
                    Level = 5,
                    CurrentXP = 250,
                    MaxHP = 1500f,
                    CurrentHP = 1000f
                }
            };
            _saveManager.CurrentSaveData = saveData;

            // Act
            _saveManager.SaveGame();
            _saveManager.LoadGame();

            // Assert
            AssertThat(_saveManager.CurrentSaveData).IsNotNull();
            AssertThat(_saveManager.CurrentSaveData.Player).IsNotNull();
            AssertThat(_saveManager.CurrentSaveData.Player.Level).IsEqual(5);
            AssertThat(_saveManager.CurrentSaveData.Player.CurrentXP).IsEqual(250);
        }

        [TestCase]
        public void SaveLoad_ShouldUpdateLastSavedTime()
        {
            // Arrange
            DateTime beforeSave = DateTime.Now;

            // Act
            _saveManager.SaveGame();
            _saveManager.LoadGame();

            // Assert
            AssertThat(_saveManager.CurrentSaveData.LastSaved).IsGreaterEqualThan(beforeSave);
        }

        [TestCase]
        public void DeleteSave_ShouldCreateNewSave()
        {
            // Arrange
            _saveManager.SaveGame();

            // Act
            _saveManager.DeleteSave();

            // Assert
            AssertThat(_saveManager.CurrentSaveData).IsNotNull();
            AssertThat(_saveManager.CurrentSaveData.Player).IsNotNull();
            AssertThat(_saveManager.CurrentSaveData.Player.Level).IsEqual(1); // Default level
        }

        [TestCase]
        public void CurrentPlayerData_ShouldBeAccessible()
        {
            // Assert
            AssertThat(_saveManager.CurrentPlayerData).IsNotNull();
        }

        [TestCase]
        public void SaveGame_MultipleTimes_ShouldNotFail()
        {
            // Act
            bool result1 = _saveManager.SaveGame();
            bool result2 = _saveManager.SaveGame();
            bool result3 = _saveManager.SaveGame();

            // Assert
            AssertThat(result1).IsTrue();
            AssertThat(result2).IsTrue();
            AssertThat(result3).IsTrue();
        }
    }
}
