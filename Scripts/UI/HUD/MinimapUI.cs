using Godot;
using System;
using System.Collections.Generic;

namespace MechDefenseHalo.UI.HUD
{
    /// <summary>
    /// Minimap UI component with enemy and objective markers
    /// Shows player position, enemies, and objectives on a top-down view
    /// </summary>
    public partial class MinimapUI : Control
    {
        #region Exported Properties
        
        [Export] public NodePath MinimapViewportPath { get; set; }
        [Export] public NodePath PlayerMarkerPath { get; set; }
        [Export] public float MapScale { get; set; } = 10f;
        [Export] public float MapRadius { get; set; } = 100f;
        [Export] public bool ShowEnemies { get; set; } = true;
        [Export] public bool ShowObjectives { get; set; } = true;
        [Export] public float MarkerCleanupInterval { get; set; } = 1f; // Seconds between cleanup
        [Export] public Color EnemyColor { get; set; } = Colors.Red;
        [Export] public Color ObjectiveColor { get; set; } = Colors.Yellow;
        [Export] public Color AllyColor { get; set; } = Colors.Green;
        
        #endregion
        
        #region Private Fields
        
        private Control _minimapViewport;
        private Control _playerMarker;
        private Node3D _player;
        
        private List<MinimapMarker> _enemyMarkers = new List<MinimapMarker>();
        private List<MinimapMarker> _objectiveMarkers = new List<MinimapMarker>();
        
        private float _cleanupTimer = 0f;
        
        private PackedScene _markerScene;
        
        #endregion
        
        #region Godot Lifecycle
        
        public override void _Ready()
        {
            // Get UI nodes
            _minimapViewport = GetNodeOrNull<Control>(MinimapViewportPath);
            _playerMarker = GetNodeOrNull<Control>(PlayerMarkerPath);
            
            // Try to find player node
            FindPlayer();
            
            // Create marker scene programmatically if needed
            CreateMarkerScene();
            
            GD.Print("MinimapUI initialized");
        }
        
        public override void _Process(double delta)
        {
            UpdatePlayerMarker();
            UpdateEnemyMarkers();
            UpdateObjectiveMarkers();
            
            // Periodic cleanup of invalid markers
            _cleanupTimer += (float)delta;
            if (_cleanupTimer >= MarkerCleanupInterval)
            {
                CleanupInvalidMarkers();
                _cleanupTimer = 0f;
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Set the player node to track
        /// </summary>
        public void SetPlayer(Node3D player)
        {
            _player = player;
        }
        
        /// <summary>
        /// Add an enemy marker
        /// </summary>
        public void AddEnemyMarker(Node3D enemy)
        {
            if (!ShowEnemies || _minimapViewport == null)
                return;
            
            var marker = CreateMarker(enemy, EnemyColor);
            _enemyMarkers.Add(marker);
        }
        
        /// <summary>
        /// Remove an enemy marker
        /// </summary>
        public void RemoveEnemyMarker(Node3D enemy)
        {
            var marker = _enemyMarkers.Find(m => m.Target == enemy);
            if (marker != null)
            {
                marker.MarkerNode?.QueueFree();
                _enemyMarkers.Remove(marker);
            }
        }
        
        /// <summary>
        /// Add an objective marker
        /// </summary>
        public void AddObjectiveMarker(Node3D objective)
        {
            if (!ShowObjectives || _minimapViewport == null)
                return;
            
            var marker = CreateMarker(objective, ObjectiveColor);
            _objectiveMarkers.Add(marker);
        }
        
        /// <summary>
        /// Remove an objective marker
        /// </summary>
        public void RemoveObjectiveMarker(Node3D objective)
        {
            var marker = _objectiveMarkers.Find(m => m.Target == objective);
            if (marker != null)
            {
                marker.MarkerNode?.QueueFree();
                _objectiveMarkers.Remove(marker);
            }
        }
        
        /// <summary>
        /// Clear all markers
        /// </summary>
        public void ClearAllMarkers()
        {
            foreach (var marker in _enemyMarkers)
            {
                marker.MarkerNode?.QueueFree();
            }
            _enemyMarkers.Clear();
            
            foreach (var marker in _objectiveMarkers)
            {
                marker.MarkerNode?.QueueFree();
            }
            _objectiveMarkers.Clear();
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Find player node in scene tree
        /// </summary>
        private void FindPlayer()
        {
            var root = GetTree().Root;
            _player = FindNodeByName(root, "Player") as Node3D;
            
            if (_player == null)
            {
                GD.PrintErr("MinimapUI: Player node not found!");
            }
        }
        
        /// <summary>
        /// Recursively find node by name
        /// </summary>
        private Node FindNodeByName(Node node, string name)
        {
            if (node.Name == name)
                return node;
            
            foreach (Node child in node.GetChildren())
            {
                var found = FindNodeByName(child, name);
                if (found != null)
                    return found;
            }
            
            return null;
        }
        
        /// <summary>
        /// Create marker scene
        /// </summary>
        private void CreateMarkerScene()
        {
            // This could load a scene file, but for simplicity we'll create markers dynamically
        }
        
        /// <summary>
        /// Create a new marker
        /// </summary>
        private MinimapMarker CreateMarker(Node3D target, Color color)
        {
            var markerNode = new ColorRect();
            markerNode.Size = new Vector2(4, 4);
            markerNode.Color = color;
            markerNode.Position = Vector2.Zero;
            
            _minimapViewport?.AddChild(markerNode);
            
            return new MinimapMarker
            {
                Target = target,
                MarkerNode = markerNode
            };
        }
        
        /// <summary>
        /// Update player marker position and rotation
        /// </summary>
        private void UpdatePlayerMarker()
        {
            if (_player == null || _playerMarker == null || _minimapViewport == null)
                return;
            
            // Center player marker
            var viewportSize = _minimapViewport.Size;
            _playerMarker.Position = viewportSize / 2;
        }
        
        /// <summary>
        /// Update all enemy markers
        /// </summary>
        private void UpdateEnemyMarkers()
        {
            if (_player == null || _minimapViewport == null)
                return;
            
            var viewportSize = _minimapViewport.Size;
            var viewportCenter = viewportSize / 2;
            
            foreach (var marker in _enemyMarkers)
            {
                if (marker.MarkerNode != null && IsInstanceValid(marker.Target))
                {
                    var worldPos = WorldToMinimap(marker.Target.GlobalPosition);
                    marker.MarkerNode.Position = viewportCenter + worldPos;
                    
                    // Hide if out of range
                    marker.MarkerNode.Visible = worldPos.Length() <= MapRadius;
                }
            }
        }
        
        /// <summary>
        /// Update all objective markers
        /// </summary>
        private void UpdateObjectiveMarkers()
        {
            if (_player == null || _minimapViewport == null)
                return;
            
            var viewportSize = _minimapViewport.Size;
            var viewportCenter = viewportSize / 2;
            
            foreach (var marker in _objectiveMarkers)
            {
                if (marker.MarkerNode != null && IsInstanceValid(marker.Target))
                {
                    var worldPos = WorldToMinimap(marker.Target.GlobalPosition);
                    marker.MarkerNode.Position = viewportCenter + worldPos;
                    
                    // Always show objectives
                    marker.MarkerNode.Visible = true;
                }
            }
        }
        
        /// <summary>
        /// Clean up invalid markers (called periodically, not every frame)
        /// </summary>
        private void CleanupInvalidMarkers()
        {
            CleanupMarkerList(_enemyMarkers);
            CleanupMarkerList(_objectiveMarkers);
        }
        
        /// <summary>
        /// Clean up invalid markers from a list
        /// </summary>
        private void CleanupMarkerList(List<MinimapMarker> markers)
        {
            markers.RemoveAll(m => !IsInstanceValid(m.Target) || m.Target.IsQueuedForDeletion());
        }
        
        /// <summary>
        /// Convert world position to minimap position
        /// </summary>
        private Vector2 WorldToMinimap(Vector3 worldPos)
        {
            if (_player == null)
                return Vector2.Zero;
            
            var playerPos = _player.GlobalPosition;
            var relativePos = worldPos - playerPos;
            
            // Convert 3D to 2D (top-down view)
            var minimapPos = new Vector2(relativePos.X, -relativePos.Z) / MapScale;
            
            return minimapPos;
        }
        
        #endregion
        
        #region Helper Classes
        
        /// <summary>
        /// Minimap marker data
        /// </summary>
        private class MinimapMarker
        {
            public Node3D Target { get; set; }
            public Control MarkerNode { get; set; }
        }
        
        #endregion
    }
}
