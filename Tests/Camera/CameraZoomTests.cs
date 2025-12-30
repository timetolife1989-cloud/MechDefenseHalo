using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Camera;

namespace MechDefenseHalo.Tests.Camera
{
    /// <summary>
    /// Unit tests for CameraZoom
    /// </summary>
    [TestSuite]
    public class CameraZoomTests
    {
        #region Initialization Tests

        [TestCase]
        public void CameraZoom_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var zoom = AutoFree(new CameraZoom());

            // Assert
            AssertFloat(zoom.MinFov).IsEqual(40f);
            AssertFloat(zoom.MaxFov).IsEqual(90f);
            AssertFloat(zoom.DefaultFov).IsEqual(75f);
            AssertFloat(zoom.ZoomSpeed).IsEqual(5f);
        }

        #endregion

        #region Zoom In/Out Tests

        [TestCase]
        public void ZoomIn_DecreasesTargetFov()
        {
            // Arrange
            var camera = AutoFree(new Camera3D());
            var zoom = AutoFree(new CameraZoom());
            camera.AddChild(zoom);
            zoom._Ready();
            float initialFov = zoom.GetCurrentFov();

            // Act
            zoom.ZoomIn(10f);
            zoom._Process(1.0); // Process to apply zoom

            // Assert
            AssertFloat(zoom.GetCurrentFov()).IsLess(initialFov);
        }

        [TestCase]
        public void ZoomOut_IncreasesTargetFov()
        {
            // Arrange
            var camera = AutoFree(new Camera3D());
            var zoom = AutoFree(new CameraZoom());
            camera.AddChild(zoom);
            zoom._Ready();
            zoom.SetTargetFov(50f); // Start at lower FOV
            zoom._Process(2.0); // Apply
            float initialFov = zoom.GetCurrentFov();

            // Act
            zoom.ZoomOut(10f);
            zoom._Process(1.0); // Process to apply zoom

            // Assert
            AssertFloat(zoom.GetCurrentFov()).IsGreater(initialFov);
        }

        [TestCase]
        public void ZoomIn_RespectsMinFov()
        {
            // Arrange
            var camera = AutoFree(new Camera3D());
            var zoom = AutoFree(new CameraZoom());
            camera.AddChild(zoom);
            zoom.MinFov = 40f;
            zoom._Ready();

            // Act - zoom in beyond minimum
            zoom.ZoomIn(100f);
            zoom._Process(2.0); // Process to apply

            // Assert
            AssertFloat(zoom.GetCurrentFov()).IsGreaterEqual(40f);
        }

        [TestCase]
        public void ZoomOut_RespectsMaxFov()
        {
            // Arrange
            var camera = AutoFree(new Camera3D());
            var zoom = AutoFree(new CameraZoom());
            camera.AddChild(zoom);
            zoom.MaxFov = 90f;
            zoom._Ready();

            // Act - zoom out beyond maximum
            zoom.ZoomOut(100f);
            zoom._Process(2.0); // Process to apply

            // Assert
            AssertFloat(zoom.GetCurrentFov()).IsLessEqual(90f);
        }

        #endregion

        #region Target FOV Tests

        [TestCase]
        public void SetTargetFov_SetsCorrectValue()
        {
            // Arrange
            var camera = AutoFree(new Camera3D());
            var zoom = AutoFree(new CameraZoom());
            camera.AddChild(zoom);
            zoom._Ready();

            // Act
            zoom.SetTargetFov(60f);
            zoom._Process(2.0); // Process to apply

            // Assert
            AssertFloat(zoom.GetCurrentFov()).IsEqual(60f, 0.1f);
        }

        [TestCase]
        public void SetTargetFov_ClampsToMinMax()
        {
            // Arrange
            var camera = AutoFree(new Camera3D());
            var zoom = AutoFree(new CameraZoom());
            camera.AddChild(zoom);
            zoom.MinFov = 40f;
            zoom.MaxFov = 90f;
            zoom._Ready();

            // Act - try to set below min
            zoom.SetTargetFov(20f);
            zoom._Process(2.0);

            // Assert
            AssertFloat(zoom.GetCurrentFov()).IsGreaterEqual(40f);
        }

        [TestCase]
        public void ResetZoom_RestoresDefaultFov()
        {
            // Arrange
            var camera = AutoFree(new Camera3D());
            var zoom = AutoFree(new CameraZoom());
            camera.AddChild(zoom);
            zoom.DefaultFov = 75f;
            zoom._Ready();
            zoom.ZoomIn(20f);
            zoom._Process(1.0);

            // Act
            zoom.ResetZoom();
            zoom._Process(2.0); // Process to apply

            // Assert
            AssertFloat(zoom.GetCurrentFov()).IsEqual(75f, 0.1f);
        }

        #endregion

        #region Process Tests

        [TestCase]
        public void Process_WithNoCamera_DoesNotCrash()
        {
            // Arrange
            var zoom = AutoFree(new CameraZoom());
            // Don't add to camera, so GetParent will be null or not Camera3D

            // Act & Assert (should not throw)
            zoom._Process(0.016);
        }

        #endregion
    }
}
