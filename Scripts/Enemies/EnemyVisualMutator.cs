using Godot;
using System;

namespace MechDefenseHalo.Enemies
{
    /// <summary>
    /// Applies shader-based visual variations to enemies
    /// </summary>
    public partial class EnemyVisualMutator : Node
    {
        private const int MAX_RARITY_VALUE = 5; // Number of rarity tiers (Common to Legendary)
        
        private ShaderMaterial colorVariationShader;
        private ShaderMaterial heatmapShader;
        private ShaderMaterial eliteGlowShader;
        
        public override void _Ready()
        {
            // Load shaders with null checks
            colorVariationShader = ResourceLoader.Load<ShaderMaterial>("res://Shaders/enemy_color_variation.tres");
            if (colorVariationShader == null)
            {
                GD.PrintErr("Failed to load enemy_color_variation.tres");
            }
            
            heatmapShader = ResourceLoader.Load<ShaderMaterial>("res://Shaders/enemy_heatmap.tres");
            if (heatmapShader == null)
            {
                GD.PrintErr("Failed to load enemy_heatmap.tres");
            }
            
            eliteGlowShader = ResourceLoader.Load<ShaderMaterial>("res://Shaders/elite_glow.tres");
            if (eliteGlowShader == null)
            {
                GD.PrintErr("Failed to load elite_glow.tres");
            }
        }
        
        public void MutateVisuals(Node3D enemy, EnemyStats stats, EnemyRarity rarity)
        {
            // Scale based on size stat
            enemy.Scale = Vector3.One * stats.Size;
            
            // Apply color variation shader
            ApplyColorVariation(enemy, stats.Archetype, rarity);
            
            // Elite enemies get glow effect
            if (rarity >= EnemyRarity.Rare)
            {
                ApplyEliteGlow(enemy, rarity);
            }
        }
        
        private void ApplyColorVariation(Node3D enemy, float archetype, EnemyRarity rarity)
        {
            var meshInstance = enemy.FindChild("MeshInstance3D") as MeshInstance3D;
            if (meshInstance == null || colorVariationShader == null) return;
            
            var material = colorVariationShader.Duplicate() as ShaderMaterial;
            
            // Color based on archetype (NOT fixed colors like red=strong)
            Color baseColor = ColorFromArchetype(archetype);
            
            // Rarity adds saturation/brightness
            float rarityBrightness = (float)rarity / MAX_RARITY_VALUE;
            baseColor = baseColor.Lightened(rarityBrightness * 0.3f);
            
            // Convert Color to Vector3 for shader parameter
            material.SetShaderParameter("base_color", new Vector3(baseColor.R, baseColor.G, baseColor.B));
            material.SetShaderParameter("variation_amount", GD.Randf());
            
            meshInstance.MaterialOverride = material;
        }
        
        private Color ColorFromArchetype(float archetype)
        {
            // Smooth gradient across spectrum - clamp values to [0,1] range
            return new Color(
                Mathf.Clamp(Mathf.Sin(archetype * Mathf.Pi) * 0.5f + 0.5f, 0f, 1f),
                Mathf.Clamp(Mathf.Cos(archetype * Mathf.Pi * 0.5f) * 0.5f + 0.5f, 0f, 1f),
                Mathf.Clamp(Mathf.Sin(archetype * Mathf.Pi * 1.5f) * 0.5f + 0.5f, 0f, 1f)
            );
        }
        
        private void ApplyEliteGlow(Node3D enemy, EnemyRarity rarity)
        {
            var meshInstance = enemy.FindChild("MeshInstance3D") as MeshInstance3D;
            if (meshInstance == null || eliteGlowShader == null) return;
            
            var glowMaterial = eliteGlowShader.Duplicate() as ShaderMaterial;
            
            Color glowColor = rarity switch
            {
                EnemyRarity.Rare => new Color(0.2f, 0.5f, 1.0f), // Blue
                EnemyRarity.Elite => new Color(0.8f, 0.2f, 1.0f), // Purple
                EnemyRarity.Legendary => new Color(1.0f, 0.8f, 0.0f), // Gold
                _ => Colors.White
            };
            
            glowMaterial.SetShaderParameter("glow_color", glowColor);
            glowMaterial.SetShaderParameter("glow_intensity", 2.0f);
            
            meshInstance.MaterialOverlay = glowMaterial;
        }
    }
}
