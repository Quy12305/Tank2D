using System;
using UnityEngine;

public class DailyRewardManager : Singleton<DailyRewardManager>
{
    public const int RewardDayCount = 7;

    [Serializable]
    public class DailyRewardDayInfo
    {
        public int dayIndex;
        public int rewardAmount;
        public bool isUnlocked;
        public bool isClaimed;
        public bool canClaim;
    }

    [Serializable]
    public class DailyRewardSaveData
    {
        public long cycleStartUtcTicks;
        public long lastClaimUtcTicks;
        public long completedCycleUtcTicks;
        public bool[] claimedDays;
    }

    [SerializeField] private DailyRewardConfig rewardConfig;
    [SerializeField] private int[] fallbackRewards = { 50, 75, 100, 150, 200, 250, 400 };

    private DailyRewardSaveData state = new DailyRewardSaveData();

    public event Action StateChanged;

    private void Awake()
    {
        EnsureInitialized();
    }

    public void LoadFromData(DailyRewardSaveData data)
    {
        state = data ?? new DailyRewardSaveData();
        EnsureInitialized();
        EvaluateState(false);
        StateChanged?.Invoke();
    }

    public DailyRewardSaveData GetSaveData()
    {
        EnsureInitialized();
        return new DailyRewardSaveData
        {
            cycleStartUtcTicks = state.cycleStartUtcTicks,
            lastClaimUtcTicks = state.lastClaimUtcTicks,
            completedCycleUtcTicks = state.completedCycleUtcTicks,
            claimedDays = (bool[])state.claimedDays.Clone()
        };
    }

    public DailyRewardDayInfo[] GetDayInfos()
    {
        EvaluateState(false);

        DailyRewardDayInfo[] infos = new DailyRewardDayInfo[RewardDayCount];
        int unlockedDayCount = GetUnlockedDayCount(GetUtcNow());

        for (int i = 0; i < RewardDayCount; i++)
        {
            bool isUnlocked = i < unlockedDayCount;
            bool isClaimed = state.claimedDays[i];

            infos[i] = new DailyRewardDayInfo
            {
                dayIndex = i,
                rewardAmount = GetRewardAmount(i),
                isUnlocked = isUnlocked,
                isClaimed = isClaimed,
                canClaim = isUnlocked && !isClaimed
            };
        }

        return infos;
    }

    public bool HasAnyClaimableReward()
    {
        DailyRewardDayInfo[] infos = GetDayInfos();
        for (int i = 0; i < infos.Length; i++)
        {
            if (infos[i].canClaim)
            {
                return true;
            }
        }

        return false;
    }

    public bool ClaimReward(int dayIndex)
    {
        EvaluateState(false);
        if (!CanClaim(dayIndex))
        {
            return false;
        }

        state.claimedDays[dayIndex] = true;
        state.lastClaimUtcTicks = GetUtcNow().Ticks;

        if (AreAllRewardsClaimed())
        {
            state.completedCycleUtcTicks = state.lastClaimUtcTicks;
        }

        if (Coin.Instance != null)
        {
            Coin.Instance.AddCoin(GetRewardAmount(dayIndex));
        }
        else
        {
            SaveLoadManager.Instance.SaveGame();
        }

        StateChanged?.Invoke();
        return true;
    }

    public void ForceRefresh()
    {
        EvaluateState(true);
    }

    private bool CanClaim(int dayIndex)
    {
        if (dayIndex < 0 || dayIndex >= RewardDayCount)
        {
            return false;
        }

        if (state.claimedDays[dayIndex])
        {
            return false;
        }

        return dayIndex < GetUnlockedDayCount(GetUtcNow());
    }

    private void EnsureInitialized()
    {
        if (state == null)
        {
            state = new DailyRewardSaveData();
        }

        if (state.claimedDays == null || state.claimedDays.Length != RewardDayCount)
        {
            state.claimedDays = new bool[RewardDayCount];
        }

        if (state.cycleStartUtcTicks <= 0)
        {
            ResetCycle(GetUtcNow());
        }
    }

    private void EvaluateState(bool notify)
    {
        EnsureInitialized();
        DateTime now = GetUtcNow();

        if (AreAllRewardsClaimed() &&
            state.completedCycleUtcTicks > 0 &&
            now >= new DateTime(state.completedCycleUtcTicks, DateTimeKind.Utc).AddHours(24))
        {
            ResetCycle(now);
        }

        if (notify)
        {
            StateChanged?.Invoke();
        }
    }

    private int GetUnlockedDayCount(DateTime nowUtc)
    {
        DateTime cycleStartUtc = new DateTime(state.cycleStartUtcTicks, DateTimeKind.Utc);
        double elapsedHours = Math.Max(0d, (nowUtc - cycleStartUtc).TotalHours);
        return Mathf.Clamp(1 + Mathf.FloorToInt((float)(elapsedHours / 24d)), 1, RewardDayCount);
    }

    private bool AreAllRewardsClaimed()
    {
        EnsureInitialized();
        for (int i = 0; i < state.claimedDays.Length; i++)
        {
            if (!state.claimedDays[i])
            {
                return false;
            }
        }

        return true;
    }

    private int GetRewardAmount(int dayIndex)
    {
        if (rewardConfig != null &&
            rewardConfig.days != null &&
            dayIndex >= 0 &&
            dayIndex < rewardConfig.days.Length &&
            rewardConfig.days[dayIndex] != null)
        {
            return Mathf.Max(0, rewardConfig.days[dayIndex].rewardAmount);
        }

        if (fallbackRewards == null || fallbackRewards.Length == 0)
        {
            return 0;
        }

        if (dayIndex < fallbackRewards.Length)
        {
            return fallbackRewards[dayIndex];
        }

        return fallbackRewards[fallbackRewards.Length - 1];
    }

    private void ResetCycle(DateTime nowUtc)
    {
        state.cycleStartUtcTicks = nowUtc.Ticks;
        state.lastClaimUtcTicks = 0;
        state.completedCycleUtcTicks = 0;
        state.claimedDays = new bool[RewardDayCount];
    }

    private static DateTime GetUtcNow()
    {
        return DateTime.UtcNow;
    }
}
