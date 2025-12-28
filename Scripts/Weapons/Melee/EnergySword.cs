using Godot;
using System;
using MechDefenseHalo.Components;

namespace MechDefenseHalo.Weapons.Melee
{
    /// <summary>
    /// Fast melee weapon with medium damage.
    /// Energy sword with quick swing animations.
    /// </summary>
    public partial class EnergySword : WeaponBase
    {
        #region Exported Properties

        [Export] public float SwingAngle { get; set; } = 90f; // Degrees
        [Export] public float SwingRadius { get; set; } = 2f;

        #endregion

        #region Constructor

        public EnergySword()
        {
            WeaponName = "Energy Sword";
            BaseDamage = 50f;
            FireRate = 0.5f; // 2 swings per second
            Range = 2.5f;
            MaxAmmo = 100; // Energy charge
            ReloadTime = 0f; // No reload, recharges over time
            ElementType = ElementalType.Physical;
            IsAutomatic = false;
        }

        #endregion

        #region Protected Methods

        protected override void OnFire()
        {
            // Perform melee swing - check for enemies in range
            var enemies = GetEnemiesInRange();

            foreach (var enemy in enemies)
            {
                var healthComp = enemy.GetNodeOrNull<HealthComponent>("HealthComponent");
                if (healthComp != null)
                {
                    healthComp.TakeDamage(BaseDamage, this);
                    GD.Print($"Energy Sword slashed {enemy.Name} for {BaseDamage} damage");
                }
            }

            // Play swing animation
            PlaySwingAnimation();

            // Spawn swing effect
            SpawnSwingEffect();
        }

        #endregion

        #region Private Methods

        private Godot.Collections.Array<Node3D> GetEnemiesInRange()
        {
            var result = new Godot.Collections.Array<Node3D>();
            var enemies = GetTree().GetNodesInGroup("enemies");

            Vector3 playerPos = GlobalPosition;
            Vector3 forward = -GlobalTransform.Basis.Z;

            foreach (var enemy in enemies)
            {
                if (enemy is Node3D enemy3D)
                {
                    float distance = playerPos.DistanceTo(enemy3D.GlobalPosition);
                    if (distance <= Range)
                    {
                        // Check if enemy is in front of player (within swing arc)
                        Vector3 toEnemy = (enemy3D.GlobalPosition - playerPos).Normalized();
                        float angle = forward.AngleTo(toEnemy);
                        
                        if (Mathf.RadToDeg(angle) <= SwingAngle / 2f)
                        {
                            result.Add(enemy3D);
                        }
                    }
                }
            }

            return result;
        }

        private void PlaySwingAnimation()
        {
            // TODO: Implement sword swing animation
        }

        private void SpawnSwingEffect()
        {
            // TODO: Spawn energy sword trail effect
        }

        #endregion
    }
}
