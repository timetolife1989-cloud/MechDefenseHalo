using Godot;
using System;

namespace MechDefenseHalo.Tutorial
{
    /// <summary>
    /// UI component for displaying tutorial dialog and instructions
    /// </summary>
    public partial class TutorialDialog : Control
    {
        #region Node References

        private Label _stepTitle;
        private Label _description;
        private ProgressBar _objectiveProgress;
        private Label _objectiveLabel;
        private Button _skipStepButton;
        private Button _skipTutorialButton;
        private Panel _backgroundPanel;

        #endregion

        #region Private Fields

        private TutorialStep _currentStep;
        private Action _onSkipStepCallback;
        private Action _onSkipTutorialCallback;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Get node references
            _backgroundPanel = GetNodeOrNull<Panel>("Panel");
            _stepTitle = GetNodeOrNull<Label>("Panel/StepTitle");
            _description = GetNodeOrNull<Label>("Panel/Description");
            _objectiveProgress = GetNodeOrNull<ProgressBar>("Panel/ObjectiveProgress");
            _objectiveLabel = GetNodeOrNull<Label>("Panel/ObjectiveLabel");
            _skipStepButton = GetNodeOrNull<Button>("Panel/Buttons/SkipStep");
            _skipTutorialButton = GetNodeOrNull<Button>("Panel/Buttons/SkipTutorial");

            // Connect button signals
            if (_skipStepButton != null)
            {
                _skipStepButton.Pressed += OnSkipStepPressed;
            }

            if (_skipTutorialButton != null)
            {
                _skipTutorialButton.Pressed += OnSkipTutorialPressed;
            }

            // Start hidden
            Hide();

            GD.Print("TutorialDialog initialized");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show the dialog with tutorial step information
        /// </summary>
        /// <param name="step">The tutorial step to display</param>
        public void ShowDialog(TutorialStep step)
        {
            _currentStep = step;

            if (_stepTitle != null)
            {
                _stepTitle.Text = $"Step {step.StepNumber}: {step.Title}";
            }

            if (_description != null)
            {
                _description.Text = step.Description;
            }

            // Setup progress bar
            UpdateProgress();

            // Show/hide skip step button
            if (_skipStepButton != null)
            {
                _skipStepButton.Visible = step.CanSkip;
            }

            Show();

            GD.Print($"Showing tutorial dialog: {step.Title}");
        }

        /// <summary>
        /// Update the progress display
        /// </summary>
        public void UpdateProgress()
        {
            if (_currentStep == null) return;

            if (_objectiveProgress != null)
            {
                _objectiveProgress.Value = _currentStep.GetProgressPercentage();
            }

            if (_objectiveLabel != null)
            {
                string objectiveText = GetUserFriendlyObjectiveType(_currentStep.ObjectiveType);
                _objectiveLabel.Text = $"{objectiveText}: {_currentStep.GetObjectiveText()}";
            }
        }

        #endregion

        #region Private Methods

        private string GetUserFriendlyObjectiveType(string objectiveType)
        {
            return objectiveType switch
            {
                "distance_moved" => "Distance Moved",
                "shots_fired" => "Shots Fired",
                "enemy_kills" => "Enemies Killed",
                "items_collected" => "Items Collected",
                "ui_opened" => "Open UI",
                "item_equipped" => "Items Equipped",
                "drone_deployed" => "Drones Deployed",
                "wave_completed" => "Waves Completed",
                "craft_started" => "Crafting Started",
                _ => objectiveType
            };
        }

        /// <summary>
        /// Show objective complete message
        /// </summary>
        public void ShowObjectiveComplete()
        {
            if (_objectiveLabel != null)
            {
                _objectiveLabel.Text = "✓ Objective Complete!";
            }

            if (_objectiveProgress != null)
            {
                _objectiveProgress.Value = 100;
            }

            GD.Print("Objective complete message displayed");
        }

        /// <summary>
        /// Show a simple message (for errors or notifications)
        /// </summary>
        /// <param name="message">Message to display</param>
        public void ShowMessage(string message)
        {
            if (_description != null)
            {
                _description.Text = message;
            }

            // Auto-hide after 2 seconds
            GetTree().CreateTimer(2.0).Timeout += () => {
                if (_currentStep != null && _description != null)
                {
                    _description.Text = _currentStep.Description;
                }
            };
        }

        /// <summary>
        /// Show skip tutorial confirmation dialog
        /// </summary>
        /// <param name="onConfirm">Callback when confirmed</param>
        public void ShowSkipConfirmation(Action onConfirm)
        {
            var confirmDialog = new ConfirmationDialog();
            confirmDialog.DialogText = "Are you sure you want to skip the tutorial?\nYou can replay it later from settings.";
            confirmDialog.Title = "Skip Tutorial";
            
            confirmDialog.Confirmed += () => {
                onConfirm?.Invoke();
                confirmDialog.QueueFree();
            };

            confirmDialog.Canceled += () => {
                confirmDialog.QueueFree();
            };

            AddChild(confirmDialog);
            confirmDialog.PopupCentered();
        }

        /// <summary>
        /// Set callbacks for skip buttons
        /// </summary>
        public void SetSkipCallbacks(Action onSkipStep, Action onSkipTutorial)
        {
            _onSkipStepCallback = onSkipStep;
            _onSkipTutorialCallback = onSkipTutorial;
        }

        /// <summary>
        /// Hide the dialog
        /// </summary>
        public void HideDialog()
        {
            Hide();
            _currentStep = null;
        }

        #endregion

        #region Private Methods

        private void OnSkipStepPressed()
        {
            _onSkipStepCallback?.Invoke();
        }

        private void OnSkipTutorialPressed()
        {
            _onSkipTutorialCallback?.Invoke();
        }

        #endregion
    }
}
