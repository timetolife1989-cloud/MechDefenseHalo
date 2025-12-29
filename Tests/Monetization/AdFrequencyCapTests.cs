using Godot;
using GdUnit4;
using MechDefenseHalo.Monetization;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Monetization
{
    /// <summary>
    /// Unit tests for AdFrequencyCap system
    /// Tests daily limits and cooldown enforcement
    /// </summary>
    [TestSuite]
    public class AdFrequencyCapTests
    {
        private AdFrequencyCap _adFrequencyCap;

        [Before]
        public void Setup()
        {
            _adFrequencyCap = new AdFrequencyCap();
            _adFrequencyCap._Ready();
            
            // Reset counters for testing
            AdFrequencyCap.ResetDailyCounter();
            AdFrequencyCap.ResetAllCooldowns();
        }

        [After]
        public void Teardown()
        {
            _adFrequencyCap?.QueueFree();
            _adFrequencyCap = null;
        }

        [TestCase]
        public void CanShowAd_InitialState_ShouldReturnTrue()
        {
            // Act
            bool canShow = AdFrequencyCap.CanShowAd("test_ad");

            // Assert
            AssertBool(canShow).IsTrue();
        }

        [TestCase]
        public void RecordAdShown_ShouldIncrementCounter()
        {
            // Arrange
            int initialCount = AdFrequencyCap.Instance.AdsShownToday;

            // Act
            AdFrequencyCap.RecordAdShown("test_ad");

            // Assert
            AssertInt(AdFrequencyCap.Instance.AdsShownToday).IsEqual(initialCount + 1);
        }

        [TestCase]
        public void CanShowAd_AfterDailyLimit_ShouldReturnFalse()
        {
            // Arrange - Show 5 ads (daily limit)
            for (int i = 0; i < 5; i++)
            {
                AdFrequencyCap.RecordAdShown($"test_ad_{i}");
            }

            // Act
            bool canShow = AdFrequencyCap.CanShowAd("test_ad");

            // Assert
            AssertBool(canShow).IsFalse();
        }

        [TestCase]
        public void RemainingAds_AfterThreeAds_ShouldReturnTwo()
        {
            // Arrange
            AdFrequencyCap.RecordAdShown("ad1");
            AdFrequencyCap.RecordAdShown("ad2");
            AdFrequencyCap.RecordAdShown("ad3");

            // Act
            int remaining = AdFrequencyCap.Instance.RemainingAds;

            // Assert
            AssertInt(remaining).IsEqual(2);
        }

        [TestCase]
        public void HasReachedDailyLimit_BeforeLimit_ShouldReturnFalse()
        {
            // Arrange
            AdFrequencyCap.RecordAdShown("test_ad");

            // Act
            bool reachedLimit = AdFrequencyCap.Instance.HasReachedDailyLimit;

            // Assert
            AssertBool(reachedLimit).IsFalse();
        }

        [TestCase]
        public void HasReachedDailyLimit_AtLimit_ShouldReturnTrue()
        {
            // Arrange - Show 5 ads (daily limit)
            for (int i = 0; i < 5; i++)
            {
                AdFrequencyCap.RecordAdShown($"test_ad_{i}");
            }

            // Act
            bool reachedLimit = AdFrequencyCap.Instance.HasReachedDailyLimit;

            // Assert
            AssertBool(reachedLimit).IsTrue();
        }

        [TestCase]
        public void ResetDailyCounter_ShouldClearAdsShownToday()
        {
            // Arrange
            AdFrequencyCap.RecordAdShown("test_ad");
            AdFrequencyCap.RecordAdShown("test_ad2");

            // Act
            AdFrequencyCap.ResetDailyCounter();

            // Assert
            AssertInt(AdFrequencyCap.Instance.AdsShownToday).IsEqual(0);
        }

        [TestCase]
        public void GetCooldownRemaining_AfterRecording_ShouldReturnNonZero()
        {
            // Arrange
            AdFrequencyCap.RecordAdShown("test_ad");

            // Act
            float cooldown = AdFrequencyCap.GetCooldownRemaining("test_ad");

            // Assert
            // Cooldown should be close to 300 seconds (5 minutes)
            AssertFloat(cooldown).IsGreaterEqual(290f);
        }
    }
}
