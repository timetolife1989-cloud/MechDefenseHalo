using Godot;
using System;
using MechDefenseHalo.Core;
using MechDefenseHalo.Economy;
using MechDefenseHalo.Items;

namespace MechDefenseHalo.Monetization
{
    /// <summary>
    /// Manages milestone reward ad offers every 5 waves.
    /// Offers to upgrade chest rewards in exchange for watching an ad.
    /// </summary>
    public partial class MilestoneRewardAd : Node
    {
        #region Constants

        private const string AD_TYPE = "milestone";
        private const int MILESTONE_INTERVAL = 5;
        private const int BASE_CHEST_ITEMS = 3;
        private const int BONUS_CHEST_ITEMS = 5;
        private const int BASE_CREDITS = 200;
        private const int BONUS_CREDITS = 500;

        #endregion

        #region Godot Lifecycle

        public override void _Ready()
        {
            // Subscribe to wave completed event
            EventBus.On(EventBus.WaveCompleted, OnWaveCompleted);

            GD.Print("MilestoneRewardAd system initialized");
        }

        public override void _ExitTree()
        {
            EventBus.Off(EventBus.WaveCompleted, OnWaveCompleted);
        }

        #endregion

        #region Event Handlers

        private void OnWaveCompleted(object data)
        {
            if (data is GamePlay.WaveCompletedData waveData)
            {
                // Check if this is a milestone wave (every 5 waves)
                if (waveData.WaveNumber % MILESTONE_INTERVAL == 0)
                {
                    ShowWaveMilestoneBonus(waveData.WaveNumber);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Show wave milestone bonus offer
        /// </summary>
        /// <param name="waveNumber">Wave number that was completed</param>
        public void ShowWaveMilestoneBonus(int waveNumber)
        {
            // Request ad offer
            bool canShowOffer = AdPlacementManager.RequestAdOffer(
                AD_TYPE,
                () => OnAdWatched(waveNumber),
                () => OnAdSkipped(waveNumber)
            );

            if (canShowOffer)
            {
                // Emit event to show UI
                EventBus.Emit("show_milestone_reward_offer", new MilestoneRewardOfferData
                {
                    WaveNumber = waveNumber,
                    BaseItems = BASE_CHEST_ITEMS,
                    BonusItems = BONUS_CHEST_ITEMS,
                    BaseCredits = BASE_CREDITS,
                    BonusCredits = BONUS_CREDITS
                });

                GD.Print($"Milestone reward offer shown for wave {waveNumber}");
            }
            else
            {
                // Give base rewards immediately
                GiveBaseRewards(waveNumber);
            }
        }

        #endregion

        #region Private Methods

        private void OnAdWatched(int waveNumber)
        {
            GD.Print($"Milestone ad watched for wave {waveNumber} - giving premium chest!");
            GiveBonusRewards(waveNumber);

            // Emit analytics event
            EventBus.Emit("milestone_bonus_claimed", new MilestoneBonusClaimedData
            {
                WaveNumber = waveNumber,
                AdType = AD_TYPE,
                ItemCount = BONUS_CHEST_ITEMS,
                Credits = BONUS_CREDITS
            });
        }

        private void OnAdSkipped(int waveNumber)
        {
            GD.Print($"Milestone ad skipped for wave {waveNumber} - giving standard chest");
            GiveBaseRewards(waveNumber);
            AdPlacementManager.RecordAdSkipped(AD_TYPE);
        }

        private void GiveBaseRewards(int waveNumber)
        {
            // Give base chest rewards
            CurrencyManager.AddCredits(BASE_CREDITS, $"Wave {waveNumber} Milestone");

            // Generate base chest items
            GenerateChestItems(BASE_CHEST_ITEMS, ItemRarity.Common);

            GD.Print($"Base milestone rewards given: {BASE_CREDITS} Credits, {BASE_CHEST_ITEMS} items");
        }

        private void GiveBonusRewards(int waveNumber)
        {
            // Give premium chest rewards
            CurrencyManager.AddCredits(BONUS_CREDITS, $"Wave {waveNumber} Premium Milestone");

            // Generate premium chest items (better rarity)
            GenerateChestItems(BONUS_CHEST_ITEMS, ItemRarity.Uncommon);

            GD.Print($"Premium milestone rewards given: {BONUS_CREDITS} Credits, {BONUS_CHEST_ITEMS} items");
        }

        private void GenerateChestItems(int itemCount, ItemRarity minRarity)
        {
            // TODO: Integrate with loot system to generate actual items
            // For now, just log what would be generated
            for (int i = 0; i < itemCount; i++)
            {
                ItemRarity rarity = RollItemRarity(minRarity);
                GD.Print($"  Chest item {i + 1}: {RarityConfig.GetDisplayName(rarity)}");
            }
        }

        private ItemRarity RollItemRarity(ItemRarity minRarity)
        {
            // Roll random rarity with minimum threshold
            ItemRarity rolled = RarityConfig.RollRarity(1.0f);
            
            // Ensure it meets minimum rarity
            if ((int)rolled < (int)minRarity)
            {
                rolled = minRarity;
            }

            return rolled;
        }

        #endregion
    }

    #region Event Data Structures

    /// <summary>
    /// Data for milestone reward offer display
    /// </summary>
    public class MilestoneRewardOfferData
    {
        public int WaveNumber { get; set; }
        public int BaseItems { get; set; }
        public int BonusItems { get; set; }
        public int BaseCredits { get; set; }
        public int BonusCredits { get; set; }
    }

    /// <summary>
    /// Data for milestone bonus claimed event
    /// </summary>
    public class MilestoneBonusClaimedData
    {
        public int WaveNumber { get; set; }
        public string AdType { get; set; }
        public int ItemCount { get; set; }
        public int Credits { get; set; }
    }

    #endregion
}
