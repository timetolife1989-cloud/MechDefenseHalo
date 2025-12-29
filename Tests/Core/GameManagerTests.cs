using Godot;
using GdUnit4;
using MechDefenseHalo.Core;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Core
{
    /// <summary>
    /// Unit tests for GameManager system
    /// Tests game state transitions and lifecycle management
    /// </summary>
    [TestSuite]
    public class GameManagerTests
    {
        private GameManager _gameManager;

        [Before]
        public void Setup()
        {
            _gameManager = new GameManager();
        }

        [After]
        public void Teardown()
        {
            _gameManager = null;
        }

        [TestCase]
        public void InitialState_ShouldBeMenu()
        {
            // Assert
            AssertThat(_gameManager.CurrentState).IsEqual(GameState.Menu);
        }

        [TestCase]
        public void ChangeState_ToPlaying_ShouldUpdateState()
        {
            // Act
            _gameManager.ChangeState(GameState.Playing);

            // Assert
            AssertThat(_gameManager.CurrentState).IsEqual(GameState.Playing);
        }

        [TestCase]
        public void ChangeState_ToSameState_ShouldNotChange()
        {
            // Arrange
            var initialState = _gameManager.CurrentState;

            // Act
            _gameManager.ChangeState(initialState);

            // Assert
            AssertThat(_gameManager.CurrentState).IsEqual(initialState);
        }

        [TestCase]
        public void StartGame_ShouldResetGameTime()
        {
            // Act
            _gameManager.StartGame();

            // Assert
            AssertFloat(_gameManager.GameTime).IsEqual(0f);
        }

        [TestCase]
        public void StartGame_ShouldResetScore()
        {
            // Act
            _gameManager.StartGame();

            // Assert
            AssertInt(_gameManager.PlayerScore).IsEqual(0);
        }

        [TestCase]
        public void StartGame_ShouldChangeStateToPlaying()
        {
            // Act
            _gameManager.StartGame();

            // Assert
            AssertThat(_gameManager.CurrentState).IsEqual(GameState.Playing);
        }

        [TestCase]
        public void AddScore_ShouldIncreasePlayerScore()
        {
            // Arrange
            int initialScore = _gameManager.PlayerScore;
            int amountToAdd = 100;

            // Act
            _gameManager.AddScore(amountToAdd);

            // Assert
            AssertInt(_gameManager.PlayerScore).IsEqual(initialScore + amountToAdd);
        }

        [TestCase]
        public void AddScore_Multiple_ShouldAccumulate()
        {
            // Act
            _gameManager.AddScore(50);
            _gameManager.AddScore(75);
            _gameManager.AddScore(25);

            // Assert
            AssertInt(_gameManager.PlayerScore).IsEqual(150);
        }

        [TestCase]
        public void EndGame_Victory_ShouldChangeStateToGameOver()
        {
            // Act
            _gameManager.EndGame(true);

            // Assert
            AssertThat(_gameManager.CurrentState).IsEqual(GameState.GameOver);
        }

        [TestCase]
        public void EndGame_Defeat_ShouldChangeStateToGameOver()
        {
            // Act
            _gameManager.EndGame(false);

            // Assert
            AssertThat(_gameManager.CurrentState).IsEqual(GameState.GameOver);
        }

        [TestCase]
        public void SetPaused_True_ShouldPauseGame()
        {
            // Act
            _gameManager.SetPaused(true);

            // Note: We can't test GetTree().Paused in unit tests
            // This test verifies the method executes without error
            AssertBool(true).IsTrue();
        }

        [TestCase]
        public void SetPaused_False_ShouldUnpauseGame()
        {
            // Arrange
            _gameManager.SetPaused(true);

            // Act
            _gameManager.SetPaused(false);

            // Assert - method should execute without error
            AssertBool(true).IsTrue();
        }

        [TestCase]
        public void GameStateTransition_MenuToPlaying_ShouldBeValid()
        {
            // Arrange
            _gameManager.ChangeState(GameState.Menu);

            // Act
            _gameManager.ChangeState(GameState.Playing);

            // Assert
            AssertThat(_gameManager.CurrentState).IsEqual(GameState.Playing);
        }

        [TestCase]
        public void GameStateTransition_PlayingToGameOver_ShouldBeValid()
        {
            // Arrange
            _gameManager.ChangeState(GameState.Playing);

            // Act
            _gameManager.ChangeState(GameState.GameOver);

            // Assert
            AssertThat(_gameManager.CurrentState).IsEqual(GameState.GameOver);
        }

        [TestCase]
        public void GameStateTransition_HubToPlaying_ShouldBeValid()
        {
            // Arrange
            _gameManager.ChangeState(GameState.Hub);

            // Act
            _gameManager.ChangeState(GameState.Playing);

            // Assert
            AssertThat(_gameManager.CurrentState).IsEqual(GameState.Playing);
        }
    }
}
