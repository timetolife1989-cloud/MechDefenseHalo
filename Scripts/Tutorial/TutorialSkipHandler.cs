using Godot;
using System;

namespace MechDefenseHalo.Tutorial
{
    /// <summary>
    /// Handles tutorial skip logic and confirmation dialogs
    /// </summary>
    public partial class TutorialSkipHandler : Node
    {
        #region Public Methods

        /// <summary>
        /// Check if a step can be skipped
        /// </summary>
        /// <param name="step">The step to check</param>
        /// <returns>True if skippable</returns>
        public bool CanSkipStep(TutorialStep step)
        {
            if (step == null) return false;
            return step.CanSkip;
        }

        /// <summary>
        /// Show skip confirmation dialog
        /// </summary>
        /// <param name="skipType">Type of skip (step or tutorial)</param>
        /// <param name="onConfirm">Callback when confirmed</param>
        /// <param name="onCancel">Callback when canceled</param>
        public void ShowSkipConfirmation(SkipType skipType, Action onConfirm, Action onCancel = null)
        {
            string dialogText = skipType == SkipType.Step
                ? "Skip this tutorial step?"
                : "Are you sure you want to skip the entire tutorial?\nYou can replay it later from settings.";

            string title = skipType == SkipType.Step
                ? "Skip Step"
                : "Skip Tutorial";

            var confirmDialog = new ConfirmationDialog();
            confirmDialog.DialogText = dialogText;
            confirmDialog.Title = title;
            confirmDialog.OkButtonText = "Skip";
            confirmDialog.CancelButtonText = "Continue Tutorial";

            confirmDialog.Confirmed += () => {
                GD.Print($"{skipType} skip confirmed");
                onConfirm?.Invoke();
                confirmDialog.QueueFree();
            };

            confirmDialog.Canceled += () => {
                GD.Print($"{skipType} skip canceled");
                onCancel?.Invoke();
                confirmDialog.QueueFree();
            };

            // Add to scene tree and show
            GetTree().Root.AddChild(confirmDialog);
            confirmDialog.PopupCentered();
        }

        /// <summary>
        /// Skip to a specific step (for debugging or replay)
        /// </summary>
        /// <param name="stepNumber">Step number to skip to</param>
        /// <param name="callback">Callback with step number</param>
        public void SkipToStep(int stepNumber, Action<int> callback)
        {
            GD.Print($"Skipping to tutorial step {stepNumber}");
            callback?.Invoke(stepNumber);
        }

        #endregion

        #region Enums

        public enum SkipType
        {
            Step,
            Tutorial
        }

        #endregion
    }
}
