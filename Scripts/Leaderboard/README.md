# Leaderboard System

Global leaderboard system with score tracking, ranking, and Firebase integration for MechDefenseHalo.

## Overview

The leaderboard system provides a comprehensive score tracking and ranking solution that allows players to compete globally. It tracks scores, ranks players, and provides time-based filtering for competitive play.

## Features

- ✅ **Score Submission**: Automatically submit player scores to the leaderboard
- ✅ **Top 100 Leaderboard**: Track and display the top 100 scores
- ✅ **Player Ranking**: Calculate and display individual player ranks
- ✅ **Firebase Sync**: Cloud sync capabilities (stub implementation for future integration)
- ✅ **Leaderboard UI**: Display top scores with filtering options
- ✅ **Time Period Filters**: Daily, Weekly, Monthly, and All-Time leaderboards
- ✅ **Friend Leaderboard**: Filter leaderboard to show only friends (requires friend system)
- ✅ **Rank Tiers**: Bronze, Silver, Gold, Platinum, Diamond, Master, Legend
- ✅ **Combo System**: Score multipliers for consecutive kills
- ✅ **Local Persistence**: Save and load leaderboard data locally

## Architecture

### Components

#### LeaderboardManager.cs
Central manager for the leaderboard system. Handles score submission, ranking, and persistence.

**Key Methods:**
- `SubmitScore(playerName, score, wave, kills)` - Submit a score to the leaderboard
- `GetTopScores(count)` - Get top N scores
- `GetScoresByTimePeriod(period, count)` - Get scores filtered by time period
- `GetPlayerRank(playerName)` - Get player's rank
- `GetPlayerBestScore(playerName)` - Get player's best score
- `GetFriendLeaderboard(friendNames, count)` - Get friend scores

#### ScoreTracker.cs
Tracks player score during gameplay based on kills, waves, and combos.

**Key Properties:**
- `TotalScore` - Current total score
- `CurrentWave` - Current wave number
- `TotalKills` - Total kills in current session
- `CurrentCombo` - Active kill combo counter
- `ComboTimer` - Time remaining before combo breaks

**Scoring:**
- Base kill points: 100 per kill
- Wave completion: 500 + (wave * 50)
- Boss defeat: 2000 points
- Combo multiplier: +10% per combo level (max 1.5x)
- Combo decay: 3 seconds

#### RankCalculator.cs
Calculates player rankings and provides rank-related utilities.

**Key Methods:**
- `GetPlayerRank(entries, playerName)` - Calculate player rank
- `GetRankForScore(entries, score)` - Get rank for a specific score
- `GetPlayerPercentile(entries, playerName)` - Get percentile rank (0-100)
- `GetRankTier(score)` - Get rank tier based on score
- `GetPointsToNextTier(currentScore)` - Calculate points needed for next tier

**Rank Tiers:**
- Unranked: < 1,000 points
- Bronze: 1,000 - 2,499 points
- Silver: 2,500 - 4,999 points
- Gold: 5,000 - 9,999 points
- Platinum: 10,000 - 24,999 points
- Diamond: 25,000 - 49,999 points
- Master: 50,000 - 99,999 points
- Legend: 100,000+ points

#### LeaderboardUI.cs
UI component for displaying the leaderboard with filtering options.

**Features:**
- Display top N entries
- Show player rank
- Time period filtering (Daily, Weekly, Monthly, All-Time)
- Friend leaderboard view
- Automatic updates when leaderboard changes

#### FirebaseLeaderboard.cs
Handles Firebase integration for cloud leaderboard sync (stub implementation).

**Methods:**
- `UploadScore(entry)` - Upload score to Firebase
- `DownloadLeaderboard(callback)` - Download leaderboard from Firebase
- `DownloadLeaderboardByPeriod(period, callback)` - Download with time filter
- `DownloadFriendLeaderboard(friendNames, callback)` - Download friend scores

## Usage

### Automatic Score Tracking

The ScoreTracker automatically tracks score during gameplay:

```csharp
// Score is automatically tracked through EventBus events:
// - EntityDied: Adds kill points with combo multiplier
// - WaveCompleted: Adds wave completion bonus
// - BossDefeated: Adds boss defeat bonus
// - GameOver: Automatically submits score to leaderboard
```

### Manual Score Submission

```csharp
// Submit a score manually
LeaderboardManager.Instance.SubmitScore("PlayerName", 15000, 25, 342);

// Submit current game score
LeaderboardManager.Instance.SubmitCurrentGameScore();
```

### Querying Leaderboard

```csharp
// Get top 10 scores
var topScores = LeaderboardManager.Instance.GetTopScores(10);

// Get weekly leaderboard
var weeklyScores = LeaderboardManager.Instance.GetScoresByTimePeriod(TimePeriod.Weekly, 10);

// Get player rank
int rank = LeaderboardManager.Instance.GetPlayerRank("PlayerName");

// Get player's best score
var bestScore = LeaderboardManager.Instance.GetPlayerBestScore("PlayerName");

// Get friend leaderboard
var friendNames = new List<string> { "Friend1", "Friend2", "Friend3" };
var friendScores = LeaderboardManager.Instance.GetFriendLeaderboard(friendNames, 10);
```

### Using RankCalculator

```csharp
var calculator = new RankCalculator();

// Get rank tier for score
var tier = calculator.GetRankTier(25000); // Returns RankTier.Diamond

// Get points needed for next tier
int pointsNeeded = calculator.GetPointsToNextTier(24500); // Returns 500 (to reach Diamond)

// Get player percentile
float percentile = calculator.GetPlayerPercentile(entries, "PlayerName");
```

### UI Integration

```csharp
// Reference LeaderboardUI in your scene
[Export] public NodePath LeaderboardUIPath;
private LeaderboardUI _leaderboardUI;

public override void _Ready()
{
    _leaderboardUI = GetNode<LeaderboardUI>(LeaderboardUIPath);
    
    // Refresh leaderboard display
    _leaderboardUI.RefreshLeaderboard();
    
    // Show friend leaderboard
    var friendNames = new List<string> { "Friend1", "Friend2" };
    _leaderboardUI.ShowFriendLeaderboard(friendNames);
}
```

## Events

The leaderboard system emits and listens to the following events:

### Emitted Events
- `leaderboard_updated` - When leaderboard is updated with new scores
- `score_updated` - When player score changes (includes score, combo, and points gained)
- `combo_broken` - When kill combo is broken

### Listened Events
- `entity_died` - Tracks kills and updates score
- `wave_completed` - Awards wave completion bonus
- `boss_defeated` - Awards boss defeat bonus
- `game_over` - Submits final score to leaderboard

## Firebase Integration

The Firebase integration is currently a stub implementation. To fully integrate:

1. Add Firebase Godot plugin or GDNative module
2. Configure Firebase project settings
3. Initialize Firebase.App in FirebaseLeaderboard.cs
4. Implement upload/download methods in FirebaseLeaderboard.cs
5. Set up Firebase Realtime Database or Firestore with leaderboard structure

Example Firebase structure:
```json
{
  "leaderboard": {
    "entry_id_1": {
      "playerName": "Player1",
      "score": 15000,
      "wave": 25,
      "kills": 342,
      "timestamp": "2025-12-30T06:00:00Z"
    }
  }
}
```

## Persistence

Leaderboard data is automatically saved to local storage at:
- Path: `user://leaderboard_data.json`
- Format: JSON
- Auto-save: On score submission and manager exit

## Configuration

Configure the leaderboard system through exported properties:

### LeaderboardManager
- `MaxLeaderboardEntries` (default: 100) - Maximum entries to track
- `SaveFilePath` (default: "user://leaderboard_data.json") - Save file location

### ScoreTracker
- `PointsPerKill` (default: 100) - Points awarded per kill
- `PointsPerWave` (default: 500) - Base points for wave completion
- `PointsPerBoss` (default: 2000) - Points for boss defeat
- `ComboMultiplier` (default: 1.5) - Maximum combo multiplier
- `ComboDecayTime` (default: 3.0) - Seconds before combo breaks

### LeaderboardUI
- `MaxDisplayedEntries` (default: 10) - Number of entries to display
- `EntryPrefab` - Custom prefab for leaderboard entry (optional)

## Testing

To test the leaderboard system:

1. Start a game session
2. Kill enemies to build score and combo
3. Complete waves for bonus points
4. Defeat bosses for large bonuses
5. Check leaderboard after game over
6. Verify rank calculation and display

## Future Enhancements

- [ ] Implement actual Firebase integration
- [ ] Add friend system integration
- [ ] Add achievements for leaderboard ranks
- [ ] Add season-based leaderboards with resets
- [ ] Add regional leaderboards
- [ ] Add clan/team leaderboards
- [ ] Add challenge leaderboards (specific game modes)
- [ ] Add leaderboard rewards system
- [ ] Add anti-cheat validation

## Dependencies

- Godot 4.x
- C# / .NET
- EventBus system (from MechDefenseHalo.Core)
- StatisticsManager (for integration)

## Notes

- Leaderboard entries are sorted by score in descending order
- Player rank is based on their best score, not their latest
- Combo system encourages aggressive, consistent play
- Local persistence ensures data is not lost between sessions
- Firebase integration is stubbed for future cloud sync
