using Godot;
using System;

namespace MechDefenseHalo.Hangar
{
    /// <summary>
    /// UI panel for displaying stats of enemies, weapons, and items
    /// </summary>
    public partial class StatDisplayPanel : Panel
    {
        private Label nameLabel;
        private Label descriptionLabel;
        private VBoxContainer statsContainer;
        
        public override void _Ready()
        {
            nameLabel = GetNodeOrNull<Label>("NameLabel");
            descriptionLabel = GetNodeOrNull<Label>("DescriptionLabel");
            statsContainer = GetNodeOrNull<VBoxContainer>("StatsContainer");
            
            // Create nodes if they don't exist
            if (nameLabel == null)
            {
                nameLabel = new Label();
                nameLabel.Name = "NameLabel";
                AddChild(nameLabel);
            }
            
            if (descriptionLabel == null)
            {
                descriptionLabel = new Label();
                descriptionLabel.Name = "DescriptionLabel";
                AddChild(descriptionLabel);
            }
            
            if (statsContainer == null)
            {
                statsContainer = new VBoxContainer();
                statsContainer.Name = "StatsContainer";
                AddChild(statsContainer);
            }
        }
        
        public void ShowEnemyStats(EnemyData enemy)
        {
            if (enemy == null) return;
            
            nameLabel.Text = enemy.Name;
            descriptionLabel.Text = enemy.Description ?? "";
            
            ClearStats();
            
            AddStat("HP", enemy.HP.ToString());
            AddStat("Damage", enemy.Damage.ToString());
            AddStat("Speed", enemy.Speed.ToString("F1"));
            AddStat("Kills", enemy.KillCount.ToString());
        }
        
        public void ShowWeaponStats(WeaponData weapon)
        {
            if (weapon == null) return;
            
            nameLabel.Text = weapon.Name;
            descriptionLabel.Text = weapon.Description ?? "";
            
            ClearStats();
            
            AddStat("Damage", weapon.Damage.ToString());
            AddStat("Fire Rate", weapon.FireRate.ToString("F2"));
            AddStat("Accuracy", $"{weapon.Accuracy * 100}%");
            AddStat("Range", weapon.Range.ToString());
        }
        
        public void ShowItemStats(ItemData item)
        {
            if (item == null) return;
            
            nameLabel.Text = item.Name;
            descriptionLabel.Text = item.Description ?? "";
            
            ClearStats();
            
            AddStat("Rarity", item.Rarity.ToString());
            AddStat("Type", item.Type ?? "Unknown");
        }
        
        private void ClearStats()
        {
            if (statsContainer == null) return;
            
            foreach (var child in statsContainer.GetChildren())
            {
                child.QueueFree();
            }
        }
        
        private void AddStat(string label, string value)
        {
            if (statsContainer == null) return;
            
            var statRow = new HBoxContainer();
            
            var labelNode = new Label();
            labelNode.Text = label + ":";
            labelNode.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            
            var valueNode = new Label();
            valueNode.Text = value;
            valueNode.HorizontalAlignment = HorizontalAlignment.Right;
            
            statRow.AddChild(labelNode);
            statRow.AddChild(valueNode);
            statsContainer.AddChild(statRow);
        }
    }
}
