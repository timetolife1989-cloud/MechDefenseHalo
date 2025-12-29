using Godot;
using GdUnit4;
using MechDefenseHalo.Analytics;
using MechDefenseHalo.Core;
using MechDefenseHalo.Monetization;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Monetization
{
    /// <summary>
    /// Unit tests for AdMetricsTracker system
    /// Tests analytics tracking and conversion rate calculation
    /// </summary>
    [TestSuite]
    public class AdMetricsTrackerTests
    {
        private EventBus _eventBus;
        private AdMetricsTracker _metricsTracker;

        [Before]
        public void Setup()
        {
            _eventBus = new EventBus();
            _eventBus._Ready();

            _metricsTracker = new AdMetricsTracker();
            _metricsTracker._Ready();

            // Reset metrics
            AdMetricsTracker.ResetMetrics();
        }

        [After]
        public void Teardown()
        {
            _metricsTracker?.QueueFree();
            _eventBus?.QueueFree();
            _metricsTracker = null;
            _eventBus = null;
        }

        [TestCase]
        public void InitialConversionRate_ShouldBeZero()
        {
            // Assert
            AssertFloat(AdMetricsTracker.Instance.OverallConversionRate).IsEqual(0f);
        }

        [TestCase]
        public void GetMetrics_ForNonExistentAdType_ShouldReturnEmptyMetrics()
        {
            // Act
            var metrics = AdMetricsTracker.GetMetrics("non_existent");

            // Assert
            AssertInt(metrics.Offered).IsEqual(0);
            AssertInt(metrics.Watched).IsEqual(0);
            AssertInt(metrics.Skipped).IsEqual(0);
        }

        [TestCase]
        public void OnAdOffered_ShouldIncrementOfferedCount()
        {
            // Act
            EventBus.Emit("ad_offered", new AdOfferedData { AdType = "test_ad" });

            // Wait a frame for event processing
            // (In actual runtime, events are processed immediately)
            
            // Assert
            var metrics = AdMetricsTracker.GetMetrics("test_ad");
            AssertInt(metrics.Offered).IsEqual(1);
        }

        [TestCase]
        public void OnAdWatched_ShouldIncrementWatchedCount()
        {
            // Arrange
            EventBus.Emit("ad_offered", new AdOfferedData { AdType = "test_ad" });

            // Act
            EventBus.Emit("ad_watched", new AdWatchedData { AdType = "test_ad" });

            // Assert
            var metrics = AdMetricsTracker.GetMetrics("test_ad");
            AssertInt(metrics.Watched).IsEqual(1);
        }

        [TestCase]
        public void OnAdSkipped_ShouldIncrementSkippedCount()
        {
            // Arrange
            EventBus.Emit("ad_offered", new AdOfferedData { AdType = "test_ad" });

            // Act
            EventBus.Emit("ad_skipped", new AdSkippedData { AdType = "test_ad" });

            // Assert
            var metrics = AdMetricsTracker.GetMetrics("test_ad");
            AssertInt(metrics.Skipped).IsEqual(1);
        }

        [TestCase]
        public void ConversionRate_WithTwoOfferedOneWatched_ShouldBeFiftyPercent()
        {
            // Arrange & Act
            EventBus.Emit("ad_offered", new AdOfferedData { AdType = "test_ad" });
            EventBus.Emit("ad_watched", new AdWatchedData { AdType = "test_ad" });
            
            EventBus.Emit("ad_offered", new AdOfferedData { AdType = "test_ad" });
            EventBus.Emit("ad_skipped", new AdSkippedData { AdType = "test_ad" });

            // Assert
            var metrics = AdMetricsTracker.GetMetrics("test_ad");
            AssertFloat(metrics.ConversionRate).IsEqual(0.5f, 0.01f); // 50% with tolerance
        }

        [TestCase]
        public void OverallConversionRate_WithMultipleAdTypes_ShouldCalculateCorrectly()
        {
            // Arrange & Act
            // Victory ad: 2 offered, 1 watched
            EventBus.Emit("ad_offered", new AdOfferedData { AdType = "victory" });
            EventBus.Emit("ad_watched", new AdWatchedData { AdType = "victory" });
            EventBus.Emit("ad_offered", new AdOfferedData { AdType = "victory" });
            EventBus.Emit("ad_skipped", new AdSkippedData { AdType = "victory" });

            // Milestone ad: 2 offered, 2 watched
            EventBus.Emit("ad_offered", new AdOfferedData { AdType = "milestone" });
            EventBus.Emit("ad_watched", new AdWatchedData { AdType = "milestone" });
            EventBus.Emit("ad_offered", new AdOfferedData { AdType = "milestone" });
            EventBus.Emit("ad_watched", new AdWatchedData { AdType = "milestone" });

            // Assert
            // Total: 4 offered, 3 watched = 75%
            float expectedRate = 3f / 4f;
            AssertFloat(AdMetricsTracker.Instance.OverallConversionRate).IsEqual(expectedRate, 0.01f);
        }

        [TestCase]
        public void ResetMetrics_ShouldClearAllData()
        {
            // Arrange
            EventBus.Emit("ad_offered", new AdOfferedData { AdType = "test_ad" });
            EventBus.Emit("ad_watched", new AdWatchedData { AdType = "test_ad" });

            // Act
            AdMetricsTracker.ResetMetrics();

            // Assert
            var metrics = AdMetricsTracker.GetMetrics("test_ad");
            AssertInt(metrics.Offered).IsEqual(0);
            AssertInt(metrics.Watched).IsEqual(0);
            AssertFloat(AdMetricsTracker.Instance.OverallConversionRate).IsEqual(0f);
        }
    }
}
