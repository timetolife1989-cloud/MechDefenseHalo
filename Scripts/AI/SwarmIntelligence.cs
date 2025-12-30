using Godot;
using System.Collections.Generic;
using System.Linq;

namespace MechDefenseHalo.AI
{
    /// <summary>
    /// Manages swarm intelligence and coordinated enemy behavior.
    /// Handles spawn positioning, flanking, and group tactics.
    /// </summary>
    public partial class SwarmIntelligence : Node
    {
        private List<Node3D> activeEnemies = new();
        
        public override void _Ready()
        {
            GD.Print("SwarmIntelligence initialized");
        }
        
        /// <summary>
        /// Get the count of active allies
        /// </summary>
        public int GetAllyCount() => activeEnemies.Count;
        
        /// <summary>
        /// Calculate optimal spawn position based on enemy distribution and player location
        /// </summary>
        public Vector3 GetOptimalSpawnPosition()
        {
            Vector3 playerPos = GetPlayerPosition();
            
            // Analyze current enemy positions
            if (activeEnemies.Count == 0)
            {
                // No enemies, spawn near player
                return GetRandomPositionAroundPlayer(playerPos, 20f);
            }
            
            // Find gaps in coverage (flanking opportunity)
            Vector3 bestPosition = FindFlankingPosition(playerPos);
            
            return bestPosition;
        }
        
        /// <summary>
        /// Find a flanking position relative to player
        /// </summary>
        private Vector3 FindFlankingPosition(Vector3 playerPos)
        {
            // Calculate player's facing direction
            Vector3 playerForward = GetPlayerForwardDirection();
            
            // Spawn behind or to the side
            float angle = GD.Randf() > 0.5f ? 135f : -135f;
            Vector3 offset = playerForward.Rotated(Vector3.Up, Mathf.DegToRad(angle)) * 15f;
            
            return playerPos + offset;
        }
        
        /// <summary>
        /// Register an enemy to the swarm
        /// </summary>
        public void RegisterEnemy(Node3D enemy)
        {
            if (!activeEnemies.Contains(enemy))
            {
                activeEnemies.Add(enemy);
                enemy.TreeExiting += () => UnregisterEnemy(enemy);
            }
        }
        
        /// <summary>
        /// Unregister an enemy from the swarm
        /// </summary>
        private void UnregisterEnemy(Node3D enemy)
        {
            activeEnemies.Remove(enemy);
        }
        
        /// <summary>
        /// Get nearby allies within radius
        /// </summary>
        public List<Node3D> GetNearbyAllies(Vector3 position, float radius)
        {
            return activeEnemies.Where(e => 
                IsInstanceValid(e) && 
                e.GlobalPosition.DistanceTo(position) < radius
            ).ToList();
        }
        
        /// <summary>
        /// Get player position from scene tree
        /// </summary>
        private Vector3 GetPlayerPosition()
        {
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0 && players[0] is Node3D player)
            {
                return player.GlobalPosition;
            }
            return Vector3.Zero;
        }
        
        /// <summary>
        /// Get player forward direction
        /// </summary>
        private Vector3 GetPlayerForwardDirection()
        {
            var players = GetTree().GetNodesInGroup("player");
            if (players.Count > 0 && players[0] is Node3D player)
            {
                // Get player's forward direction (negative Z in Godot)
                return -player.GlobalTransform.Basis.Z;
            }
            // Default to negative Z (Godot's forward direction)
            return Vector3.Back;
        }
        
        /// <summary>
        /// Get random position around player
        /// </summary>
        private Vector3 GetRandomPositionAroundPlayer(Vector3 playerPos, float radius)
        {
            float angle = GD.Randf() * Mathf.Tau; // Random angle (0 to 2*PI)
            float distance = GD.Randf() * radius;
            
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * distance,
                0,
                Mathf.Sin(angle) * distance
            );
            
            return playerPos + offset;
        }
    }
}
