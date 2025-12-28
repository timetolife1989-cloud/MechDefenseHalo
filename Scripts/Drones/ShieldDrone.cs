using Godot;
using System;

namespace MechDefenseHalo.Drones
{
    /// <summary>
    /// Shield drone that creates a protective bubble around the player.
    /// Absorbs incoming damage.
    /// </summary>
    public partial class ShieldDrone : DroneBase
    {
        #region Exported Properties

        [Export] public float ShieldStrength { get; set; } = 100f;
        [Export] public float ShieldRadius { get; set; } = 5f;
        [Export] public float ShieldRegenRate { get; set; } = 5f; // HP per second

        #endregion

        #region Private Fields

        private float _currentShieldStrength;
        private Area3D _shieldArea;
        private MeshInstance3D _shieldBubble;

        #endregion

        #region Constructor

        public ShieldDrone()
        {
            DroneName = "Shield Drone";
            EnergyCost = 30f;
            Lifetime = 20f;
        }

        #endregion

        #region Protected Methods

        protected override Color GetDroneColor()
        {
            return new Color(0.3f, 0.3f, 1f); // Blue for shield
        }

        protected override void OnReady()
        {
            _currentShieldStrength = ShieldStrength;
            CreateShieldBubble();
        }

        protected override void OnUpdate(float delta)
        {
            // Regenerate shield
            if (_currentShieldStrength < ShieldStrength)
            {
                _currentShieldStrength = Mathf.Min(ShieldStrength, _currentShieldStrength + ShieldRegenRate * delta);
            }

            // Update shield visual opacity based on strength
            if (_shieldBubble != null)
            {
                var material = _shieldBubble.MaterialOverride as StandardMaterial3D;
                if (material != null)
                {
                    float alpha = 0.3f * (_currentShieldStrength / ShieldStrength);
                    var color = material.AlbedoColor;
                    color.A = alpha;
                    material.AlbedoColor = color;
                }
            }

            // Position shield at orbit target
            if (OrbitTarget != null && IsInstanceValid(OrbitTarget))
            {
                if (_shieldBubble != null)
                {
                    _shieldBubble.GlobalPosition = OrbitTarget.GlobalPosition;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Absorb damage to the shield
        /// </summary>
        public float AbsorbDamage(float damage)
        {
            if (_currentShieldStrength <= 0)
                return damage; // Shield depleted, pass through all damage

            float absorbed = Mathf.Min(damage, _currentShieldStrength);
            _currentShieldStrength -= absorbed;
            
            GD.Print($"Shield absorbed {absorbed} damage ({_currentShieldStrength}/{ShieldStrength} remaining)");

            if (_currentShieldStrength <= 0)
            {
                Deactivate();
            }

            return damage - absorbed; // Return remaining damage
        }

        #endregion

        #region Private Methods

        private void CreateShieldBubble()
        {
            _shieldBubble = new MeshInstance3D();
            _shieldBubble.Name = "ShieldBubble";

            var sphereMesh = new SphereMesh();
            sphereMesh.Radius = ShieldRadius;
            sphereMesh.Height = ShieldRadius * 2;
            _shieldBubble.Mesh = sphereMesh;

            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(0.3f, 0.3f, 1f, 0.3f);
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            material.EmissionEnabled = true;
            material.Emission = new Color(0.3f, 0.3f, 1f);
            material.EmissionEnergy = 0.5f;
            _shieldBubble.MaterialOverride = material;

            AddChild(_shieldBubble);
        }

        #endregion
    }
}
