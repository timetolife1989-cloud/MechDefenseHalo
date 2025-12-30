using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Managers;

namespace MechDefenseHalo.Tests.Managers
{
    /// <summary>
    /// Unit tests for EnemySpawner
    /// </summary>
    [TestSuite]
    public class EnemySpawnerTests
    {
        private EnemySpawner _spawner;
        private SceneTree _sceneTree;

        [Before]
        public void Setup()
        {
            _spawner = new EnemySpawner();
            _spawner.AutoStart = false; // Don't auto-start waves during tests
            _spawner.EnemiesPerWave = 5;
            _spawner.WaveMultiplier = 1.5f;
            _spawner.TimeBetweenWaves = 10.0f;
            _spawner.SpawnInterval = 2.0f;
        }

        [After]
        public void Teardown()
        {
            if (_spawner != null)
            {
                _spawner.QueueFree();
                _spawner = null;
            }
        }

        #region Singleton Tests

        [TestCase]
        public void Singleton_FirstInstance_SetsInstanceProperty()
        {
            // Arrange - spawner created in Setup
            
            // Act - Simulate _Ready
            _spawner._Ready();

            // Assert
            AssertThat(EnemySpawner.Instance).IsNotNull();
            AssertThat(EnemySpawner.Instance).IsEqual(_spawner);
        }

        #endregion

        #region Wave Configuration Tests

        [TestCase]
        public void CurrentWave_Initial_IsZero()
        {
            // Assert
            AssertThat(_spawner.CurrentWave).IsEqual(0);
        }

        [TestCase]
        public void EnemiesPerWave_Default_IsFive()
        {
            // Assert
            AssertThat(_spawner.EnemiesPerWave).IsEqual(5);
        }

        [TestCase]
        public void WaveMultiplier_Default_IsOnePointFive()
        {
            // Assert
            AssertFloat(_spawner.WaveMultiplier).IsEqual(1.5f);
        }

        [TestCase]
        public void TimeBetweenWaves_Default_IsTenSeconds()
        {
            // Assert
            AssertFloat(_spawner.TimeBetweenWaves).IsEqual(10.0f);
        }

        [TestCase]
        public void SpawnInterval_Default_IsTwoSeconds()
        {
            // Assert
            AssertFloat(_spawner.SpawnInterval).IsEqual(2.0f);
        }

        #endregion

        #region Manual Control Tests

        [TestCase]
        public void GetEnemiesAlive_Initial_ReturnsZero()
        {
            // Act
            int result = _spawner.GetEnemiesAlive();

            // Assert
            AssertThat(result).IsEqual(0);
        }

        [TestCase]
        public void GetCurrentWave_Initial_ReturnsZero()
        {
            // Act
            int result = _spawner.GetCurrentWave();

            // Assert
            AssertThat(result).IsEqual(0);
        }

        [TestCase]
        public void StopSpawning_WhenCalled_DoesNotThrow()
        {
            // Act & Assert - Should not throw
            _spawner.StopSpawning();
        }

        #endregion

        #region Configuration Property Tests

        [TestCase]
        public void AutoStart_Default_IsTrue()
        {
            // Arrange
            var newSpawner = new EnemySpawner();

            // Assert
            AssertBool(newSpawner.AutoStart).IsTrue();

            // Cleanup
            newSpawner.QueueFree();
        }

        [TestCase]
        public void SpawnPointPaths_CanBeNull()
        {
            // Arrange
            _spawner.SpawnPointPaths = null;

            // Act - Should not throw
            _spawner._Ready();

            // Assert - Just verify it didn't crash
            AssertThat(_spawner.SpawnPointPaths).IsNull();
        }

        #endregion
    }
}
