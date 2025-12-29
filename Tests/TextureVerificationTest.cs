using Godot;
using GdUnit4;
using MechDefenseHalo.Items;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests
{
    /// <summary>
    /// Unit tests to verify all UI texture placeholders are accessible
    /// 
    /// SETUP:
    /// 1. Install GdUnit4 from Asset Library
    /// 2. Enable plugin in Project Settings
    /// 3. Run: GdUnit4 > Run Tests
    /// </summary>
    [TestSuite]
    public class TextureVerificationTest
    {
        [TestCase]
        public void RarityBorders_AllLoadSuccessfully()
        {
            // Arrange
            var rarityNames = new[] { "common", "uncommon", "rare", "epic", "legendary", "exotic", "mythic" };

            // Act & Assert
            foreach (var rarityName in rarityNames)
            {
                var texture = GD.Load<Texture2D>($"res://Assets/Textures/UI/Rarity/border_{rarityName}.png");
                AssertThat(texture).IsNotNull($"Rarity border texture '{rarityName}' should load");
                AssertThat(texture.GetWidth()).IsGreater(0, $"Texture '{rarityName}' should have valid width");
                AssertThat(texture.GetHeight()).IsGreater(0, $"Texture '{rarityName}' should have valid height");
            }
        }

        [TestCase]
        public void CategoryIcons_AllLoadSuccessfully()
        {
            // Arrange
            var iconNames = new[] 
            { 
                "icon_weapon", "icon_armor", "icon_drone", "icon_consumable", 
                "icon_material", "icon_cosmetic", "icon_currency_credits", "icon_currency_cores" 
            };

            // Act & Assert
            foreach (var iconName in iconNames)
            {
                var texture = GD.Load<Texture2D>($"res://Assets/Textures/UI/Icons/{iconName}.png");
                AssertThat(texture).IsNotNull($"Icon texture '{iconName}' should load");
                AssertThat(texture.GetWidth()).IsEqual(64, $"Icon '{iconName}' should be 64x64");
                AssertThat(texture.GetHeight()).IsEqual(64, $"Icon '{iconName}' should be 64x64");
            }
        }

        [TestCase]
        public void UIElements_AllLoadSuccessfully()
        {
            // Arrange
            var elementPaths = new Dictionary<string, (int width, int height)>
            {
                { "button_normal", (128, 48) },
                { "button_hover", (128, 48) },
                { "button_pressed", (128, 48) },
                { "panel_background", (256, 256) },
                { "slot_background", (70, 70) },
                { "crosshair", (32, 32) },
                { "healthbar_fill", (200, 20) }
            };

            // Act & Assert
            foreach (var (elementName, expectedSize) in elementPaths)
            {
                var texture = GD.Load<Texture2D>($"res://Assets/Textures/UI/Elements/{elementName}.png");
                AssertThat(texture).IsNotNull($"UI element texture '{elementName}' should load");
                AssertThat(texture.GetWidth()).IsEqual(expectedSize.width, 
                    $"Element '{elementName}' should have width {expectedSize.width}");
                AssertThat(texture.GetHeight()).IsEqual(expectedSize.height, 
                    $"Element '{elementName}' should have height {expectedSize.height}");
            }
        }

        [TestCase]
        public void PlaceholderItems_AllLoadSuccessfully()
        {
            // Arrange
            var itemNames = new[]
            {
                "placeholder_weapon", "placeholder_armor", "placeholder_consumable",
                "placeholder_drone", "placeholder_cosmetic", "placeholder_weaponmod",
                "placeholder_mechpart", "placeholder_material_common", "placeholder_material_metal",
                "placeholder_material_crystal", "placeholder_material_organic", "placeholder_material_tech"
            };

            // Act & Assert
            foreach (var itemName in itemNames)
            {
                var texture = GD.Load<Texture2D>($"res://Assets/Textures/Items/{itemName}.png");
                AssertThat(texture).IsNotNull($"Item placeholder texture '{itemName}' should load");
                AssertThat(texture.GetWidth()).IsEqual(64, $"Item '{itemName}' should be 64x64");
                AssertThat(texture.GetHeight()).IsEqual(64, $"Item '{itemName}' should be 64x64");
            }
        }

        [TestCase]
        public void GetRarityBorder_ReturnsCorrectTexture()
        {
            // Arrange
            var rarities = new[] 
            { 
                ItemRarity.Common, ItemRarity.Uncommon, ItemRarity.Rare, 
                ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Exotic, ItemRarity.Mythic 
            };

            // Act & Assert
            foreach (var rarity in rarities)
            {
                var texture = GetRarityBorder(rarity);
                AssertThat(texture).IsNotNull($"Rarity border for {rarity} should load");
            }
        }

        /// <summary>
        /// Helper method to get rarity border texture for a specific rarity level
        /// </summary>
        /// <param name="rarity">Item rarity</param>
        /// <returns>Border texture</returns>
        private static Texture2D GetRarityBorder(ItemRarity rarity)
        {
            string rarityName = rarity.ToString().ToLower();
            return GD.Load<Texture2D>($"res://Assets/Textures/UI/Rarity/border_{rarityName}.png");
        }
    }
}
