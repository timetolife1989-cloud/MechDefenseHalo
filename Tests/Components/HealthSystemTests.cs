using Godot;
using GdUnit4;
using static GdUnit4.Assertions;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Tests.Components
{
    /// <summary>
    /// Unit tests for HealthSystem component
    /// Tests health, shield, damage, healing, death, and invincibility mechanics
    /// </summary>
    [TestSuite]
    public class HealthSystemTests
    {
        private HealthSystem healthSystem;
        private Node testParent;

        [Before]
        public void Setup()
        {
            // Create a test parent node and health system
            testParent = new Node();
            testParent.Name = "TestEntity";
            healthSystem = new HealthSystem();
            testParent.AddChild(healthSystem);
        }

        [After]
        public void Teardown()
        {
            if (testParent != null)
            {
                testParent.QueueFree();
            }
        }

        // ===== INITIALIZATION TESTS =====

        [TestCase]
        public void Initialize_DefaultValues_ShouldSetCorrectly()
        {
            // Act
            healthSystem._Ready();

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(healthSystem.MaxHealth);
            AssertBool(healthSystem.IsDead()).IsFalse();
            AssertBool(healthSystem.IsAlive()).IsTrue();
        }

        [TestCase]
        public void Initialize_WithShield_ShouldSetShieldToMax()
        {
            // Arrange
            healthSystem.HasShield = true;
            healthSystem.MaxShield = 50f;

            // Act
            healthSystem._Ready();

            // Assert
            AssertFloat(healthSystem.CurrentShield).IsEqual(50f);
        }

        // ===== DAMAGE TESTS =====

        [TestCase]
        public void TakeDamage_NormalDamage_ShouldReduceHealth()
        {
            // Arrange
            healthSystem._Ready();
            float initialHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.TakeDamage(25f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(initialHealth - 25f);
        }

        [TestCase]
        public void TakeDamage_ZeroDamage_ShouldNotChangeHealth()
        {
            // Arrange
            healthSystem._Ready();
            float initialHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.TakeDamage(0f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(initialHealth);
        }

        [TestCase]
        public void TakeDamage_NegativeDamage_ShouldNotChangeHealth()
        {
            // Arrange
            healthSystem._Ready();
            float initialHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.TakeDamage(-10f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(initialHealth);
        }

        [TestCase]
        public void TakeDamage_WithInvincible_ShouldNotTakeDamage()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.IsInvincible = true;
            float initialHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.TakeDamage(25f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(initialHealth);
        }

        [TestCase]
        public void TakeDamage_WhenDead_ShouldNotTakeDamage()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(200f); // Kill the entity
            AssertBool(healthSystem.IsDead()).IsTrue();

            // Act
            healthSystem.TakeDamage(25f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(0f);
        }

        [TestCase]
        public void TakeDamage_LethalDamage_ShouldTriggerDeath()
        {
            // Arrange
            healthSystem._Ready();
            AssertBool(healthSystem.IsAlive()).IsTrue();

            // Act
            healthSystem.TakeDamage(150f); // More than max health

            // Assert
            AssertBool(healthSystem.IsDead()).IsTrue();
            AssertFloat(healthSystem.CurrentHealth).IsEqual(0f);
        }

        // ===== SHIELD TESTS =====

        [TestCase]
        public void TakeDamage_WithShield_ShouldDamageShieldFirst()
        {
            // Arrange
            healthSystem.HasShield = true;
            healthSystem.MaxShield = 50f;
            healthSystem._Ready();
            float initialHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.TakeDamage(25f);

            // Assert
            AssertFloat(healthSystem.CurrentShield).IsEqual(25f);
            AssertFloat(healthSystem.CurrentHealth).IsEqual(initialHealth); // Health unchanged
        }

        [TestCase]
        public void TakeDamage_WithShield_ExceedsShield_ShouldDamageHealthToo()
        {
            // Arrange
            healthSystem.HasShield = true;
            healthSystem.MaxShield = 30f;
            healthSystem._Ready();
            float initialHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.TakeDamage(50f); // More than shield

            // Assert
            AssertFloat(healthSystem.CurrentShield).IsEqual(0f);
            AssertFloat(healthSystem.CurrentHealth).IsEqual(initialHealth - 20f); // 50 - 30 shield
        }

        [TestCase]
        public void RestoreShield_ValidAmount_ShouldIncreaseShield()
        {
            // Arrange
            healthSystem.HasShield = true;
            healthSystem.MaxShield = 50f;
            healthSystem._Ready();
            healthSystem.TakeDamage(30f); // Reduce shield to 20

            // Act
            healthSystem.RestoreShield(15f);

            // Assert
            AssertFloat(healthSystem.CurrentShield).IsEqual(35f);
        }

        [TestCase]
        public void RestoreShield_ExceedsMax_ShouldCapAtMax()
        {
            // Arrange
            healthSystem.HasShield = true;
            healthSystem.MaxShield = 50f;
            healthSystem._Ready();
            healthSystem.TakeDamage(30f);

            // Act
            healthSystem.RestoreShield(100f); // More than max

            // Assert
            AssertFloat(healthSystem.CurrentShield).IsEqual(50f);
        }

        [TestCase]
        public void RestoreShield_WithoutShield_ShouldNotRestore()
        {
            // Arrange
            healthSystem.HasShield = false;
            healthSystem._Ready();

            // Act
            healthSystem.RestoreShield(25f);

            // Assert - no exception, just no effect
            AssertBool(true).IsTrue();
        }

        // ===== HEALING TESTS =====

        [TestCase]
        public void Heal_ValidAmount_ShouldIncreaseHealth()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(40f);
            float currentHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.Heal(20f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(currentHealth + 20f);
        }

        [TestCase]
        public void Heal_ExceedsMax_ShouldCapAtMaxHealth()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(20f);

            // Act
            healthSystem.Heal(100f); // More than max

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(healthSystem.MaxHealth);
        }

        [TestCase]
        public void Heal_WhenDead_ShouldNotHeal()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(200f); // Kill
            AssertBool(healthSystem.IsDead()).IsTrue();

            // Act
            healthSystem.Heal(50f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(0f);
        }

        [TestCase]
        public void Heal_ZeroAmount_ShouldNotChangeHealth()
        {
            // Arrange
            healthSystem._Ready();
            float initialHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.Heal(0f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(initialHealth);
        }

        // ===== DEATH AND REVIVAL TESTS =====

        [TestCase]
        public void Die_WhenHealthZero_ShouldSetDeadState()
        {
            // Arrange
            healthSystem._Ready();

            // Act
            healthSystem.TakeDamage(100f);

            // Assert
            AssertBool(healthSystem.IsDead()).IsTrue();
            AssertBool(healthSystem.IsAlive()).IsFalse();
            AssertFloat(healthSystem.CurrentHealth).IsEqual(0f);
        }

        [TestCase]
        public void Revive_WhenDead_ShouldRestoreToMaxHealth()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(200f); // Kill
            AssertBool(healthSystem.IsDead()).IsTrue();

            // Act
            healthSystem.Revive();

            // Assert
            AssertBool(healthSystem.IsAlive()).IsTrue();
            AssertFloat(healthSystem.CurrentHealth).IsEqual(healthSystem.MaxHealth);
        }

        [TestCase]
        public void Revive_WithSpecificHealth_ShouldSetToAmount()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(200f); // Kill

            // Act
            healthSystem.Revive(50f);

            // Assert
            AssertBool(healthSystem.IsAlive()).IsTrue();
            AssertFloat(healthSystem.CurrentHealth).IsEqual(50f);
        }

        [TestCase]
        public void Revive_WithShield_ShouldRestoreShield()
        {
            // Arrange
            healthSystem.HasShield = true;
            healthSystem.MaxShield = 50f;
            healthSystem._Ready();
            healthSystem.TakeDamage(200f); // Kill

            // Act
            healthSystem.Revive();

            // Assert
            AssertFloat(healthSystem.CurrentShield).IsEqual(healthSystem.MaxShield);
        }

        [TestCase]
        public void Revive_WhenAlive_ShouldNotRevive()
        {
            // Arrange
            healthSystem._Ready();
            AssertBool(healthSystem.IsAlive()).IsTrue();
            float initialHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.Revive(50f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(initialHealth);
        }

        // ===== UTILITY METHODS TESTS =====

        [TestCase]
        public void GetHealthPercent_FullHealth_ShouldReturnOne()
        {
            // Arrange
            healthSystem._Ready();

            // Act
            float percent = healthSystem.GetHealthPercent();

            // Assert
            AssertFloat(percent).IsEqual(1.0f);
        }

        [TestCase]
        public void GetHealthPercent_HalfHealth_ShouldReturnHalf()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(50f);

            // Act
            float percent = healthSystem.GetHealthPercent();

            // Assert
            AssertFloat(percent).IsEqual(0.5f);
        }

        [TestCase]
        public void GetHealthPercent_ZeroHealth_ShouldReturnZero()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(200f);

            // Act
            float percent = healthSystem.GetHealthPercent();

            // Assert
            AssertFloat(percent).IsEqual(0.0f);
        }

        [TestCase]
        public void GetShieldPercent_FullShield_ShouldReturnOne()
        {
            // Arrange
            healthSystem.HasShield = true;
            healthSystem.MaxShield = 50f;
            healthSystem._Ready();

            // Act
            float percent = healthSystem.GetShieldPercent();

            // Assert
            AssertFloat(percent).IsEqual(1.0f);
        }

        [TestCase]
        public void SetMaxHealth_IncreaseMax_ShouldScaleCurrentHealth()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(50f); // 50/100 = 50%

            // Act
            healthSystem.SetMaxHealth(200f); // Should scale to 100/200 = 50%

            // Assert
            AssertFloat(healthSystem.MaxHealth).IsEqual(200f);
            AssertFloat(healthSystem.CurrentHealth).IsEqual(100f);
        }

        [TestCase]
        public void SetMaxHealth_DecreaseMax_ShouldScaleCurrentHealth()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(50f); // 50/100 = 50%

            // Act
            healthSystem.SetMaxHealth(60f); // Should scale to 30/60 = 50%

            // Assert
            AssertFloat(healthSystem.MaxHealth).IsEqual(60f);
            AssertFloat(healthSystem.CurrentHealth).IsEqual(30f);
        }

        // ===== DAMAGE TYPE TESTS =====

        [TestCase]
        public void TakeDamage_WithDamageType_ShouldAcceptDifferentTypes()
        {
            // Arrange
            healthSystem._Ready();
            float initialHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.TakeDamage(10f, "explosion");
            healthSystem.TakeDamage(10f, "fire");
            healthSystem.TakeDamage(10f, "normal");

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(initialHealth - 30f);
        }

        // ===== EDGE CASES =====

        [TestCase]
        public void TakeDamage_ExactlyMaxHealth_ShouldKill()
        {
            // Arrange
            healthSystem._Ready();

            // Act
            healthSystem.TakeDamage(healthSystem.MaxHealth);

            // Assert
            AssertBool(healthSystem.IsDead()).IsTrue();
            AssertFloat(healthSystem.CurrentHealth).IsEqual(0f);
        }

        [TestCase]
        public void TakeDamage_MultipleTimes_ShouldAccumulate()
        {
            // Arrange
            healthSystem._Ready();
            float initialHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.TakeDamage(10f);
            healthSystem.TakeDamage(20f);
            healthSystem.TakeDamage(15f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(initialHealth - 45f);
        }

        [TestCase]
        public void Heal_MultipleTimes_ShouldAccumulate()
        {
            // Arrange
            healthSystem._Ready();
            healthSystem.TakeDamage(60f);
            float currentHealth = healthSystem.CurrentHealth;

            // Act
            healthSystem.Heal(10f);
            healthSystem.Heal(15f);
            healthSystem.Heal(5f);

            // Assert
            AssertFloat(healthSystem.CurrentHealth).IsEqual(currentHealth + 30f);
        }
    }
}
