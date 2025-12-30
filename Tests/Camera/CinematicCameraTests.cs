using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Camera;

namespace MechDefenseHalo.Tests.Camera
{
    /// <summary>
    /// Unit tests for CinematicCamera
    /// </summary>
    [TestSuite]
    public class CinematicCameraTests
    {
        #region Initialization Tests

        [TestCase]
        public void CinematicCamera_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var camera = AutoFree(new CinematicCamera());

            // Assert
            AssertFloat(camera.TransitionSpeed).IsEqual(2f);
            AssertBool(camera.AutoPlay).IsFalse();
        }

        #endregion

        #region Keyframe Management Tests

        [TestCase]
        public void AddKeyframe_IncreasesKeyframeCount()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());

            // Act
            camera.AddKeyframe(new Vector3(0, 0, 0), Vector3.Zero);

            // Assert
            AssertInt(camera.GetKeyframeCount()).IsEqual(1);
        }

        [TestCase]
        public void AddKeyframe_MultipleKeyframes_TracksCorrectly()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());

            // Act
            camera.AddKeyframe(new Vector3(0, 0, 0), Vector3.Zero);
            camera.AddKeyframe(new Vector3(10, 0, 0), Vector3.Zero);
            camera.AddKeyframe(new Vector3(20, 0, 0), Vector3.Zero);

            // Assert
            AssertInt(camera.GetKeyframeCount()).IsEqual(3);
        }

        [TestCase]
        public void AddKeyframeFromNode_AddsKeyframeCorrectly()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());
            var node = AutoFree(new Node3D());
            node.GlobalPosition = new Vector3(5, 5, 5);

            // Act
            camera.AddKeyframeFromNode(node);

            // Assert
            AssertInt(camera.GetKeyframeCount()).IsEqual(1);
        }

        [TestCase]
        public void ClearKeyframes_RemovesAllKeyframes()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());
            camera.AddKeyframe(new Vector3(0, 0, 0), Vector3.Zero);
            camera.AddKeyframe(new Vector3(10, 0, 0), Vector3.Zero);

            // Act
            camera.ClearKeyframes();

            // Assert
            AssertInt(camera.GetKeyframeCount()).IsEqual(0);
        }

        #endregion

        #region Sequence Playback Tests

        [TestCase]
        public void PlaySequence_WithNoKeyframes_DoesNothing()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());

            // Act & Assert (should not throw)
            camera.PlaySequence();
            AssertBool(camera.IsPlaying()).IsFalse();
        }

        [TestCase]
        public void PlaySequence_WithKeyframes_StartsPlayback()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());
            camera.AddKeyframe(new Vector3(0, 0, 0), Vector3.Zero);

            // Act
            camera.PlaySequence();

            // Assert
            AssertBool(camera.IsPlaying()).IsTrue();
        }

        [TestCase]
        public void StopSequence_StopsPlayback()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());
            camera.AddKeyframe(new Vector3(0, 0, 0), Vector3.Zero);
            camera.PlaySequence();

            // Act
            camera.StopSequence();

            // Assert
            AssertBool(camera.IsPlaying()).IsFalse();
        }

        [TestCase]
        public void PlaySequence_EmitsSequenceStartedSignal()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());
            camera.AddKeyframe(new Vector3(0, 0, 0), Vector3.Zero);
            bool signalEmitted = false;
            camera.SequenceStarted += () => signalEmitted = true;

            // Act
            camera.PlaySequence();

            // Assert
            AssertBool(signalEmitted).IsTrue();
        }

        [TestCase]
        public void Process_CompletesSequence_EmitsCompletedSignal()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());
            camera.AddKeyframe(new Vector3(0, 0, 0), Vector3.Zero);
            camera.TransitionSpeed = 100f; // Fast transition
            bool completedSignalEmitted = false;
            camera.SequenceCompleted += () => completedSignalEmitted = true;
            camera.PlaySequence();

            // Act - process enough to complete the sequence
            for (int i = 0; i < 10; i++)
            {
                camera._Process(0.1);
            }

            // Assert
            AssertBool(completedSignalEmitted).IsTrue();
        }

        #endregion

        #region AutoPlay Tests

        [TestCase]
        public void Ready_WithAutoPlayAndKeyframes_StartsPlaying()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());
            camera.AutoPlay = true;
            camera.AddKeyframe(new Vector3(0, 0, 0), Vector3.Zero);

            // Act
            camera._Ready();

            // Assert
            AssertBool(camera.IsPlaying()).IsTrue();
        }

        [TestCase]
        public void Ready_WithAutoPlayNoKeyframes_DoesNotStart()
        {
            // Arrange
            var camera = AutoFree(new CinematicCamera());
            camera.AutoPlay = true;

            // Act
            camera._Ready();

            // Assert
            AssertBool(camera.IsPlaying()).IsFalse();
        }

        #endregion
    }
}
