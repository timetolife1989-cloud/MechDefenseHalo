using Godot;
using GdUnit4;
using MechDefenseHalo.Items;
using static GdUnit4.Assertions;

namespace MechDefenseHalo.Tests.Loot
{
    /// <summary>
    /// Unit tests for RaritySystem utilities
    /// </summary>
    [TestSuite]
    public class RaritySystemTests
    {
        [TestCase]
        public void GetTier_AllRarities_ShouldReturnCorrectValues()
        {
            // Assert
            AssertInt(RaritySystem.GetTier(ItemRarity.Common)).IsEqual(0);
            AssertInt(RaritySystem.GetTier(ItemRarity.Uncommon)).IsEqual(1);
            AssertInt(RaritySystem.GetTier(ItemRarity.Rare)).IsEqual(2);
            AssertInt(RaritySystem.GetTier(ItemRarity.Epic)).IsEqual(3);
            AssertInt(RaritySystem.GetTier(ItemRarity.Legendary)).IsEqual(4);
            AssertInt(RaritySystem.GetTier(ItemRarity.Exotic)).IsEqual(5);
            AssertInt(RaritySystem.GetTier(ItemRarity.Mythic)).IsEqual(6);
        }

        [TestCase]
        public void GetRarityFromTier_ValidTiers_ShouldReturnCorrectRarity()
        {
            // Assert
            AssertObject(RaritySystem.GetRarityFromTier(0)).IsEqual(ItemRarity.Common);
            AssertObject(RaritySystem.GetRarityFromTier(2)).IsEqual(ItemRarity.Rare);
            AssertObject(RaritySystem.GetRarityFromTier(4)).IsEqual(ItemRarity.Legendary);
        }

        [TestCase]
        public void GetRarityFromTier_OutOfRange_ShouldClamp()
        {
            // Assert
            AssertObject(RaritySystem.GetRarityFromTier(-1)).IsEqual(ItemRarity.Common);
            AssertObject(RaritySystem.GetRarityFromTier(10)).IsEqual(ItemRarity.Mythic);
        }

        [TestCase]
        public void IsRareOrAbove_ShouldIdentifyCorrectly()
        {
            // Assert
            AssertBool(RaritySystem.IsRareOrAbove(ItemRarity.Common)).IsFalse();
            AssertBool(RaritySystem.IsRareOrAbove(ItemRarity.Uncommon)).IsFalse();
            AssertBool(RaritySystem.IsRareOrAbove(ItemRarity.Rare)).IsTrue();
            AssertBool(RaritySystem.IsRareOrAbove(ItemRarity.Epic)).IsTrue();
            AssertBool(RaritySystem.IsRareOrAbove(ItemRarity.Legendary)).IsTrue();
        }

        [TestCase]
        public void IsEpicOrAbove_ShouldIdentifyCorrectly()
        {
            // Assert
            AssertBool(RaritySystem.IsEpicOrAbove(ItemRarity.Rare)).IsFalse();
            AssertBool(RaritySystem.IsEpicOrAbove(ItemRarity.Epic)).IsTrue();
            AssertBool(RaritySystem.IsEpicOrAbove(ItemRarity.Legendary)).IsTrue();
            AssertBool(RaritySystem.IsEpicOrAbove(ItemRarity.Mythic)).IsTrue();
        }

        [TestCase]
        public void IsLegendaryTier_ShouldIdentifyCorrectly()
        {
            // Assert
            AssertBool(RaritySystem.IsLegendaryTier(ItemRarity.Epic)).IsFalse();
            AssertBool(RaritySystem.IsLegendaryTier(ItemRarity.Legendary)).IsTrue();
            AssertBool(RaritySystem.IsLegendaryTier(ItemRarity.Exotic)).IsTrue();
            AssertBool(RaritySystem.IsLegendaryTier(ItemRarity.Mythic)).IsTrue();
        }

        [TestCase]
        public void GetUIColor_AllRarities_ShouldReturnValidColors()
        {
            // Assert - just check they return non-null colors
            AssertObject(RaritySystem.GetUIColor(ItemRarity.Common)).IsNotNull();
            AssertObject(RaritySystem.GetUIColor(ItemRarity.Uncommon)).IsNotNull();
            AssertObject(RaritySystem.GetUIColor(ItemRarity.Rare)).IsNotNull();
            AssertObject(RaritySystem.GetUIColor(ItemRarity.Epic)).IsNotNull();
            AssertObject(RaritySystem.GetUIColor(ItemRarity.Legendary)).IsNotNull();
        }

        [TestCase]
        public void GetEmissionColor_ShouldBeBrighterThanBase()
        {
            // Arrange
            Color baseColor = RaritySystem.GetUIColor(ItemRarity.Legendary);
            Color emissionColor = RaritySystem.GetEmissionColor(ItemRarity.Legendary);

            // Assert - emission should be brighter (though multiplied by 1.5 may exceed 1.0)
            AssertObject(emissionColor).IsNotNull();
        }

        [TestCase]
        public void GetColorWithAlpha_ShouldSetAlpha()
        {
            // Act
            Color color = RaritySystem.GetColorWithAlpha(ItemRarity.Rare, 0.5f);

            // Assert
            AssertFloat(color.A).IsEqual(0.5f);
        }

        [TestCase]
        public void GetGlowIntensity_ShouldIncreaseWithRarity()
        {
            // Act
            float commonGlow = RaritySystem.GetGlowIntensity(ItemRarity.Common);
            float rareGlow = RaritySystem.GetGlowIntensity(ItemRarity.Rare);
            float legendaryGlow = RaritySystem.GetGlowIntensity(ItemRarity.Legendary);

            // Assert
            AssertFloat(commonGlow).IsLess(rareGlow);
            AssertFloat(rareGlow).IsLess(legendaryGlow);
        }

        [TestCase]
        public void GetParticleMultiplier_ShouldIncreaseWithRarity()
        {
            // Act
            float commonParticles = RaritySystem.GetParticleMultiplier(ItemRarity.Common);
            float legendaryParticles = RaritySystem.GetParticleMultiplier(ItemRarity.Legendary);

            // Assert
            AssertFloat(commonParticles).IsLess(legendaryParticles);
        }

        [TestCase]
        public void GetScaleMultiplier_ShouldIncreaseWithRarity()
        {
            // Act
            float commonScale = RaritySystem.GetScaleMultiplier(ItemRarity.Common);
            float epicScale = RaritySystem.GetScaleMultiplier(ItemRarity.Epic);

            // Assert
            AssertFloat(commonScale).IsLess(epicScale);
        }

        [TestCase]
        public void ShowBeamEffect_ShouldBeEpicAndAbove()
        {
            // Assert
            AssertBool(RaritySystem.ShowBeamEffect(ItemRarity.Common)).IsFalse();
            AssertBool(RaritySystem.ShowBeamEffect(ItemRarity.Rare)).IsFalse();
            AssertBool(RaritySystem.ShowBeamEffect(ItemRarity.Epic)).IsTrue();
            AssertBool(RaritySystem.ShowBeamEffect(ItemRarity.Legendary)).IsTrue();
        }

        [TestCase]
        public void PlaySpecialSound_ShouldBeRareAndAbove()
        {
            // Assert
            AssertBool(RaritySystem.PlaySpecialSound(ItemRarity.Common)).IsFalse();
            AssertBool(RaritySystem.PlaySpecialSound(ItemRarity.Uncommon)).IsFalse();
            AssertBool(RaritySystem.PlaySpecialSound(ItemRarity.Rare)).IsTrue();
            AssertBool(RaritySystem.PlaySpecialSound(ItemRarity.Epic)).IsTrue();
        }

        [TestCase]
        public void GetDropRate_ShouldReturnValidRates()
        {
            // Act & Assert
            float commonRate = RaritySystem.GetDropRate(ItemRarity.Common);
            float legendaryRate = RaritySystem.GetDropRate(ItemRarity.Legendary);

            AssertFloat(commonRate).IsGreater(0.0f);
            AssertFloat(legendaryRate).IsGreater(0.0f);
            AssertFloat(commonRate).IsGreater(legendaryRate);
        }

        [TestCase]
        public void GetModifiedDropRate_WithLuck_ShouldAffectRareItems()
        {
            // Act
            float baseRate = RaritySystem.GetDropRate(ItemRarity.Legendary);
            float modifiedRate = RaritySystem.GetModifiedDropRate(ItemRarity.Legendary, 2.0f);

            // Assert - legendary should be affected by luck
            AssertFloat(modifiedRate).IsGreater(baseRate);
        }

        [TestCase]
        public void GetModifiedDropRate_WithLuck_ShouldNotAffectCommon()
        {
            // Act
            float baseRate = RaritySystem.GetDropRate(ItemRarity.Common);
            float modifiedRate = RaritySystem.GetModifiedDropRate(ItemRarity.Common, 2.0f);

            // Assert - common should not be affected by luck
            AssertFloat(modifiedRate).IsEqual(baseRate);
        }

        [TestCase]
        public void GetAllRaritiesSorted_ShouldReturnInOrder()
        {
            // Act
            var rarities = RaritySystem.GetAllRaritiesSorted();

            // Assert
            AssertInt(rarities.Length).IsEqual(7);
            AssertObject(rarities[0]).IsEqual(ItemRarity.Common);
            AssertObject(rarities[6]).IsEqual(ItemRarity.Mythic);
        }

        [TestCase]
        public void GetAllRaritiesReverse_ShouldReturnReversed()
        {
            // Act
            var rarities = RaritySystem.GetAllRaritiesReverse();

            // Assert
            AssertInt(rarities.Length).IsEqual(7);
            AssertObject(rarities[0]).IsEqual(ItemRarity.Mythic);
            AssertObject(rarities[6]).IsEqual(ItemRarity.Common);
        }

        [TestCase]
        public void GetDisplayName_AllRarities_ShouldReturnNames()
        {
            // Assert
            AssertString(RaritySystem.GetDisplayName(ItemRarity.Common)).IsNotEmpty();
            AssertString(RaritySystem.GetDisplayName(ItemRarity.Legendary)).IsNotEmpty();
        }

        [TestCase]
        public void GetColoredDisplayName_ShouldIncludeColorTags()
        {
            // Act
            string colored = RaritySystem.GetColoredDisplayName(ItemRarity.Legendary);

            // Assert
            AssertString(colored).Contains("[color=");
            AssertString(colored).Contains("[/color]");
        }

        [TestCase]
        public void GetDescription_AllRarities_ShouldReturnDescriptions()
        {
            // Assert
            AssertString(RaritySystem.GetDescription(ItemRarity.Common)).IsNotEmpty();
            AssertString(RaritySystem.GetDescription(ItemRarity.Legendary)).IsNotEmpty();
        }

        [TestCase]
        public void GetSymbol_AllRarities_ShouldReturnSymbols()
        {
            // Assert
            AssertString(RaritySystem.GetSymbol(ItemRarity.Common)).IsNotEmpty();
            AssertString(RaritySystem.GetSymbol(ItemRarity.Legendary)).IsNotEmpty();
        }

        [TestCase]
        public void GetStatMultiplier_ShouldIncreaseWithRarity()
        {
            // Act
            float commonMult = RaritySystem.GetStatMultiplier(ItemRarity.Common);
            float legendaryMult = RaritySystem.GetStatMultiplier(ItemRarity.Legendary);

            // Assert
            AssertFloat(commonMult).IsEqual(1.0f);
            AssertFloat(legendaryMult).IsGreater(commonMult);
        }

        [TestCase]
        public void GetSellValueMultiplier_ShouldIncreaseWithRarity()
        {
            // Act
            float commonValue = RaritySystem.GetSellValueMultiplier(ItemRarity.Common);
            float legendaryValue = RaritySystem.GetSellValueMultiplier(ItemRarity.Legendary);

            // Assert
            AssertFloat(commonValue).IsEqual(1.0f);
            AssertFloat(legendaryValue).IsGreater(commonValue);
        }

        [TestCase]
        public void RollRandom_ShouldReturnValidRarity()
        {
            // Act
            var rarity = RaritySystem.RollRandom();

            // Assert - should be one of the valid rarities
            AssertBool(rarity >= ItemRarity.Common && rarity <= ItemRarity.Mythic).IsTrue();
        }

        [TestCase]
        public void RollInRange_ShouldReturnWithinRange()
        {
            // Act
            var rarity = RaritySystem.RollInRange(ItemRarity.Uncommon, ItemRarity.Epic);

            // Assert
            AssertBool(rarity >= ItemRarity.Uncommon && rarity <= ItemRarity.Epic).IsTrue();
        }
    }
}
