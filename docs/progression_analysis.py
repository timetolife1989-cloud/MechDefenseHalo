#!/usr/bin/env python3
"""
XP Curve Visualization and Verification Script
Demonstrates the progression system's XP requirements
"""

def calculate_xp_for_level(level):
    """Calculate XP required to go from (level-1) to level"""
    if level <= 1:
        return 0
    
    # Level 1-10: Linear (100 XP per level)
    if level <= 10:
        return 100
    # Level 11-30: Quadratic growth starting at 150 XP
    elif level <= 30:
        levels_above_10 = level - 10
        return 150 + (levels_above_10 * levels_above_10 * 10)
    # Level 31-60: Steeper quadratic growth
    elif level <= 60:
        levels_above_30 = level - 30
        return 2250 + (levels_above_30 * levels_above_30 * 25)
    # Level 61-100: Very steep growth for endgame
    else:
        levels_above_60 = level - 60
        return 24750 + (levels_above_60 * levels_above_60 * 50)

def calculate_total_xp(level):
    """Calculate total XP required to reach a level from level 1"""
    if level <= 1:
        return 0
    
    total = 0
    for i in range(2, level + 1):
        total += calculate_xp_for_level(i)
    return total

def main():
    print("=" * 80)
    print("PLAYER PROGRESSION SYSTEM - XP CURVE ANALYSIS")
    print("=" * 80)
    print()
    
    # Show progression milestones
    milestones = [1, 5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100]
    
    print("Level Progression Milestones:")
    print("-" * 80)
    print(f"{'Level':<10} {'XP to Next':<15} {'Total XP':<15} {'Hours Estimate':<20}")
    print("-" * 80)
    
    for level in milestones:
        xp_for_next = calculate_xp_for_level(level + 1) if level < 100 else 0
        total_xp = calculate_total_xp(level)
        
        # Estimate hours (assuming ~20,000 XP per hour of gameplay)
        hours = total_xp / 20000
        
        print(f"{level:<10} {xp_for_next:<15,} {total_xp:<15,} {hours:<20.1f}")
    
    print("-" * 80)
    print()
    
    # Show XP sources and their impact
    print("XP Sources:")
    print("-" * 80)
    print(f"{'Source':<30} {'XP Gain':<20} {'Actions to Level 2':<25}")
    print("-" * 80)
    
    sources = [
        ("Enemy Kill (Level 1)", 10, 100 / 10),
        ("Enemy Kill (Level 5)", 50, 100 / 50),
        ("Wave 1 Complete", 100, 100 / 100),
        ("Wave 5 Complete", 500, 100 / 500),
        ("Boss Tier 1 Defeat", 500, 100 / 500),
        ("Boss Tier 3 Defeat", 1500, 100 / 1500),
        ("Crafting Item", 20, 100 / 20),
    ]
    
    for source, xp, actions in sources:
        print(f"{source:<30} {xp:<20,} {actions:<25.1f}")
    
    print("-" * 80)
    print()
    
    # Show level rewards
    print("Level Reward Milestones:")
    print("-" * 80)
    rewards = [
        (10, "Second Weapon Slot"),
        (20, "Third Drone Slot"),
        (30, "Crafting Speed +10%"),
        (50, "Fourth Weapon Slot + 500 Cores"),
        (100, "Prestige System + Legendary Item"),
    ]
    
    for level, reward in rewards:
        total_xp = calculate_total_xp(level)
        hours = total_xp / 20000
        print(f"Level {level:>3}: {reward:<40} (Est. {hours:.1f} hours)")
    
    print("-" * 80)
    print()
    
    # Show prestige system
    print("Prestige System:")
    print("-" * 80)
    print("Available at Level 100")
    print("- Resets level to 1")
    print("- Keeps all items and currency")
    print("- Grants +5% to all stats per prestige")
    print("- Maximum 10 prestiges (+50% total)")
    print()
    
    for prestige in range(1, 11):
        bonus = prestige * 5
        print(f"Prestige {prestige:>2}: +{bonus:>2}% All Stats")
    
    print("-" * 80)
    print()
    
    total_xp_100 = calculate_total_xp(100)
    print(f"Total XP to Level 100: {total_xp_100:,}")
    print(f"Estimated playtime to max level: {total_xp_100 / 20000:.1f} hours")
    print()
    print("=" * 80)

if __name__ == "__main__":
    main()
