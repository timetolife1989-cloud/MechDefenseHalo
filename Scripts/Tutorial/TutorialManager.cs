using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Inventory;

namespace MechDefenseHalo.Tutorial
{
    /// <summary>
    /// Main manager for the interactive tutorial system
    /// Coordinates tutorial flow, UI, and objective tracking
    /// </summary>
    public partial class TutorialManager : Node
    {
        #region Exported Properties

        [Export] public string TutorialStepsPath { get; set; } = "res://Data/Tutorial/tutorial_steps.json";
        [Export] public string TutorialRewardsPath { get; set; } = "res://Data/Tutorial/tutorial_rewards.json";

        #endregion

        #region Public Properties

        public bool IsTutorialActive { get; private set; } = false;
        public bool IsTutorialComplete { get; private set; } = false;
        public int CurrentStepIndex => _currentStepIndex;

        #endregion

        #region Private Fields

        private List<TutorialStep> _tutorialSteps = new List<TutorialStep>();
        private int _currentStepIndex = 0;
        private TutorialStep _currentStep;
        
        private TutorialDialog _dialogUI;
        private TutorialHighlight _highlightUI;
        private TutorialProgressTracker _progressTracker;
        private TutorialSkipHandler _skipHandler;
        
        private TutorialRewardsData _rewardsData;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Initialize child components
            InitializeComponents();
            
            // Load tutorial data
            LoadTutorialSteps();
            LoadRewardsData();
            
            // Check if tutorial already completed
            if (GetTutorialCompleted())
            {
                IsTutorialComplete = true;
                GD.Print("Tutorial already completed");
                return;
            }
            
            // Start tutorial for new players or if requested
            if (IsFirstLaunch())
            {
                CallDeferred(nameof(StartTutorial));
            }

            GD.Print("TutorialManager initialized");
        }

        public override void _ExitTree()
        {
            // Clean up
            if (_progressTracker != null)
            {
                _progressTracker.StopTracking();
            }
        }

        #endregion

        #region Public Methods - Tutorial Control

        /// <summary>
        /// Start the tutorial from the beginning
        /// </summary>
        public void StartTutorial()
        {
            if (IsTutorialActive)
            {
                GD.Print("Tutorial is already active");
                return;
            }

            IsTutorialActive = true;
            _currentStepIndex = 0;
            
            // Pause wave spawner if it exists
            var waveSpawner = GetNodeOrNull<GamePlay.WaveSpawner>("/root/GameManager/WaveSpawner");
            if (waveSpawner != null)
            {
                waveSpawner.Set("paused_for_tutorial", true);
            }
            
            ShowStep(_tutorialSteps[0]);
            
            EventBus.Emit(EventBus.TutorialStarted, null);
            GD.Print("Tutorial started");
        }

        /// <summary>
        /// Skip the current step (if allowed)
        /// </summary>
        public void SkipCurrentStep()
        {
            if (_currentStep == null || !_currentStep.CanSkip)
            {
                if (_dialogUI != null)
                {
                    _dialogUI.ShowMessage("This step cannot be skipped");
                }
                return;
            }

            _skipHandler.ShowSkipConfirmation(
                TutorialSkipHandler.SkipType.Step,
                () => NextStep()
            );
        }

        /// <summary>
        /// Skip the entire tutorial
        /// </summary>
        public void SkipTutorial()
        {
            if (!IsTutorialActive)
            {
                GD.Print("No active tutorial to skip");
                return;
            }

            _skipHandler.ShowSkipConfirmation(
                TutorialSkipHandler.SkipType.Tutorial,
                () => CompleteTutorial()
            );
        }

        /// <summary>
        /// Restart the tutorial from the beginning
        /// </summary>
        public void RestartTutorial()
        {
            if (IsTutorialActive)
            {
                StopTutorial();
            }

            IsTutorialComplete = false;
            SetTutorialCompleted(false);
            
            StartTutorial();
        }

        /// <summary>
        /// Stop the tutorial without completing it
        /// </summary>
        public void StopTutorial()
        {
            if (!IsTutorialActive) return;

            IsTutorialActive = false;
            
            if (_progressTracker != null)
            {
                _progressTracker.StopTracking();
            }

            if (_highlightUI != null)
            {
                _highlightUI.ClearHighlights();
            }

            if (_dialogUI != null)
            {
                _dialogUI.HideDialog();
            }

            EventBus.Emit(EventBus.TutorialStopped, null);
            GD.Print("Tutorial stopped");
        }

        #endregion

        #region Private Methods - Tutorial Flow

        private void ShowStep(TutorialStep step)
        {
            _currentStep = step;
            
            GD.Print($"Showing tutorial step {step.StepNumber}: {step.Title}");
            
            // Show dialog with instructions
            if (_dialogUI != null)
            {
                _dialogUI.ShowDialog(step);
            }
            
            // Clear previous highlights
            if (_highlightUI != null)
            {
                _highlightUI.ClearHighlights();
            }
            
            // Highlight relevant UI elements
            if (!string.IsNullOrEmpty(step.HighlightUI) && _highlightUI != null)
            {
                _highlightUI.HighlightElement(step.HighlightUI);
            }
            
            // Highlight keyboard keys
            if (step.HighlightKeys != null && step.HighlightKeys.Count > 0 && _highlightUI != null)
            {
                _highlightUI.HighlightKeys(step.HighlightKeys);
            }
            
            // Spawn tutorial enemies if needed
            if (step.SpawnEnemies != null && step.SpawnEnemies.Count > 0)
            {
                SpawnTutorialEnemies(step.SpawnEnemies);
            }
            
            // Start wave if needed
            if (step.SpawnWave > 0)
            {
                StartTutorialWave(step.SpawnWave);
            }
            
            // Start tracking objectives
            if (_progressTracker != null)
            {
                _progressTracker.StartTracking(step);
            }
            
            EventBus.Emit(EventBus.TutorialStepStarted, new TutorialStepEventData 
            { 
                StepNumber = step.StepNumber,
                Title = step.Title 
            });
        }

        private void NextStep()
        {
            _currentStepIndex++;
            
            if (_currentStepIndex >= _tutorialSteps.Count)
            {
                CompleteTutorial();
                return;
            }
            
            ShowStep(_tutorialSteps[_currentStepIndex]);
        }

        private void CompleteTutorial()
        {
            IsTutorialActive = false;
            IsTutorialComplete = true;
            
            // Stop tracking
            if (_progressTracker != null)
            {
                _progressTracker.StopTracking();
            }
            
            // Clear UI
            if (_highlightUI != null)
            {
                _highlightUI.ClearHighlights();
            }
            
            if (_dialogUI != null)
            {
                _dialogUI.HideDialog();
            }
            
            // Re-enable game systems
            var waveSpawner = GetNodeOrNull<GamePlay.WaveSpawner>("/root/GameManager/WaveSpawner");
            if (waveSpawner != null)
            {
                waveSpawner.Set("paused_for_tutorial", false);
            }
            
            // Grant completion rewards
            GrantTutorialRewards();
            
            // Show completion screen
            ShowCompletionScreen();
            
            // Save completion status
            SetTutorialCompleted(true);
            
            EventBus.Emit(EventBus.TutorialCompleted, null);
            GD.Print("Tutorial completed!");
        }

        private void OnObjectiveComplete()
        {
            GD.Print($"Tutorial objective complete for step {_currentStep?.StepNumber}");
            
            // Show completion feedback
            if (_dialogUI != null)
            {
                _dialogUI.ShowObjectiveComplete();
            }
            
            // Grant step rewards if any
            GrantStepRewards(_currentStep.StepNumber);
            
            // Wait 2 seconds, then next step
            var timer = GetTree().CreateTimer(2.0);
            timer.Timeout += () => NextStep();
        }

        #endregion

        #region Private Methods - Spawning

        private void SpawnTutorialEnemies(List<string> enemyTypes)
        {
            GD.Print($"Spawning {enemyTypes.Count} tutorial enemies");
            
            // Find wave spawner or enemy container
            var waveSpawner = GetNodeOrNull<GamePlay.WaveSpawner>("/root/GameManager/WaveSpawner");
            
            if (waveSpawner != null)
            {
                // Use wave spawner to spawn enemies
                foreach (var enemyType in enemyTypes)
                {
                    // Call spawn method on wave spawner
                    waveSpawner.Call("spawn_enemy", enemyType);
                }
            }
            else
            {
                GD.PrintErr("Wave spawner not found, cannot spawn tutorial enemies");
            }
        }

        private void StartTutorialWave(int waveNumber)
        {
            GD.Print($"Starting tutorial wave {waveNumber}");
            
            var waveSpawner = GetNodeOrNull<GamePlay.WaveSpawner>("/root/GameManager/WaveSpawner");
            
            if (waveSpawner != null)
            {
                waveSpawner.Call("start_wave", waveNumber);
            }
            else
            {
                GD.PrintErr("Wave spawner not found, cannot start tutorial wave");
            }
        }

        #endregion

        #region Private Methods - Rewards

        private void GrantTutorialRewards()
        {
            if (_rewardsData?.CompletionRewards == null)
            {
                GD.Print("No completion rewards data found");
                return;
            }

            var rewards = _rewardsData.CompletionRewards;
            
            // Grant currency
            if (rewards.Credits > 0)
            {
                CurrencyManager.AddCredits(rewards.Credits, "tutorial_complete");
            }
            
            if (rewards.Cores > 0)
            {
                CurrencyManager.AddCores(rewards.Cores, "tutorial_complete");
            }
            
            // Grant XP (if player level system exists)
            if (rewards.Experience > 0)
            {
                EventBus.Emit("add_experience", rewards.Experience);
            }
            
            // Grant items
            if (rewards.Items != null)
            {
                var inventoryManager = GetNodeOrNull<InventoryManager>("/root/GameManager/InventoryManager");
                
                foreach (var itemReward in rewards.Items)
                {
                    // Note: This would require ItemDatabase to get actual item
                    // For now, just emit an event
                    EventBus.Emit("grant_tutorial_item", new { 
                        ItemId = itemReward.ItemId, 
                        Quantity = itemReward.Quantity 
                    });
                }
            }
            
            GD.Print($"Granted tutorial rewards: {rewards.Credits} credits, {rewards.Cores} cores, {rewards.Experience} XP");
        }

        private void GrantStepRewards(int stepNumber)
        {
            if (_rewardsData?.StepRewards == null) return;
            
            if (_rewardsData.StepRewards.TryGetValue(stepNumber.ToString(), out var stepReward))
            {
                if (stepReward.Credits > 0)
                {
                    CurrencyManager.AddCredits(stepReward.Credits, $"tutorial_step_{stepNumber}");
                }
                
                GD.Print($"Granted step {stepNumber} rewards: {stepReward.Credits} credits");
            }
        }

        private void ShowCompletionScreen()
        {
            // Create a simple completion notification
            var dialog = new AcceptDialog();
            dialog.Title = "Tutorial Complete!";
            dialog.DialogText = "Congratulations! You've completed the tutorial.\n\n" +
                              $"Rewards:\n" +
                              $"• {_rewardsData?.CompletionRewards?.Credits ?? 0} Credits\n" +
                              $"• {_rewardsData?.CompletionRewards?.Cores ?? 0} Cores\n" +
                              $"• {_rewardsData?.CompletionRewards?.Experience ?? 0} XP\n" +
                              $"• Starter items";
            
            dialog.Confirmed += () => dialog.QueueFree();
            dialog.Canceled += () => dialog.QueueFree();
            
            GetTree().Root.AddChild(dialog);
            dialog.PopupCentered();
        }

        #endregion

        #region Private Methods - Initialization

        private void InitializeComponents()
        {
            // Create dialog UI
            _dialogUI = new TutorialDialog();
            _dialogUI.Name = "TutorialDialog";
            AddChild(_dialogUI);
            _dialogUI.SetSkipCallbacks(SkipCurrentStep, SkipTutorial);
            
            // Create highlight UI
            _highlightUI = new TutorialHighlight();
            _highlightUI.Name = "TutorialHighlight";
            AddChild(_highlightUI);
            
            // Create progress tracker
            _progressTracker = new TutorialProgressTracker();
            _progressTracker.Name = "TutorialProgressTracker";
            AddChild(_progressTracker);
            
            // Create skip handler
            _skipHandler = new TutorialSkipHandler();
            _skipHandler.Name = "TutorialSkipHandler";
            AddChild(_skipHandler);
            
            // Listen for objective completion
            EventBus.On(EventBus.TutorialObjectiveComplete, (data) => OnObjectiveComplete());
        }

        private void LoadTutorialSteps()
        {
            try
            {
                if (!FileAccess.FileExists(TutorialStepsPath))
                {
                    GD.PrintErr($"Tutorial steps file not found: {TutorialStepsPath}");
                    CreateDefaultSteps();
                    return;
                }

                using var file = FileAccess.Open(TutorialStepsPath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"Failed to open tutorial steps file: {FileAccess.GetOpenError()}");
                    CreateDefaultSteps();
                    return;
                }

                string json = file.GetAsText();
                var stepDataList = JsonSerializer.Deserialize<List<TutorialStepData>>(json);

                if (stepDataList == null || stepDataList.Count == 0)
                {
                    GD.PrintErr("Failed to parse tutorial steps or empty list");
                    CreateDefaultSteps();
                    return;
                }

                // Convert to TutorialStep objects
                foreach (var data in stepDataList)
                {
                    var step = new TutorialStep
                    {
                        StepNumber = data.Step,
                        Title = data.Title ?? "",
                        Description = data.Description ?? "",
                        ObjectiveType = data.ObjectiveType ?? "",
                        ObjectiveValue = data.ObjectiveValue,
                        HighlightKeys = data.HighlightKeys ?? new List<string>(),
                        HighlightUI = data.HighlightUI ?? "",
                        SpawnEnemies = data.SpawnEnemies ?? new List<string>(),
                        SpawnWave = data.SpawnWave,
                        CanSkip = data.CanSkip ?? true
                    };
                    
                    _tutorialSteps.Add(step);
                }

                GD.Print($"Loaded {_tutorialSteps.Count} tutorial steps");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading tutorial steps: {e.Message}");
                CreateDefaultSteps();
            }
        }

        private void CreateDefaultSteps()
        {
            // Create minimal default steps
            _tutorialSteps.Add(new TutorialStep
            {
                StepNumber = 1,
                Title = "Welcome",
                Description = "Welcome to the tutorial",
                ObjectiveType = "manual",
                ObjectiveValue = 1,
                CanSkip = false
            });
            
            GD.Print("Created default tutorial steps");
        }

        private void LoadRewardsData()
        {
            try
            {
                if (!FileAccess.FileExists(TutorialRewardsPath))
                {
                    GD.PrintErr($"Tutorial rewards file not found: {TutorialRewardsPath}");
                    CreateDefaultRewards();
                    return;
                }

                using var file = FileAccess.Open(TutorialRewardsPath, FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"Failed to open tutorial rewards file: {FileAccess.GetOpenError()}");
                    CreateDefaultRewards();
                    return;
                }

                string json = file.GetAsText();
                _rewardsData = JsonSerializer.Deserialize<TutorialRewardsData>(json);

                if (_rewardsData == null)
                {
                    GD.PrintErr("Failed to parse tutorial rewards");
                    CreateDefaultRewards();
                    return;
                }

                GD.Print("Loaded tutorial rewards data");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading tutorial rewards: {e.Message}");
                CreateDefaultRewards();
            }
        }

        private void CreateDefaultRewards()
        {
            _rewardsData = new TutorialRewardsData
            {
                CompletionRewards = new CompletionRewardsData
                {
                    Credits = 500,
                    Cores = 50,
                    Experience = 200
                },
                StepRewards = new Dictionary<string, StepRewardData>()
            };
            
            GD.Print("Created default tutorial rewards");
        }

        #endregion

        #region Private Methods - Save/Load

        private bool GetTutorialCompleted()
        {
            return SaveManager.GetBool("tutorial_completed");
        }

        private void SetTutorialCompleted(bool completed)
        {
            SaveManager.SetBool("tutorial_completed", completed);
        }

        private bool IsFirstLaunch()
        {
            return SaveManager.GetBool("is_first_launch");
        }

        #endregion

        #region Data Classes

        private class TutorialStepData
        {
            [JsonPropertyName("step")]
            public int Step { get; set; }
            
            [JsonPropertyName("title")]
            public string Title { get; set; }
            
            [JsonPropertyName("description")]
            public string Description { get; set; }
            
            [JsonPropertyName("objective_type")]
            public string ObjectiveType { get; set; }
            
            [JsonPropertyName("objective_value")]
            public object ObjectiveValue { get; set; }
            
            [JsonPropertyName("highlight_keys")]
            public List<string> HighlightKeys { get; set; }
            
            [JsonPropertyName("highlight_ui")]
            public string HighlightUI { get; set; }
            
            [JsonPropertyName("spawn_enemies")]
            public List<string> SpawnEnemies { get; set; }
            
            [JsonPropertyName("spawn_wave")]
            public int SpawnWave { get; set; }
            
            [JsonPropertyName("can_skip")]
            public bool? CanSkip { get; set; }
        }

        private class TutorialRewardsData
        {
            [JsonPropertyName("completion_rewards")]
            public CompletionRewardsData CompletionRewards { get; set; }
            
            [JsonPropertyName("step_rewards")]
            public Dictionary<string, StepRewardData> StepRewards { get; set; }
        }

        private class CompletionRewardsData
        {
            [JsonPropertyName("credits")]
            public int Credits { get; set; }
            
            [JsonPropertyName("cores")]
            public int Cores { get; set; }
            
            [JsonPropertyName("experience")]
            public int Experience { get; set; }
            
            [JsonPropertyName("items")]
            public List<ItemRewardData> Items { get; set; }
        }

        private class StepRewardData
        {
            [JsonPropertyName("credits")]
            public int Credits { get; set; }
        }

        private class ItemRewardData
        {
            [JsonPropertyName("item_id")]
            public string ItemId { get; set; }
            
            [JsonPropertyName("quantity")]
            public int Quantity { get; set; }
        }

        private class TutorialStepEventData
        {
            public int StepNumber { get; set; }
            public string Title { get; set; }
        }

        #endregion
    }
}
