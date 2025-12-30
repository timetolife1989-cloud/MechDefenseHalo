using System;
using System.Collections.Generic;
using System.Linq;

namespace MechDefenseHalo.Leaderboard
{
    /// <summary>
    /// Calculates player rankings and provides rank-related utilities
    /// </summary>
    public class RankCalculator
    {
        /// <summary>
        /// Get player's rank in the leaderboard
        /// Returns -1 if player not found
        /// </summary>
        public int GetPlayerRank(List<LeaderboardEntry> entries, string playerName)
        {
            if (entries == null || entries.Count == 0 || string.IsNullOrEmpty(playerName))
                return -1;
            
            // Get player's best score
            var playerBestScore = entries
                .Where(e => e.PlayerName == playerName)
                .OrderByDescending(e => e.Score)
                .FirstOrDefault();
            
            if (playerBestScore == null)
                return -1;
            
            // Count how many unique players have a higher score
            var uniquePlayerScores = entries
                .GroupBy(e => e.PlayerName)
                .Select(g => new { PlayerName = g.Key, BestScore = g.Max(e => e.Score) })
                .OrderByDescending(p => p.BestScore)
                .ToList();
            
            int rank = 1;
            foreach (var player in uniquePlayerScores)
            {
                if (player.PlayerName == playerName)
                    return rank;
                rank++;
            }
            
            return -1;
        }
        
        /// <summary>
        /// Get rank for a specific score
        /// </summary>
        public int GetRankForScore(List<LeaderboardEntry> entries, int score)
        {
            if (entries == null || entries.Count == 0)
                return 1;
            
            // Count how many scores are higher
            int rank = entries.Count(e => e.Score > score) + 1;
            return rank;
        }
        
        /// <summary>
        /// Get percentile rank (0-100) for a player
        /// </summary>
        public float GetPlayerPercentile(List<LeaderboardEntry> entries, string playerName)
        {
            int rank = GetPlayerRank(entries, playerName);
            if (rank <= 0)
                return 0f;
            
            int totalPlayers = GetTotalUniquePlayers(entries);
            if (totalPlayers == 0)
                return 0f;
            
            return ((float)(totalPlayers - rank + 1) / totalPlayers) * 100f;
        }
        
        /// <summary>
        /// Get total number of unique players
        /// </summary>
        public int GetTotalUniquePlayers(List<LeaderboardEntry> entries)
        {
            if (entries == null)
                return 0;
            
            return entries.Select(e => e.PlayerName).Distinct().Count();
        }
        
        /// <summary>
        /// Get rank tier based on score
        /// </summary>
        public RankTier GetRankTier(int score)
        {
            if (score >= 100000)
                return RankTier.Legend;
            else if (score >= 50000)
                return RankTier.Master;
            else if (score >= 25000)
                return RankTier.Diamond;
            else if (score >= 10000)
                return RankTier.Platinum;
            else if (score >= 5000)
                return RankTier.Gold;
            else if (score >= 2500)
                return RankTier.Silver;
            else if (score >= 1000)
                return RankTier.Bronze;
            else
                return RankTier.Unranked;
        }
        
        /// <summary>
        /// Get rank tier name
        /// </summary>
        public string GetRankTierName(RankTier tier)
        {
            return tier.ToString();
        }
        
        /// <summary>
        /// Get points needed for next tier
        /// </summary>
        public int GetPointsToNextTier(int currentScore)
        {
            RankTier currentTier = GetRankTier(currentScore);
            
            switch (currentTier)
            {
                case RankTier.Unranked:
                    return 1000 - currentScore;
                case RankTier.Bronze:
                    return 2500 - currentScore;
                case RankTier.Silver:
                    return 5000 - currentScore;
                case RankTier.Gold:
                    return 10000 - currentScore;
                case RankTier.Platinum:
                    return 25000 - currentScore;
                case RankTier.Diamond:
                    return 50000 - currentScore;
                case RankTier.Master:
                    return 100000 - currentScore;
                case RankTier.Legend:
                    return 0; // Already at max tier
                default:
                    return 0;
            }
        }
        
        /// <summary>
        /// Calculate score delta to reach a specific rank
        /// </summary>
        public int GetPointsToRank(List<LeaderboardEntry> entries, string playerName, int targetRank)
        {
            if (entries == null || entries.Count == 0)
                return 0;
            
            int currentRank = GetPlayerRank(entries, playerName);
            if (currentRank <= 0 || currentRank <= targetRank)
                return 0; // Already at or above target rank
            
            // Get the score of the player at target rank
            var uniquePlayerScores = entries
                .GroupBy(e => e.PlayerName)
                .Select(g => new { PlayerName = g.Key, BestScore = g.Max(e => e.Score) })
                .OrderByDescending(p => p.BestScore)
                .ToList();
            
            if (targetRank > uniquePlayerScores.Count)
                return 0;
            
            var targetPlayer = uniquePlayerScores[targetRank - 1];
            var currentPlayer = uniquePlayerScores.FirstOrDefault(p => p.PlayerName == playerName);
            
            if (currentPlayer == null)
                return 0;
            
            return targetPlayer.BestScore - currentPlayer.BestScore + 1;
        }
    }
    
    /// <summary>
    /// Rank tier enumeration
    /// </summary>
    public enum RankTier
    {
        Unranked,
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Master,
        Legend
    }
}
