using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Items;
using MechDefenseHalo.Economy;

namespace MechDefenseHalo.Monetization
{
    /// <summary>
    /// Manages victory bonus ad offers after boss defeats.
    /// Offers to upgrade loot quality in exchange for watching an ad.
    /// </summary>
    public partial class VictoryBonusAd : Node
    {
        #region Constants

        private const string AD_TYPE = "victory";
        private const float CREDIT_MULTIPLIER = 2.0f;
        private const float CORE_MULTIPLIER = 2.0f;

        #endregion

        #region Private Fields

        private bool _playerDied = false;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Subscribe to boss defeated event
            EventBus.On(EventBus.BossDefeated, OnBossDefeated);
            EventBus.On(EventBus.PlayerDied, OnPlayerDied);

            GD.Print("VictoryBonusAd system initialized");
        }

        public override void _ExitTree()
        {
            EventBus.Off(EventBus.BossDefeated, OnBossDefeated);
            EventBus.Off(EventBus.PlayerDied, OnPlayerDied);
        }

        #endregion

        #region Event Handlers

        private void OnBossDefeated(object data)
        {
            if (data is Enemies.Bosses.BossDefeatedData bossData)
            {
                // Only show if player didn't die (no continues used)
                if (_playerDied)
                {
                    GD.Print("Boss defeated but player died - no victory bonus offer");
                    _playerDied = false; // Reset for next boss
                    return;
                }

                // Show victory bonus offer
                ShowBossVictoryOffer(bossData.BossName);
            }
        }

        private void OnPlayerDied(object data)
        {
            _playerDied = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show boss victory bonus offer
        /// </summary>
        /// <param name="bossName">Name of defeated boss</param>
        public void ShowBossVictoryOffer(string bossName)
        {
            // Create base rewards (what player gets regardless)
            var baseRewards = GenerateBaseRewards(bossName);

            // Create bonus rewards (what player gets if they watch ad)
            var bonusRewards = GenerateBonusRewards(baseRewards);

            // Request ad offer
            bool canShowOffer = AdPlacementManager.RequestAdOffer(
                AD_TYPE,
                () => OnAdWatched(baseRewards, bonusRewards),
                () => OnAdSkipped(baseRewards)
            );

            if (canShowOffer)
            {
                // Emit event to show UI
                EventBus.Emit("show_victory_bonus_offer", new VictoryBonusOfferData
                {
                    BossName = bossName,
                    BaseCredits = baseRewards.Credits,
                    BaseCores = baseRewards.Cores,
                    BaseRarity = baseRewards.LootRarity,
                    BonusCredits = bonusRewards.Credits,
                    BonusCores = bonusRewards.Cores,
                    BonusRarity = bonusRewards.LootRarity
                });

                GD.Print($"Victory bonus offer shown for defeating {bossName}");
            }
            else
            {
                // Give base rewards immediately
                GiveRewards(baseRewards);
            }
        }

        #endregion

        #region Private Methods

        private BossRewards GenerateBaseRewards(string bossName)
        {
            // Base rewards for defeating a boss
            return new BossRewards
            {
                Credits = 500,
                Cores = 50,
                LootRarity = ItemRarity.Epic
            };
        }

        private BossRewards GenerateBonusRewards(BossRewards baseRewards)
        {
            // Upgraded rewards for watching ad
            return new BossRewards
            {
                Credits = (int)(baseRewards.Credits * CREDIT_MULTIPLIER),
                Cores = (int)(baseRewards.Cores * CORE_MULTIPLIER),
                LootRarity = UpgradeLootQuality(baseRewards.LootRarity)
            };
        }

        private ItemRarity UpgradeLootQuality(ItemRarity currentRarity)
        {
            // Upgrade rarity by one tier
            return currentRarity switch
            {
                ItemRarity.Common => ItemRarity.Uncommon,
                ItemRarity.Uncommon => ItemRarity.Rare,
                ItemRarity.Rare => ItemRarity.Epic,
                ItemRarity.Epic => ItemRarity.Legendary,
                ItemRarity.Legendary => ItemRarity.Mythic,
                ItemRarity.Exotic => ItemRarity.Mythic,
                ItemRarity.Mythic => ItemRarity.Mythic, // Already max
                _ => ItemRarity.Uncommon
            };
        }

        private void OnAdWatched(BossRewards baseRewards, BossRewards bonusRewards)
        {
            GD.Print("Victory bonus ad watched - giving bonus rewards!");
            GiveRewards(bonusRewards);

            // Emit analytics event
            EventBus.Emit("victory_bonus_claimed", new VictoryBonusClaimedData
            {
                AdType = AD_TYPE,
                Credits = bonusRewards.Credits,
                Cores = bonusRewards.Cores,
                Rarity = bonusRewards.LootRarity
            });
        }

        private void OnAdSkipped(BossRewards baseRewards)
        {
            GD.Print("Victory bonus ad skipped - giving base rewards");
            GiveRewards(baseRewards);
            AdPlacementManager.RecordAdSkipped(AD_TYPE);
        }

        private void GiveRewards(BossRewards rewards)
        {
            // Give currency rewards
            CurrencyManager.AddCredits(rewards.Credits, "Boss Victory");
            CurrencyManager.AddCores(rewards.Cores, "Boss Victory");

            // TODO: Give loot item of specified rarity when loot system integration is complete
            // This requires LootTableManager to support programmatic item generation
            // For now, log what would be given
            GD.Print($"Rewards given: {rewards.Credits} Credits, {rewards.Cores} Cores, {rewards.LootRarity} loot");
        }

        #endregion

        #region Helper Classes

        private class BossRewards
        {
            public int Credits { get; set; }
            public int Cores { get; set; }
            public ItemRarity LootRarity { get; set; }
        }

        #endregion
    }

    #region Event Data Structures

    /// <summary>
    /// Data for victory bonus offer display
    /// </summary>
    public class VictoryBonusOfferData
    {
        public string BossName { get; set; }
        public int BaseCredits { get; set; }
        public int BaseCores { get; set; }
        public ItemRarity BaseRarity { get; set; }
        public int BonusCredits { get; set; }
        public int BonusCores { get; set; }
        public ItemRarity BonusRarity { get; set; }
    }

    /// <summary>
    /// Data for victory bonus claimed event
    /// </summary>
    public class VictoryBonusClaimedData
    {
        public string AdType { get; set; }
        public int Credits { get; set; }
        public int Cores { get; set; }
        public ItemRarity Rarity { get; set; }
    }

    #endregion
}
