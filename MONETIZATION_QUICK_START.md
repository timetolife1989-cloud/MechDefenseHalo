# Monetization System - Quick Start Guide

## Installation & Setup

### Step 1: Add Managers to Autoload (Required)

The monetization system requires two managers to be added to Godot's autoload system:

1. Open **Project → Project Settings → Autoload**
2. Add the following autoload singletons:

```
Name: MonetizationManager
Path: res://Scripts/Monetization/MonetizationManager.cs
Enable: ✓

Name: AnalyticsManager  
Path: res://Scripts/Analytics/AnalyticsManager.cs
Enable: ✓
```

### Step 2: Initial Setup on Game Launch

The system will automatically:
- Show consent dialog if needed (first launch)
- Check for daily login rewards
- Initialize all subsystems

### Step 3: Testing the System

#### Option A: Via Debug Console

If you have a debug console, add these commands:

```csharp
// Check monetization status
MonetizationManager.PrintDebugInfo();

// View metrics
AnalyticsManager.PrintAllSummaries();

// Reset for testing
AdFrequencyCap.ResetAllCooldowns();
AdFrequencyCap.ResetDailyCounter();

// Manual triggers
MonetizationManager.CheckDailyLogin();
MonetizationManager.ShowConsentDialog();
```

#### Option B: Run Unit Tests

```bash
# In Godot Editor:
# Bottom panel → GdUnit4 → Run All Tests

# Look for:
# - AdFrequencyCapTests: Tests daily limits
# - AdConsentManagerTests: Tests GDPR/COPPA
# - AdMetricsTrackerTests: Tests analytics
```

## How the System Works

### 1. Victory Bonus (Post-Boss)

**When**: Automatically after defeating a boss  
**Trigger**: `EventBus.BossDefeated` event  
**Offers**: 
- Base: 500 Credits, 50 Cores, Epic item
- Bonus (with ad): 1000 Credits, 100 Cores, Legendary item

**Integration Point**: Already connected to `BossBase.OnDeath()`

### 2. Milestone Rewards (Every 5 Waves)

**When**: After wave 5, 10, 15, 20...  
**Trigger**: `EventBus.WaveCompleted` event  
**Offers**:
- Base: 3 items, 200 Credits
- Bonus (with ad): 5 items, 500 Credits, better rarity

**Integration Point**: Already connected to `WaveSpawner.CompleteWave()`

### 3. Daily Login Rewards

**When**: First time opening game each day  
**Trigger**: Called from `MonetizationManager._Ready()`  
**Offers**:
- Base: 100-1000 Credits (progressive)
- Bonus (with ad): 3x Credits
- Day 7: +50 Cores (always)

**Integration Point**: Automatic on game start

### 4. Ad Frequency Protection

**Limits**:
- Maximum 5 ads per day (hard cap)
- 5 minutes cooldown between ads
- Daily counter resets at midnight

**Validation**: All ad offers automatically check these limits

### 5. GDPR/COPPA Compliance

**First Launch Flow**:
1. Age verification (if needed)
2. Consent dialog
3. Privacy policy acceptance

**Protection**:
- Users under 16: No personalized ads
- EU users: Explicit consent required
- Consent can be revoked anytime

## Creating UI Scenes

The system provides UI controller scripts. You need to create the actual Godot scenes:

### Victory Bonus UI (VictoryBonusOffer.tscn)

```
Control (CanvasLayer)
└── Panel (Panel)
    └── VBoxContainer
        ├── TitleLabel (Label)
        ├── BaseRewardLabel (Label)
        ├── BonusRewardLabel (Label)
        └── HBoxContainer
            ├── WatchAdButton (Button)
            └── NoThanksButton (Button)
```

**Attach Script**: `VictoryBonusOfferUI.cs`

### Milestone Reward UI (MilestoneRewardOffer.tscn)

```
Control (CanvasLayer)
└── Panel (Panel)
    └── VBoxContainer
        ├── TitleLabel (Label)
        ├── BaseRewardLabel (Label)
        ├── BonusRewardLabel (Label)
        └── HBoxContainer
            ├── WatchAdButton (Button)
            └── NoThanksButton (Button)
```

**Attach Script**: `MilestoneRewardOfferUI.cs`

### Daily Login UI (DailyLoginPanel.tscn)

```
Control (CanvasLayer)
└── Panel (Panel)
    └── VBoxContainer
        ├── TitleLabel (Label)
        ├── DayLabel (Label)
        ├── BaseRewardLabel (Label)
        ├── BonusRewardLabel (Label)
        ├── SpecialBonusLabel (Label) [hidden by default]
        └── HBoxContainer
            ├── WatchAdButton (Button)
            └── ClaimBaseButton (Button)
```

**Attach Script**: `DailyLoginPanelUI.cs`

### Consent Dialog UI (AdConsentDialog.tscn)

```
Control (CanvasLayer)
└── Panel (Panel)
    └── VBoxContainer
        ├── TitleLabel (Label)
        ├── MessageLabel (Label)
        ├── AgeContainer (HBoxContainer)
        │   ├── AgeLabel (Label) "Your age:"
        │   └── AgeSpinBox (SpinBox)
        ├── PrivacyPolicyCheckbox (CheckBox)
        └── HBoxContainer
            ├── AcceptButton (Button)
            └── DeclineButton (Button)
```

**Attach Script**: `AdConsentDialogUI.cs`

### Adding UI to Game

Add these UI scenes to your main game scene:

```
MainGame
├── [other game nodes]
└── MonetizationUI
    ├── VictoryBonusOffer [instance]
    ├── MilestoneRewardOffer [instance]
    ├── DailyLoginPanel [instance]
    └── AdConsentDialog [instance]
```

All UIs start hidden and show automatically when triggered by events.

## Integrating Real Ad Networks

### Current State: Simulation

The system currently **simulates** 30-second ad playback:

```csharp
// In AdPlacementManager.cs
private async void SimulateAdPlayback(string adType, Action onComplete)
{
    await ToSignal(GetTree().CreateTimer(1.0), "timeout");
    // Ad "completed"
    AdFrequencyCap.RecordAdShown(adType);
    onComplete?.Invoke();
}
```

### Integration with AdMob (Recommended)

Replace the simulation with real AdMob calls:

```csharp
// 1. Install AdMob plugin for Godot
// 2. Update AdPlacementManager.WatchAd():

public static void WatchAd(string adType, Action onComplete)
{
    if (Instance == null) return;
    
    Instance.IsAdCurrentlyShowing = true;
    
    // Load and show rewarded ad
    var adMob = GetNode<AdMob>("/root/AdMob");
    adMob.LoadRewardedAd("ca-app-pub-xxxxx/yyyyy");
    adMob.ShowRewardedAd();
    
    // Connect to ad closed signal
    adMob.Connect("rewarded_ad_closed", Instance, 
        nameof(OnAdMobAdClosed), new Godot.Collections.Array { adType, onComplete });
}

private void OnAdMobAdClosed(bool watched, string adType, Action onComplete)
{
    IsAdCurrentlyShowing = false;
    
    if (watched)
    {
        AdFrequencyCap.RecordAdShown(adType);
        onComplete?.Invoke();
    }
}
```

### Integration with Unity Ads

```csharp
// 1. Install Unity Ads plugin
// 2. Update AdPlacementManager.WatchAd():

public static void WatchAd(string adType, Action onComplete)
{
    if (Instance == null) return;
    
    Instance.IsAdCurrentlyShowing = true;
    
    var unityAds = GetNode<UnityAds>("/root/UnityAds");
    unityAds.ShowRewardedVideo(adType, (result) =>
    {
        Instance.IsAdCurrentlyShowing = false;
        
        if (result == UnityAds.ShowResult.Finished)
        {
            AdFrequencyCap.RecordAdShown(adType);
            onComplete?.Invoke();
        }
    });
}
```

## Monitoring & Analytics

### In-Game Metrics

```csharp
// Print metrics summary
AdMetricsTracker.PrintMetricsSummary();

// Output:
// === AD METRICS SUMMARY ===
// Total Ads Offered: 15
// Total Ads Watched: 8
// Total Ads Skipped: 7
// Overall Conversion Rate: 53.3%
//
// --- VICTORY ---
//   Offered: 5
//   Watched: 3
//   Skipped: 2
//   Conversion Rate: 60.0%
```

### Integration with Firebase

To send metrics to Firebase Analytics:

```csharp
// In MonetizationEvents.cs, update LogEvent():

public static void LogEvent(string eventName, params (string key, object value)[] parameters)
{
    // Console logging (keep for debugging)
    GD.Print($"[ANALYTICS] {eventName}...");
    
    // Send to Firebase
    var firebase = GetNode<Firebase>("/root/Firebase");
    var eventParams = new Dictionary<string, object>();
    
    foreach (var param in parameters)
    {
        eventParams[param.key] = param.value;
    }
    
    firebase.LogEvent(eventName, eventParams);
}
```

## Troubleshooting

### Issue: Consent dialog not showing

**Solution**: Make sure `MonetizationManager` is in autoload and game has launched fresh (not continuing from editor)

### Issue: Ads not appearing after boss/wave

**Solution**: Check that:
1. `EventBus.BossDefeated` or `EventBus.WaveCompleted` is being emitted
2. Daily ad limit (5) not reached
3. Consent was granted

### Issue: Daily counter not resetting

**Solution**: The system checks `DateTime.Today`. Make sure system clock is working correctly.

### Issue: Tests failing

**Solution**: 
1. Install GdUnit4 from Asset Library
2. Enable plugin in Project Settings
3. Run tests via Bottom Panel → GdUnit4

## Next Steps

1. ✅ System is implemented and tested
2. ⏳ Create UI scenes in Godot Editor
3. ⏳ Add managers to autoload
4. ⏳ Test in-game
5. ⏳ Integrate real ad network
6. ⏳ Deploy and monitor metrics

## Support

- Full documentation: `Data/Monetization/README.md`
- Configuration: `Data/Monetization/ad_config.json`
- Tests: `Tests/Monetization/`

## Success Criteria Checklist

- [x] All ad offers are opt-in with visible "No Thanks"
- [x] Hard cap at 5 ads/day enforced
- [x] 5-minute cooldown between ads
- [x] Base rewards given FIRST, ads only for bonus
- [x] GDPR/COPPA consent flow implemented
- [x] Analytics tracking all monetization events
- [x] No mid-combat or death-related ad offers
- [ ] UI scenes created and styled
- [ ] Real ad network integrated
- [ ] Player satisfaction >70% (post-launch survey)

**Status**: Core system complete, ready for UI design and ad network integration!
