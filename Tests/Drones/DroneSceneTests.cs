using Godot;
using GdUnit4;
using MechDefenseHalo.Drones;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Drones
{
    /// <summary>
    /// Unit tests for Drone Scene files
    /// Verifies that all drone scenes can be loaded and instantiated correctly
    /// </summary>
    [TestSuite]
    public class DroneSceneTests
    {
        private const string DRONE_SCENE_PATH = "res://Scenes/Drones/";

        [TestCase]
        public void AttackDroneScene_CanBeLoaded_AndInstantiated()
        {
            // Arrange
            var scenePath = DRONE_SCENE_PATH + "AttackDrone.tscn";
            var scene = GD.Load<PackedScene>(scenePath);

            // Assert scene loads
            AssertObject(scene).IsNotNull();

            // Act - Instantiate
            var drone = scene.Instantiate<AttackDrone>();

            // Assert
            AssertObject(drone).IsNotNull();
            AssertString(drone.DroneName).IsEqual("Attack Drone");
            AssertFloat(drone.EnergyCost).IsEqual(10.0f);
            AssertFloat(drone.Damage).IsEqual(15.0f);
            AssertFloat(drone.AttackRange).IsEqual(20.0f);

            // Cleanup
            drone.Free();
        }

        [TestCase]
        public void ShieldDroneScene_CanBeLoaded_AndInstantiated()
        {
            // Arrange
            var scenePath = DRONE_SCENE_PATH + "ShieldDrone.tscn";
            var scene = GD.Load<PackedScene>(scenePath);

            // Assert scene loads
            AssertObject(scene).IsNotNull();

            // Act - Instantiate
            var drone = scene.Instantiate<ShieldDrone>();

            // Assert
            AssertObject(drone).IsNotNull();
            AssertString(drone.DroneName).IsEqual("Shield Drone");
            AssertFloat(drone.EnergyCost).IsEqual(15.0f);
            AssertFloat(drone.ShieldStrength).IsEqual(100.0f);
            AssertFloat(drone.ShieldRadius).IsEqual(5.0f);

            // Cleanup
            drone.Free();
        }

        [TestCase]
        public void RepairDroneScene_CanBeLoaded_AndInstantiated()
        {
            // Arrange
            var scenePath = DRONE_SCENE_PATH + "RepairDrone.tscn";
            var scene = GD.Load<PackedScene>(scenePath);

            // Assert scene loads
            AssertObject(scene).IsNotNull();

            // Act - Instantiate
            var drone = scene.Instantiate<RepairDrone>();

            // Assert
            AssertObject(drone).IsNotNull();
            AssertString(drone.DroneName).IsEqual("Repair Drone");
            AssertFloat(drone.EnergyCost).IsEqual(12.0f);
            AssertFloat(drone.HealRate).IsEqual(10.0f);
            AssertFloat(drone.HealRange).IsEqual(8.0f);

            // Cleanup
            drone.Free();
        }

        [TestCase]
        public void EMPDroneScene_CanBeLoaded_AndInstantiated()
        {
            // Arrange
            var scenePath = DRONE_SCENE_PATH + "EMPDrone.tscn";
            var scene = GD.Load<PackedScene>(scenePath);

            // Assert scene loads
            AssertObject(scene).IsNotNull();

            // Act - Instantiate
            var drone = scene.Instantiate<EMPDrone>();

            // Assert
            AssertObject(drone).IsNotNull();
            AssertString(drone.DroneName).IsEqual("EMP Drone");
            AssertFloat(drone.EnergyCost).IsEqual(20.0f);
            AssertFloat(drone.EMPRange).IsEqual(10.0f);
            AssertFloat(drone.SlowAmount).IsEqual(0.5f);

            // Cleanup
            drone.Free();
        }

        [TestCase]
        public void BomberDroneScene_CanBeLoaded_AndInstantiated()
        {
            // Arrange
            var scenePath = DRONE_SCENE_PATH + "BomberDrone.tscn";
            var scene = GD.Load<PackedScene>(scenePath);

            // Assert scene loads
            AssertObject(scene).IsNotNull();

            // Act - Instantiate
            var drone = scene.Instantiate<BomberDrone>();

            // Assert
            AssertObject(drone).IsNotNull();
            AssertString(drone.DroneName).IsEqual("Bomber Drone");
            AssertFloat(drone.EnergyCost).IsEqual(25.0f);
            AssertFloat(drone.ExplosionDamage).IsEqual(100.0f);
            AssertFloat(drone.ExplosionRadius).IsEqual(8.0f);

            // Cleanup
            drone.Free();
        }

        [TestCase]
        public void AllDroneScenes_HaveRequiredChildren()
        {
            // Test that all drones have required child nodes
            var droneScenes = new[]
            {
                "AttackDrone.tscn",
                "ShieldDrone.tscn",
                "RepairDrone.tscn",
                "EMPDrone.tscn",
                "BomberDrone.tscn"
            };

            foreach (var sceneName in droneScenes)
            {
                // Arrange
                var scenePath = DRONE_SCENE_PATH + sceneName;
                var scene = GD.Load<PackedScene>(scenePath);
                var drone = scene.Instantiate<DroneBase>();

                // Assert required children exist
                var modelMount = drone.GetNodeOrNull<Node3D>("ModelMount");
                var detectionRange = drone.GetNodeOrNull<Area3D>("DetectionRange");
                var attackPoint = drone.GetNodeOrNull<Node3D>("AttackPoint");
                var controller = drone.GetNodeOrNull<Node>("DroneControllerComponent");
                var orbitCenter = drone.GetNodeOrNull<Marker3D>("OrbitCenter");

                AssertObject(modelMount).IsNotNull();
                AssertObject(detectionRange).IsNotNull();
                AssertObject(attackPoint).IsNotNull();
                AssertObject(controller).IsNotNull();
                AssertObject(orbitCenter).IsNotNull();

                // Cleanup
                drone.Free();
            }
        }

        [TestCase]
        public void AllDroneScenes_HaveCorrectScriptTypes()
        {
            // Verify each scene has the correct script type attached
            var droneTestData = new[]
            {
                ("AttackDrone.tscn", typeof(AttackDrone)),
                ("ShieldDrone.tscn", typeof(ShieldDrone)),
                ("RepairDrone.tscn", typeof(RepairDrone)),
                ("EMPDrone.tscn", typeof(EMPDrone)),
                ("BomberDrone.tscn", typeof(BomberDrone))
            };

            foreach (var (sceneName, expectedType) in droneTestData)
            {
                var scenePath = DRONE_SCENE_PATH + sceneName;
                var scene = GD.Load<PackedScene>(scenePath);
                var drone = scene.Instantiate();
                
                AssertBool(drone.GetType() == expectedType).IsTrue();
                
                drone.Free();
            }
        }
    }
}
