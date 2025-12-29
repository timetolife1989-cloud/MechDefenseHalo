using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.Tutorial
{
    /// <summary>
    /// Tracks tutorial progress and manages objective state
    /// </summary>
    public partial class TutorialProgressTracker : Node
    {
        #region Private Fields

        private TutorialStep _currentStep;
        private Dictionary<string, Action<object>> _activeListeners = new Dictionary<string, Action<object>>();

        #endregion

        #region Public Properties

        public TutorialStep CurrentStep => _currentStep;

        #endregion

        #region Public Methods

        /// <summary>
        /// Start tracking a new tutorial step
        /// </summary>
        /// <param name="step">The step to track</param>
        public void StartTracking(TutorialStep step)
        {
            if (_currentStep != null)
            {
                StopTracking();
            }

            _currentStep = step;
            _currentStep.CurrentProgress = 0;

            // Set up listeners based on objective type
            SetupObjectiveTracking();

            GD.Print($"Started tracking tutorial step {step.StepNumber}: {step.Title}");
        }

        /// <summary>
        /// Stop tracking current step and clean up listeners
        /// </summary>
        public void StopTracking()
        {
            if (_currentStep == null) return;

            // Clean up all active listeners
            foreach (var kvp in _activeListeners)
            {
                Core.EventBus.Off(kvp.Key, kvp.Value);
            }

            _activeListeners.Clear();
            _currentStep = null;

            GD.Print("Stopped tracking tutorial step");
        }

        /// <summary>
        /// Manually update progress (for testing or special cases)
        /// </summary>
        /// <param name="progress">New progress value</param>
        public void SetProgress(int progress)
        {
            if (_currentStep == null) return;

            _currentStep.CurrentProgress = progress;
            
            if (_currentStep.IsObjectiveComplete())
            {
                OnObjectiveComplete();
            }
        }

        #endregion

        #region Private Methods - Objective Tracking

        private void SetupObjectiveTracking()
        {
            if (_currentStep == null) return;

            switch (_currentStep.ObjectiveType)
            {
                case "distance_moved":
                    RegisterListener("player_moved", OnPlayerMoved);
                    break;
                case "shots_fired":
                    RegisterListener(Core.EventBus.WeaponFired, OnShotFired);
                    break;
                case "enemy_kills":
                    RegisterListener(Core.EventBus.EntityDied, OnEnemyKilled);
                    break;
                case "items_collected":
                    RegisterListener(Core.EventBus.LootPickedUp, OnItemCollected);
                    break;
                case "ui_opened":
                    RegisterListener("ui_opened", OnUIOpened);
                    break;
                case "item_equipped":
                    RegisterListener(Core.EventBus.ItemEquipped, OnItemEquipped);
                    break;
                case "drone_deployed":
                    RegisterListener(Core.EventBus.DroneDeployed, OnDroneDeployed);
                    break;
                case "wave_completed":
                    RegisterListener(Core.EventBus.WaveCompleted, OnWaveCompleted);
                    break;
                case "craft_started":
                    RegisterListener(Core.EventBus.CraftStarted, OnCraftStarted);
                    break;
            }
        }

        private void RegisterListener(string eventName, Action<object> callback)
        {
            _activeListeners[eventName] = callback;
            Core.EventBus.On(eventName, callback);
        }

        #endregion

        #region Event Handlers

        private void OnPlayerMoved(object data)
        {
            if (_currentStep == null || _currentStep.ObjectiveType != "distance_moved") return;

            if (data is float distance)
            {
                _currentStep.CurrentProgress += (int)distance;
                CheckProgress();
            }
        }

        private void OnShotFired(object data)
        {
            if (_currentStep == null || _currentStep.ObjectiveType != "shots_fired") return;

            _currentStep.CurrentProgress++;
            CheckProgress();
        }

        private void OnEnemyKilled(object data)
        {
            if (_currentStep == null || _currentStep.ObjectiveType != "enemy_kills") return;

            _currentStep.CurrentProgress++;
            CheckProgress();
        }

        private void OnItemCollected(object data)
        {
            if (_currentStep == null || _currentStep.ObjectiveType != "items_collected") return;

            _currentStep.CurrentProgress++;
            CheckProgress();
        }

        private void OnUIOpened(object data)
        {
            if (_currentStep == null || _currentStep.ObjectiveType != "ui_opened") return;

            if (data is string uiName && _currentStep.ObjectiveValue is string targetUI)
            {
                if (uiName == targetUI)
                {
                    _currentStep.CurrentProgress = 1;
                    CheckProgress();
                }
            }
        }

        private void OnItemEquipped(object data)
        {
            if (_currentStep == null || _currentStep.ObjectiveType != "item_equipped") return;

            _currentStep.CurrentProgress++;
            CheckProgress();
        }

        private void OnDroneDeployed(object data)
        {
            if (_currentStep == null || _currentStep.ObjectiveType != "drone_deployed") return;

            _currentStep.CurrentProgress++;
            CheckProgress();
        }

        private void OnWaveCompleted(object data)
        {
            if (_currentStep == null || _currentStep.ObjectiveType != "wave_completed") return;

            if (data is int waveNumber && _currentStep.ObjectiveValue is int targetWave)
            {
                if (waveNumber >= targetWave)
                {
                    _currentStep.CurrentProgress = 1;
                    CheckProgress();
                }
            }
        }

        private void OnCraftStarted(object data)
        {
            if (_currentStep == null || _currentStep.ObjectiveType != "craft_started") return;

            _currentStep.CurrentProgress++;
            CheckProgress();
        }

        private void CheckProgress()
        {
            if (_currentStep != null && _currentStep.IsObjectiveComplete())
            {
                OnObjectiveComplete();
            }
        }

        private void OnObjectiveComplete()
        {
            GD.Print($"Tutorial objective complete: {_currentStep.Title}");
            Core.EventBus.Emit("tutorial_objective_complete", _currentStep);
        }

        #endregion

        #region Godot Lifecycle

        public override void _ExitTree()
        {
            StopTracking();
        }

        #endregion
    }
}
