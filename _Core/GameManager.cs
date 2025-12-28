using Godot;
using System;
using MechDefenseHalo.Core;

namespace MechDefenseHalo.Core
{
    /// <summary>
    /// Central game manager singleton.
    /// Manages game state, scene transitions, and game flow.
    /// </summary>
    public partial class GameManager : Node
    {
        #region Singleton

        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GD.PrintErr("GameManager accessed before initialization!");
                }
                return _instance;
            }
        }

        #endregion

        #region Public Properties

        public GameState CurrentState { get; private set; } = GameState.Menu;
        public float GameTime { get; private set; } = 0f;
        public int PlayerScore { get; private set; } = 0;

        #endregion

        #region Private Fields

        private bool _isPaused = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr("Multiple GameManager instances detected! Removing duplicate.");
                QueueFree();
                return;
            }

            _instance = this;
            ProcessMode = ProcessModeEnum.Always; // Always process even when paused

            GD.Print("GameManager initialized");
        }

        public override void _Process(double delta)
        {
            if (CurrentState == GameState.Playing && !_isPaused)
            {
                GameTime += (float)delta;
            }

            // Handle pause input
            if (Input.IsActionJustPressed("pause"))
            {
                TogglePause();
            }
        }

        public override void _ExitTree()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Public Methods - State Management

        /// <summary>
        /// Change game state
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState)
                return;

            GameState oldState = CurrentState;
            CurrentState = newState;

            GD.Print($"Game state changed: {oldState} -> {newState}");

            // Handle state transitions
            OnStateChanged(oldState, newState);
        }

        /// <summary>
        /// Start new game
        /// </summary>
        public void StartGame()
        {
            GameTime = 0f;
            PlayerScore = 0;
            
            ChangeState(GameState.Playing);
            EventBus.Emit(EventBus.GameStarted, null);
            
            GD.Print("Game started!");
        }

        /// <summary>
        /// End current game
        /// </summary>
        public void EndGame(bool victory)
        {
            ChangeState(GameState.GameOver);
            EventBus.Emit(EventBus.GameOver, new GameOverData
            {
                Victory = victory,
                Score = PlayerScore,
                TimePlayed = GameTime
            });

            GD.Print($"Game ended! Victory: {victory}, Score: {PlayerScore}");
        }

        /// <summary>
        /// Toggle pause state
        /// </summary>
        public void TogglePause()
        {
            SetPaused(!_isPaused);
        }

        /// <summary>
        /// Set pause state
        /// </summary>
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
            GetTree().Paused = paused;

            EventBus.Emit(EventBus.GamePaused, paused);
            
            GD.Print($"Game {(paused ? "paused" : "resumed")}");
        }

        #endregion

        #region Public Methods - Score & Stats

        /// <summary>
        /// Add to player score
        /// </summary>
        public void AddScore(int amount)
        {
            PlayerScore += amount;
            GD.Print($"Score: {PlayerScore} (+{amount})");
        }

        #endregion

        #region Public Methods - Scene Management

        /// <summary>
        /// Load a scene by path
        /// </summary>
        public void LoadScene(string scenePath)
        {
            GetTree().ChangeSceneToFile(scenePath);
            GD.Print($"Loading scene: {scenePath}");
        }

        /// <summary>
        /// Reload current scene
        /// </summary>
        public void ReloadScene()
        {
            GetTree().ReloadCurrentScene();
            GD.Print("Reloading current scene");
        }

        /// <summary>
        /// Go to main menu
        /// </summary>
        public void GoToMainMenu()
        {
            SetPaused(false);
            ChangeState(GameState.Menu);
            LoadScene("res://Scenes/MainMenu.tscn");
        }

        /// <summary>
        /// Go to hub scene
        /// </summary>
        public void GoToHub()
        {
            SetPaused(false);
            ChangeState(GameState.Hub);
            LoadScene("res://Scenes/Hub/DroneSanctuary.tscn");
        }

        #endregion

        #region Private Methods

        private void OnStateChanged(GameState oldState, GameState newState)
        {
            // Clean up old state
            switch (oldState)
            {
                case GameState.Playing:
                    // Save game state if needed
                    break;
            }

            // Initialize new state
            switch (newState)
            {
                case GameState.Menu:
                    SetPaused(false);
                    break;
                case GameState.Playing:
                    SetPaused(false);
                    break;
                case GameState.Paused:
                    SetPaused(true);
                    break;
                case GameState.GameOver:
                    SetPaused(false);
                    break;
            }
        }

        #endregion
    }

    #region Enums

    public enum GameState
    {
        Menu,
        Hub,
        Playing,
        Paused,
        BossFight,
        GameOver
    }

    #endregion

    #region Event Data Structures

    public class GameOverData
    {
        public bool Victory { get; set; }
        public int Score { get; set; }
        public float TimePlayed { get; set; }
    }

    #endregion
}
