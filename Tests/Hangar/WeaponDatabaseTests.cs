using Godot;
using GdUnit4;
using MechDefenseHalo.Hangar;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Hangar
{
    /// <summary>
    /// Unit tests for WeaponDatabase
    /// </summary>
    [TestSuite]
    public class WeaponDatabaseTests
    {
        [TestCase]
        public void GetWeapon_WithValidId_ShouldReturnWeaponData()
        {
            // Act
            var weapon = WeaponDatabase.GetWeapon("assault_rifle");

            // Assert
            AssertObject(weapon).IsNotNull();
            AssertString(weapon.Name).IsEqual("Assault Rifle");
            AssertString(weapon.Id).IsEqual("assault_rifle");
            AssertInt(weapon.Damage).IsGreater(0);
            AssertFloat(weapon.FireRate).IsGreater(0);
        }

        [TestCase]
        public void GetWeapon_WithInvalidId_ShouldReturnDefaultWeapon()
        {
            // Act
            var weapon = WeaponDatabase.GetWeapon("nonexistent_weapon");

            // Assert
            AssertObject(weapon).IsNotNull();
            AssertString(weapon.Name).IsEqual("Unknown Weapon");
            AssertInt(weapon.Damage).IsEqual(0);
        }

        [TestCase]
        public void GetWeapon_MultipleWeapons_ShouldHaveDifferentStats()
        {
            // Act
            var rifle = WeaponDatabase.GetWeapon("assault_rifle");
            var cannon = WeaponDatabase.GetWeapon("plasma_cannon");

            // Assert - Weapons should have different characteristics
            AssertThat(rifle.Damage).IsNotEqual(cannon.Damage);
        }

        [TestCase]
        public void GetWeapon_ShouldIncludeModelPath()
        {
            // Act
            var weapon = WeaponDatabase.GetWeapon("assault_rifle");

            // Assert
            AssertString(weapon.ModelPath).IsNotEmpty();
            AssertString(weapon.ModelPath).Contains("res://");
        }

        [TestCase]
        public void GetWeapon_AccuracyStat_ShouldBeValidPercentage()
        {
            // Act
            var weapon = WeaponDatabase.GetWeapon("assault_rifle");

            // Assert - Accuracy should be between 0 and 1
            AssertFloat(weapon.Accuracy).IsGreaterEqual(0f);
            AssertFloat(weapon.Accuracy).IsLessEqual(1f);
        }
    }
}
