# Ethical Victory Monetization System

## Overview

This monetization system implements player-first rewarded video ads based on 2024-2025 industry best practices and behavioral psychology research.

## Core Principles

1. **Skill > Money (always)** - Ads never provide competitive advantages
2. **Ads = Bonus rewards, not requirements** - Players can enjoy the full game without watching ads
3. **Victory first, monetization second** - Ads appear after achievements, not during gameplay
4. **User control (always opt-in)** - Every ad is optional with a clear "No Thanks" option
5. **Respect player time (max 5 ads/day)** - Hard cap prevents ad fatigue

## System Architecture

### Core Components

#### 1. AdFrequencyCap.cs
- Enforces 5 ads per day limit
- 5-minute cooldown between ads
- Prevents ad spam and fatigue

#### 2. AdConsentManager.cs
- GDPR/COPPA compliance
- Age verification (16+ for personalized ads)
- Regional consent handling (EU/UK/EEA)

#### 3. AdPlacementManager.cs
- Central coordination for all ad offers
- Validates consent and frequency caps
- Manages ad playback simulation

#### 4. VictoryBonusAd.cs
- Post-boss defeat bonuses
- Upgrades loot quality (Common → Uncommon → Rare → Epic → Legendary → Mythic)
- 2x credit and core multipliers

#### 5. MilestoneRewardAd.cs
- Every 5 waves milestone
- Upgrades chest from 3 items → 5 items
- Better item rarity rolls

#### 6. DailyLoginReward.cs
- Daily login bonuses with 3x multiplier
- Progressive rewards (100 → 1000 credits over 7 days)
- Day 7 bonus: +50 Cores (always awarded)

### Analytics Components

#### 1. AdMetricsTracker.cs
- Tracks conversion rates per ad type
- Monitors overall ad performance
- Records offers, watches, and skips

#### 2. RetentionMetrics.cs
- Tracks D1/D7/D30 retention
- Session count and playtime
- Correlates retention with ad frequency

#### 3. MonetizationEvents.cs
- Centralized event definitions
- Analytics logging helpers
- Integration guide for Firebase/Unity Analytics

### UI Controllers

#### 1. VictoryBonusOfferUI.cs
- Displays boss victory offers
- Shows base vs. bonus rewards
- "Watch Ad" vs. "No Thanks" buttons

#### 2. MilestoneRewardOfferUI.cs
- Wave milestone celebration
- Standard vs. Premium chest comparison

#### 3. DailyLoginPanelUI.cs
- Daily login rewards
- Special Day 7 bonus display
- 3x multiplier visualization

#### 4. AdConsentDialogUI.cs
- First-launch consent flow
- Age verification (COPPA)
- Privacy policy agreement

## Configuration

See `Data/Monetization/ad_config.json` for all configurable parameters:

- Ad limits and cooldowns
- Reward multipliers
- Network settings
- Compliance options
- Feature flags

## Integration Guide

### Step 1: Add to Autoload

Add `MonetizationManager` and `AnalyticsManager` to your project's autoload singletons:

```
Project Settings → Autoload
- MonetizationManager: Scripts/Monetization/MonetizationManager.cs
- AnalyticsManager: Scripts/Analytics/AnalyticsManager.cs
```

### Step 2: Add UI to Game Scene

Add the UI controller scripts to your game's main UI:

```
MainUI/
├── VictoryBonusOfferUI (Control node)
├── MilestoneRewardOfferUI (Control node)
├── DailyLoginPanelUI (Control node)
└── AdConsentDialogUI (Control node)
```

### Step 3: Test the System

Use these console commands (via Debug Console):

```csharp
// Print current status
MonetizationManager.PrintDebugInfo();

// View analytics
AnalyticsManager.PrintAllSummaries();

// Reset for testing
AdFrequencyCap.ResetAllCooldowns();
AdFrequencyCap.ResetDailyCounter();
```

### Step 4: Integrate Ad Network SDK

Replace the simulated ad playback in `AdPlacementManager.WatchAd()` with your ad network SDK:

```csharp
// Example for AdMob:
AdMobRewarded.ShowAd(adType, (success) => {
    if (success) {
        AdFrequencyCap.RecordAdShown(adType);
        onComplete?.Invoke();
    }
});

// Example for Unity Ads:
UnityAds.ShowRewardedAd(adType, (result) => {
    if (result == UnityAds.ShowResult.Finished) {
        AdFrequencyCap.RecordAdShown(adType);
        onComplete?.Invoke();
    }
});
```

## Success Metrics

### Key Performance Indicators (KPIs)

1. **Conversion Rate**: Target 30-50% (industry average: 20-30%)
2. **eCPM**: Target $10+ USD (depends on region/network)
3. **Retention**: D1 >40%, D7 >20%, D30 >10%
4. **Player Satisfaction**: >70% (post-launch survey)

### Analytics Events to Monitor

- `ad_offered` - Track when/where ads are offered
- `ad_watched` - Calculate conversion rates
- `ad_skipped` - Understand why players decline
- `retention_milestone` - Correlate ads with retention
- `session_started` - Track daily active users

## Ethical Considerations

### What Makes This System Ethical?

✅ **Transparent**: Players know exactly what they get  
✅ **Optional**: Never required for progression  
✅ **Respectful**: Limited to 5/day, no mid-combat interruptions  
✅ **Fair**: Bonus rewards, not core gameplay  
✅ **Compliant**: GDPR/COPPA age gates and consent  
✅ **Celebratory**: Appears after victories, not failures  

### What to Avoid

❌ Never show ads during combat  
❌ Never show ads after player death  
❌ Never make ads feel mandatory  
❌ Never exceed 5 ads per day  
❌ Never skip the cooldown period  
❌ Never show ads without consent  

## Testing Checklist

- [ ] Consent dialog appears on first launch
- [ ] Age verification blocks users under 16 (COPPA)
- [ ] Daily ad cap (5/day) enforced
- [ ] 5-minute cooldown between ads works
- [ ] Victory bonus appears after boss defeat
- [ ] Milestone offer appears every 5 waves
- [ ] Daily login appears once per day
- [ ] "No Thanks" always gives base reward
- [ ] Ad watching gives bonus reward
- [ ] Analytics tracks all events
- [ ] Day 7 bonus cores awarded regardless of ad

## Support

For questions or issues, refer to:
- `ARCHITECTURE.md` - Overall game architecture
- `PROJECT_STATUS.md` - Implementation status
- `TESTING.md` - Testing guidelines

## License

This monetization system is part of MechDefenseHalo and follows the project's license.

---

**Remember**: Player satisfaction > Short-term revenue. Build trust, and monetization will follow naturally.
