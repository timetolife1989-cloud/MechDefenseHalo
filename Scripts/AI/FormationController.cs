using Godot;
using System.Collections.Generic;

namespace MechDefenseHalo.AI
{
    /// <summary>
    /// Controls formation tactics for groups of enemies.
    /// This can be used for coordinated enemy attacks.
    /// </summary>
    public partial class FormationController : Node
    {
        [Export] public FormationType Formation { get; set; } = FormationType.Line;
        [Export] public float Spacing { get; set; } = 2f;
        
        private List<EnemyAIController> _members = new();
        
        public enum FormationType
        {
            Line,
            Column,
            Wedge,
            Circle,
            Scattered
        }
        
        public void AddMember(EnemyAIController enemy)
        {
            if (!_members.Contains(enemy))
            {
                _members.Add(enemy);
                UpdateFormation();
            }
        }
        
        public void RemoveMember(EnemyAIController enemy)
        {
            if (_members.Contains(enemy))
            {
                _members.Remove(enemy);
                UpdateFormation();
            }
        }
        
        public Vector3 GetFormationPosition(EnemyAIController enemy, Vector3 targetPosition)
        {
            int index = _members.IndexOf(enemy);
            if (index < 0)
                return targetPosition;
            
            return CalculateFormationOffset(index, targetPosition);
        }
        
        private void UpdateFormation()
        {
            // Recalculate formation positions for all members
            // This would be called when members join/leave
        }
        
        private Vector3 CalculateFormationOffset(int index, Vector3 center)
        {
            switch (Formation)
            {
                case FormationType.Line:
                    return center + new Vector3(index * Spacing, 0, 0);
                
                case FormationType.Column:
                    return center + new Vector3(0, 0, index * Spacing);
                
                case FormationType.Wedge:
                    int row = (int)Mathf.Floor(Mathf.Sqrt(index));
                    int col = index - row * row;
                    return center + new Vector3((col - row / 2f) * Spacing, 0, row * Spacing);
                
                case FormationType.Circle:
                    float angle = (index / (float)_members.Count) * Mathf.Tau;
                    float radius = Spacing * 2;
                    return center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                
                case FormationType.Scattered:
                    // Random offset
                    var rng = new RandomNumberGenerator();
                    rng.Seed = (ulong)index; // Deterministic randomness
                    float x = rng.RandfRange(-Spacing * 2, Spacing * 2);
                    float z = rng.RandfRange(-Spacing * 2, Spacing * 2);
                    return center + new Vector3(x, 0, z);
                
                default:
                    return center;
            }
        }
    }
}
