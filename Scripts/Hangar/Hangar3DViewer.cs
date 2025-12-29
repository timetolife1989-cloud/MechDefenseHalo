using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Hangar
{
    /// <summary>
    /// Main 3D viewer control for inspecting enemies, weapons, and items in the hangar
    /// </summary>
    public partial class Hangar3DViewer : Control
    {
        [Export] private SubViewport viewport3D;
        [Export] private Node3D modelContainer;
        [Export] private ViewerCamera viewerCamera;
        [Export] private Panel statPanel;
        
        private Node3D currentModel;
        private ModelRotator rotator;
        private EnemyCodex codex;
        
        private bool isDragging = false;
        private Vector2 lastMousePos;
        
        public override void _Ready()
        {
            rotator = GetNodeOrNull<ModelRotator>("ModelRotator");
            if (rotator == null)
            {
                rotator = new ModelRotator();
                rotator.Name = "ModelRotator";
                AddChild(rotator);
            }
            
            codex = GetNodeOrNull<EnemyCodex>("EnemyCodex");
            if (codex == null)
            {
                codex = new EnemyCodex();
                codex.Name = "EnemyCodex";
                AddChild(codex);
            }
            
            ConnectSignals();
        }

        public override void _ExitTree()
        {
            DisconnectSignals();
        }
        
        private void ConnectSignals()
        {
            EventBus.On("ViewEnemy", OnViewEnemyHandler);
            EventBus.On("ViewWeapon", OnViewWeaponHandler);
            EventBus.On("ViewItem", OnViewItemHandler);
        }

        private void DisconnectSignals()
        {
            EventBus.Off("ViewEnemy", OnViewEnemyHandler);
            EventBus.Off("ViewWeapon", OnViewWeaponHandler);
            EventBus.Off("ViewItem", OnViewItemHandler);
        }

        private void OnViewEnemyHandler(object data)
        {
            if (data is string enemyId)
            {
                OnViewEnemy(enemyId);
            }
        }

        private void OnViewWeaponHandler(object data)
        {
            if (data is string weaponId)
            {
                OnViewWeapon(weaponId);
            }
        }

        private void OnViewItemHandler(object data)
        {
            if (data is string itemId)
            {
                OnViewItem(itemId);
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseButton)
            {
                if (mouseButton.ButtonIndex == MouseButton.Left)
                {
                    isDragging = mouseButton.Pressed;
                    lastMousePos = mouseButton.Position;
                }
                else if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                {
                    if (viewerCamera != null)
                    {
                        viewerCamera.ZoomIn();
                    }
                }
                else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                {
                    if (viewerCamera != null)
                    {
                        viewerCamera.ZoomOut();
                    }
                }
            }
            
            if (@event is InputEventMouseMotion mouseMotion && isDragging)
            {
                Vector2 delta = mouseMotion.Position - lastMousePos;
                if (rotator != null)
                {
                    rotator.RotateModel(delta);
                }
                lastMousePos = mouseMotion.Position;
            }
        }
        
        private void OnViewEnemy(string enemyId)
        {
            ClearCurrentModel();
            
            // Load enemy model
            var enemyData = codex?.GetEnemyData(enemyId);
            if (enemyData != null)
            {
                LoadModel(enemyData.ModelPath);
                DisplayStats(enemyData);
                PlayIdleAnimation(enemyData.AnimationPath);
            }
        }
        
        private void OnViewWeapon(string weaponId)
        {
            ClearCurrentModel();
            
            string modelPath = $"res://Models/Weapons/{weaponId}.tscn";
            LoadModel(modelPath);
            DisplayWeaponStats(weaponId);
        }
        
        private void OnViewItem(string itemId)
        {
            ClearCurrentModel();
            
            string modelPath = $"res://Models/Items/{itemId}.tscn";
            LoadModel(modelPath);
            DisplayItemStats(itemId);
        }
        
        private void LoadModel(string modelPath)
        {
            if (!ResourceLoader.Exists(modelPath))
            {
                GD.PrintErr($"Model not found: {modelPath}");
                return;
            }
            
            var modelScene = ResourceLoader.Load<PackedScene>(modelPath);
            if (modelScene != null)
            {
                currentModel = modelScene.Instantiate<Node3D>();
                
                if (modelContainer != null)
                {
                    modelContainer.AddChild(currentModel);
                }
                else
                {
                    GD.PrintErr("Model container not set");
                }
                
                // Center model
                CenterModel();
                
                // Enable rotation
                if (rotator != null)
                {
                    rotator.SetTarget(currentModel);
                }
            }
        }
        
        private void ClearCurrentModel()
        {
            if (currentModel != null)
            {
                currentModel.QueueFree();
                currentModel = null;
            }
        }
        
        private void CenterModel()
        {
            if (currentModel == null) return;
            
            // Calculate bounding box
            Aabb bounds = CalculateBounds(currentModel);
            Vector3 center = bounds.GetCenter();
            
            // Move to origin
            currentModel.Position = -center;
            
            // Adjust camera distance based on size
            float maxSize = Mathf.Max(bounds.Size.X, bounds.Size.Y, bounds.Size.Z);
            if (viewerCamera != null)
            {
                viewerCamera.SetDistance(maxSize * 2f);
            }
        }
        
        private Aabb CalculateBounds(Node3D node)
        {
            Aabb bounds = new Aabb();
            bool first = true;
            
            CalculateBoundsRecursive(node, ref bounds, ref first);
            
            return bounds;
        }

        private void CalculateBoundsRecursive(Node node, ref Aabb bounds, ref bool first)
        {
            if (node is MeshInstance3D mesh)
            {
                Aabb meshBounds = mesh.GetAabb();
                
                if (first)
                {
                    bounds = meshBounds;
                    first = false;
                }
                else
                {
                    bounds = bounds.Merge(meshBounds);
                }
            }

            foreach (var child in node.GetChildren())
            {
                CalculateBoundsRecursive(child, ref bounds, ref first);
            }
        }
        
        private void DisplayStats(EnemyData enemyData)
        {
            if (statPanel == null) return;
            
            var statsDisplay = statPanel.GetNodeOrNull<StatDisplayPanel>("StatDisplayPanel");
            if (statsDisplay == null)
            {
                statsDisplay = new StatDisplayPanel();
                statsDisplay.Name = "StatDisplayPanel";
                statPanel.AddChild(statsDisplay);
            }
            
            statsDisplay.ShowEnemyStats(enemyData);
        }
        
        private void DisplayWeaponStats(string weaponId)
        {
            if (statPanel == null) return;
            
            var statsDisplay = statPanel.GetNodeOrNull<StatDisplayPanel>("StatDisplayPanel");
            if (statsDisplay == null)
            {
                statsDisplay = new StatDisplayPanel();
                statsDisplay.Name = "StatDisplayPanel";
                statPanel.AddChild(statsDisplay);
            }
            
            var weaponData = WeaponDatabase.GetWeapon(weaponId);
            statsDisplay.ShowWeaponStats(weaponData);
        }
        
        private void DisplayItemStats(string itemId)
        {
            if (statPanel == null) return;
            
            var statsDisplay = statPanel.GetNodeOrNull<StatDisplayPanel>("StatDisplayPanel");
            if (statsDisplay == null)
            {
                statsDisplay = new StatDisplayPanel();
                statsDisplay.Name = "StatDisplayPanel";
                statPanel.AddChild(statsDisplay);
            }
            
            var itemData = ItemDatabase.GetItem(itemId);
            if (itemData != null)
            {
                var displayData = new ItemData
                {
                    Id = itemData.ItemID,
                    Name = itemData.DisplayName,
                    Description = itemData.Description,
                    Rarity = itemData.Rarity,
                    Type = itemData.GetType().Name,
                    ModelPath = $"res://Models/Items/{itemData.ItemID}.tscn"
                };
                
                statsDisplay.ShowItemStats(displayData);
            }
        }
        
        private void PlayIdleAnimation(string animationPath)
        {
            if (currentModel == null) return;
            
            var animPlayer = FindAnimationPlayer(currentModel);
            if (animPlayer != null && animPlayer.HasAnimation("idle"))
            {
                animPlayer.Play("idle");
            }
        }

        private AnimationPlayer FindAnimationPlayer(Node node)
        {
            if (node is AnimationPlayer animPlayer)
            {
                return animPlayer;
            }

            foreach (var child in node.GetChildren())
            {
                var result = FindAnimationPlayer(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
