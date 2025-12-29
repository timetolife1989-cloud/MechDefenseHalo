using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.AI;

namespace MechDefenseHalo.Tests.AI
{
    /// <summary>
    /// Unit tests for EnemyPersonality
    /// </summary>
    [TestSuite]
    public class EnemyPersonalityTests
    {
        [TestCase]
        public void GetIdealDistanceToPlayer_RangeZero_ReturnsMinDistance()
        {
            // Arrange
            var personality = new EnemyPersonality { Range = 0.0f };

            // Act
            float result = personality.GetIdealDistanceToPlayer();

            // Assert
            AssertFloat(result).IsEqual(3f);
        }

        [TestCase]
        public void GetIdealDistanceToPlayer_RangeOne_ReturnsMaxDistance()
        {
            // Arrange
            var personality = new EnemyPersonality { Range = 1.0f };

            // Act
            float result = personality.GetIdealDistanceToPlayer();

            // Assert
            AssertFloat(result).IsEqual(20f);
        }

        [TestCase]
        public void ShouldRetreat_LowHealth_HighCaution_ReturnsTrue()
        {
            // Arrange
            var personality = new EnemyPersonality 
            { 
                Caution = 0.9f,
                Aggression = 0.3f,
                Teamwork = 0.2f
            };

            // Act
            bool result = personality.ShouldRetreat(0.2f, 0);

            // Assert
            AssertBool(result).IsTrue();
        }

        [TestCase]
        public void ShouldRetreat_HighHealth_ReturnsFalse()
        {
            // Arrange
            var personality = new EnemyPersonality 
            { 
                Caution = 0.5f,
                Aggression = 0.5f
            };

            // Act
            bool result = personality.ShouldRetreat(0.8f, 0);

            // Assert
            AssertBool(result).IsFalse();
        }

        [TestCase]
        public void ShouldRetreat_LowHealth_HighTeamwork_WithAllies_ReturnsTrue()
        {
            // Arrange
            var personality = new EnemyPersonality 
            { 
                Caution = 0.7f,
                Teamwork = 0.9f
            };

            // Act
            bool result = personality.ShouldRetreat(0.3f, 3);

            // Assert
            AssertBool(result).IsTrue();
        }

        [TestCase]
        public void GetAttackDelay_HighAggression_ReturnsLowDelay()
        {
            // Arrange
            var personality = new EnemyPersonality { Aggression = 1.0f };

            // Act
            float result = personality.GetAttackDelay();

            // Assert
            AssertFloat(result).IsEqual(0.5f);
        }

        [TestCase]
        public void GetAttackDelay_LowAggression_ReturnsHighDelay()
        {
            // Arrange
            var personality = new EnemyPersonality { Aggression = 0.0f };

            // Act
            float result = personality.GetAttackDelay();

            // Assert
            AssertFloat(result).IsEqual(2f);
        }
    }
}
