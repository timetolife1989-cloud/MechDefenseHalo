using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Enemies;

namespace MechDefenseHalo.Tests.Enemies
{
    /// <summary>
    /// Unit tests for EnemyStatMixer
    /// </summary>
    [TestSuite]
    public class EnemyStatMixerTests
    {
        private EnemyStatMixer _statMixer;

        [Before]
        public void Setup()
        {
            _statMixer = new EnemyStatMixer();
        }

        [After]
        public void Cleanup()
        {
            _statMixer?.Free();
        }

        #region Stat Generation Tests

        [TestCase]
        public void GenerateStats_Common_ReturnsValidStats()
        {
            // Act
            var stats = _statMixer.GenerateStats(EnemyRarity.Common);

            // Assert
            AssertThat(stats.HP).IsGreater(0);
            AssertThat(stats.Damage).IsGreater(0);
            AssertThat(stats.Speed).IsGreater(0);
            AssertThat(stats.Range).IsGreater(0);
            AssertThat(stats.Size).IsGreater(0);
            AssertThat(stats.Archetype).IsBetween(0.0f, 1.0f);
        }

        [TestCase]
        public void GenerateStats_Legendary_HasHigherStatsThanCommon()
        {
            // Arrange
            var commonStats = _statMixer.GenerateStats(EnemyRarity.Common);
            var legendaryStats = _statMixer.GenerateStats(EnemyRarity.Legendary);

            // Assert - Legendary should generally have higher stats due to 5x multiplier
            // Note: Due to archetype trade-offs, we can't guarantee every stat is higher
            // but total power (HP + Damage) should be higher
            int commonPower = commonStats.HP + commonStats.Damage;
            int legendaryPower = legendaryStats.HP + legendaryStats.Damage;
            AssertThat(legendaryPower).IsGreater(commonPower);
        }

        [TestCase]
        public void GenerateStats_Multiple_ProducesDifferentStats()
        {
            // Act
            var stats1 = _statMixer.GenerateStats(EnemyRarity.Common);
            var stats2 = _statMixer.GenerateStats(EnemyRarity.Common);
            var stats3 = _statMixer.GenerateStats(EnemyRarity.Common);

            // Assert - Due to randomness, at least one stat should be different
            bool isDifferent = stats1.HP != stats2.HP ||
                              stats1.Damage != stats2.Damage ||
                              stats2.HP != stats3.HP ||
                              stats2.Damage != stats3.Damage;
            AssertBool(isDifferent).IsTrue();
        }

        [TestCase]
        public void GenerateStats_Range_IsWithinExpectedBounds()
        {
            // Act
            var stats = _statMixer.GenerateStats(EnemyRarity.Common);

            // Assert
            AssertThat(stats.Range).IsBetween(3f, 20f);
        }

        [TestCase]
        public void GenerateStats_Size_IsWithinExpectedBounds()
        {
            // Act
            var stats = _statMixer.GenerateStats(EnemyRarity.Common);

            // Assert
            AssertThat(stats.Size).IsBetween(0.8f, 1.5f);
        }

        #endregion

        #region Rarity Multiplier Tests

        [TestCase]
        public void GenerateStats_Uncommon_HasHigherMultiplierThanCommon()
        {
            // Act - Generate multiple to average out variance
            int commonTotal = 0;
            int uncommonTotal = 0;
            
            for (int i = 0; i < 10; i++)
            {
                var commonStats = _statMixer.GenerateStats(EnemyRarity.Common);
                var uncommonStats = _statMixer.GenerateStats(EnemyRarity.Uncommon);
                commonTotal += commonStats.HP + commonStats.Damage;
                uncommonTotal += uncommonStats.HP + uncommonStats.Damage;
            }

            // Assert
            AssertThat(uncommonTotal).IsGreater(commonTotal);
        }

        [TestCase]
        public void GenerateStats_Rare_HasHigherMultiplierThanUncommon()
        {
            // Act - Generate multiple to average out variance
            int uncommonTotal = 0;
            int rareTotal = 0;
            
            for (int i = 0; i < 10; i++)
            {
                var uncommonStats = _statMixer.GenerateStats(EnemyRarity.Uncommon);
                var rareStats = _statMixer.GenerateStats(EnemyRarity.Rare);
                uncommonTotal += uncommonStats.HP + uncommonStats.Damage;
                rareTotal += rareStats.HP + rareStats.Damage;
            }

            // Assert
            AssertThat(rareTotal).IsGreater(uncommonTotal);
        }

        #endregion

        #region Archetype Tests

        private const float GLASS_CANNON_DAMAGE_TO_HP_RATIO = 10f;
        private const float TANK_HP_TO_DAMAGE_RATIO = 10f;

        [TestCase]
        public void GenerateStats_Archetype_AffectsStatDistribution()
        {
            // Act - Generate many enemies to find different archetypes
            EnemyStats? glassCannonLike = null;
            EnemyStats? tankLike = null;

            for (int i = 0; i < 50; i++)
            {
                var stats = _statMixer.GenerateStats(EnemyRarity.Common);
                
                // Look for glass cannon (high damage relative to HP)
                if (stats.Damage > stats.HP / GLASS_CANNON_DAMAGE_TO_HP_RATIO)
                {
                    glassCannonLike = stats;
                }
                
                // Look for tank (high HP relative to damage)
                if (stats.HP > stats.Damage * TANK_HP_TO_DAMAGE_RATIO)
                {
                    tankLike = stats;
                }

                if (glassCannonLike.HasValue && tankLike.HasValue)
                    break;
            }

            // Assert - We should find different archetypes
            AssertBool(glassCannonLike.HasValue).IsTrue();
            AssertBool(tankLike.HasValue).IsTrue();
        }

        #endregion
    }
}
