using Godot;
using System;

namespace MechDefenseHalo.Components
{
    /// <summary>
    /// Component for boss weak points that take bonus damage.
    /// Can be destroyed to disable or weaken the boss.
    /// </summary>
    public partial class WeakPointComponent : Node3D
    {
        #region Exported Properties

        [Export] public float DamageMultiplier { get; set; } = 2f; // 2x damage on weak points
        [Export] public float MaxHealth { get; set; } = 1000f;
        [Export] public bool CanBeDestroyed { get; set; } = true;
        [Export] public string WeakPointName { get; set; } = "Weak Point";

        #endregion

        #region Public Properties

        public float CurrentHealth { get; private set; }
        public bool IsDestroyed { get; private set; } = false;
        public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;

        #endregion

        #region Private Fields

        private Area3D _hitArea;
        private MeshInstance3D _visual;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            CurrentHealth = MaxHealth;

            // Setup collision area if not already present
            _hitArea = GetNodeOrNull<Area3D>("HitArea");
            if (_hitArea == null)
            {
                _hitArea = new Area3D();
                _hitArea.Name = "HitArea";
                AddChild(_hitArea);

                var shape = new CollisionShape3D();
                var sphereShape = new SphereShape3D();
                sphereShape.Radius = 0.5f;
                shape.Shape = sphereShape;
                _hitArea.AddChild(shape);
            }

            _visual = GetNodeOrNull<MeshInstance3D>("Visual");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Take damage to this weak point
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (IsDestroyed)
                return;

            // Apply weak point multiplier
            float finalDamage = damage * DamageMultiplier;
            CurrentHealth = Mathf.Max(0, CurrentHealth - finalDamage);

            GD.Print($"{WeakPointName} took {finalDamage} damage ({CurrentHealth}/{MaxHealth} remaining)");

            // Check if destroyed
            if (CanBeDestroyed && CurrentHealth <= 0)
            {
                Destroy();
            }
        }

        /// <summary>
        /// Check if a position is hitting this weak point
        /// </summary>
        public bool IsHit(Vector3 position, float radius = 0.5f)
        {
            if (IsDestroyed)
                return false;

            return GlobalPosition.DistanceTo(position) <= radius;
        }

        /// <summary>
        /// Destroy this weak point
        /// </summary>
        public void Destroy()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;
            
            // Hide visual
            if (_visual != null)
            {
                _visual.Visible = false;
            }

            // Disable collision
            if (_hitArea != null)
            {
                _hitArea.Monitorable = false;
                _hitArea.Monitoring = false;
            }

            GD.Print($"{WeakPointName} destroyed!");
            
            // Notify parent (boss) that weak point is destroyed
            var parent = GetParent();
            if (parent != null && parent.HasMethod("OnWeakPointDestroyed"))
            {
                parent.Call("OnWeakPointDestroyed", this);
            }
        }

        /// <summary>
        /// Reset weak point to full health
        /// </summary>
        public void Reset()
        {
            CurrentHealth = MaxHealth;
            IsDestroyed = false;

            if (_visual != null)
            {
                _visual.Visible = true;
            }

            if (_hitArea != null)
            {
                _hitArea.Monitorable = true;
                _hitArea.Monitoring = true;
            }
        }

        #endregion
    }
}
