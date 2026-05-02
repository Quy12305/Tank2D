using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "0_Game/Daily Reward Config")]
public class DailyRewardConfig : ScriptableObject
{
    [Serializable]
    public class RewardDayEntry
    {
        [Min(1)] public int dayNumber = 1;
        [Min(0)] public int rewardAmount = 50;
    }

    public RewardDayEntry[] days = new RewardDayEntry[DailyRewardManager.RewardDayCount]
    {
        new RewardDayEntry { dayNumber = 1, rewardAmount = 50 },
        new RewardDayEntry { dayNumber = 2, rewardAmount = 75 },
        new RewardDayEntry { dayNumber = 3, rewardAmount = 100 },
        new RewardDayEntry { dayNumber = 4, rewardAmount = 150 },
        new RewardDayEntry { dayNumber = 5, rewardAmount = 200 },
        new RewardDayEntry { dayNumber = 6, rewardAmount = 250 },
        new RewardDayEntry { dayNumber = 7, rewardAmount = 400 }
    };
}
