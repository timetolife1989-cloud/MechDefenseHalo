using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.WaveSystem;

namespace MechDefenseHalo.Tests.WaveSystem
{
    /// <summary>
    /// Unit tests for SpawnPoint
    /// </summary>
    [TestSuite]
    public class SpawnPointTests
    {
        private SpawnPoint _spawnPoint;

        [Before]
        public void Setup()
        {
            _spawnPoint = new SpawnPoint();
            _spawnPoint.SpawnRadius = 20f;
            _spawnPoint.RandomSpawnRadius = 5f;
            _spawnPoint.GlobalPosition = Vector3.Zero;
        }

        [After]
        public void Teardown()
        {
            _spawnPoint = null;
        }

        #region Circle Pattern Tests

        [TestCase]
        public void GetSpawnPosition_CirclePattern_FirstEnemy_ReturnsCorrectPosition()
        {
            // Arrange
            var pattern = SpawnPoint.SpawnPattern.Circle;
            int index = 0;
            int total = 4;

            // Act
            Vector3 result = _spawnPoint.GetSpawnPosition(pattern, index, total);

            // Assert - First enemy should be at angle 0 degrees
            AssertFloat(result.X).IsEqual(20f, 0.01f);
            AssertFloat(result.Z).IsEqual(0f, 0.01f);
        }

        [TestCase]
        public void GetSpawnPosition_CirclePattern_SecondEnemy_ReturnsCorrectPosition()
        {
            // Arrange
            var pattern = SpawnPoint.SpawnPattern.Circle;
            int index = 1;
            int total = 4;

            // Act
            Vector3 result = _spawnPoint.GetSpawnPosition(pattern, index, total);

            // Assert - Second enemy should be at angle 90 degrees
            AssertFloat(result.X).IsEqual(0f, 0.01f);
            AssertFloat(result.Z).IsEqual(20f, 0.01f);
        }

        [TestCase]
        public void GetSpawnPosition_CirclePattern_ZeroTotal_ReturnsCenterPosition()
        {
            // Arrange
            var pattern = SpawnPoint.SpawnPattern.Circle;
            int index = 0;
            int total = 0;

            // Act
            Vector3 result = _spawnPoint.GetSpawnPosition(pattern, index, total);

            // Assert
            AssertThat(result).IsEqual(Vector3.Zero);
        }

        #endregion

        #region Line Pattern Tests

        [TestCase]
        public void GetSpawnPosition_LinePattern_FirstEnemy_ReturnsLeftPosition()
        {
            // Arrange
            var pattern = SpawnPoint.SpawnPattern.Line;
            int index = 0;
            int total = 4;

            // Act
            Vector3 result = _spawnPoint.GetSpawnPosition(pattern, index, total);

            // Assert - First enemy should be on the left
            AssertFloat(result.X).IsLess(0f);
            AssertFloat(result.Z).IsEqual(0f);
        }

        [TestCase]
        public void GetSpawnPosition_LinePattern_LastEnemy_ReturnsRightPosition()
        {
            // Arrange
            var pattern = SpawnPoint.SpawnPattern.Line;
            int index = 3;
            int total = 4;

            // Act
            Vector3 result = _spawnPoint.GetSpawnPosition(pattern, index, total);

            // Assert - Last enemy should be on the right
            AssertFloat(result.X).IsGreater(0f);
            AssertFloat(result.Z).IsEqual(0f);
        }

        #endregion

        #region Surround Pattern Tests

        [TestCase]
        public void GetSpawnPosition_SurroundPattern_UsesLargerRadius()
        {
            // Arrange
            var pattern = SpawnPoint.SpawnPattern.Surround;
            int index = 0;
            int total = 4;

            // Act
            Vector3 result = _spawnPoint.GetSpawnPosition(pattern, index, total);

            // Assert - Distance should be 1.5x the normal spawn radius
            float distance = result.Length();
            AssertFloat(distance).IsEqual(30f, 0.01f); // 20 * 1.5 = 30
        }

        #endregion

        #region Random Pattern Tests

        [TestCase]
        public void GetSpawnPosition_RandomPattern_ReturnsPositionWithinRadius()
        {
            // Arrange
            var pattern = SpawnPoint.SpawnPattern.Random;
            int index = 0;
            int total = 1;

            // Act
            Vector3 result = _spawnPoint.GetSpawnPosition(pattern, index, total);

            // Assert - Distance should be within random spawn radius
            float distance = result.Length();
            AssertFloat(distance).IsLessEqual(_spawnPoint.RandomSpawnRadius);
        }

        #endregion

        #region Pattern Parsing Tests

        [TestCase]
        public void ParseSpawnPattern_Circle_ReturnsCirclePattern()
        {
            // Arrange & Act
            var result = SpawnPoint.ParseSpawnPattern("circle");

            // Assert
            AssertThat(result).IsEqual(SpawnPoint.SpawnPattern.Circle);
        }

        [TestCase]
        public void ParseSpawnPattern_Line_ReturnsLinePattern()
        {
            // Arrange & Act
            var result = SpawnPoint.ParseSpawnPattern("line");

            // Assert
            AssertThat(result).IsEqual(SpawnPoint.SpawnPattern.Line);
        }

        [TestCase]
        public void ParseSpawnPattern_Surround_ReturnsSurroundPattern()
        {
            // Arrange & Act
            var result = SpawnPoint.ParseSpawnPattern("surround");

            // Assert
            AssertThat(result).IsEqual(SpawnPoint.SpawnPattern.Surround);
        }

        [TestCase]
        public void ParseSpawnPattern_Random_ReturnsRandomPattern()
        {
            // Arrange & Act
            var result = SpawnPoint.ParseSpawnPattern("random");

            // Assert
            AssertThat(result).IsEqual(SpawnPoint.SpawnPattern.Random);
        }

        [TestCase]
        public void ParseSpawnPattern_Unknown_ReturnsRandomPattern()
        {
            // Arrange & Act
            var result = SpawnPoint.ParseSpawnPattern("unknown_pattern");

            // Assert
            AssertThat(result).IsEqual(SpawnPoint.SpawnPattern.Random);
        }

        [TestCase]
        public void ParseSpawnPattern_Null_ReturnsRandomPattern()
        {
            // Arrange & Act
            var result = SpawnPoint.ParseSpawnPattern(null);

            // Assert
            AssertThat(result).IsEqual(SpawnPoint.SpawnPattern.Random);
        }

        [TestCase]
        public void ParseSpawnPattern_UpperCase_ReturnsCorrectPattern()
        {
            // Arrange & Act
            var result = SpawnPoint.ParseSpawnPattern("CIRCLE");

            // Assert
            AssertThat(result).IsEqual(SpawnPoint.SpawnPattern.Circle);
        }

        #endregion
    }
}
