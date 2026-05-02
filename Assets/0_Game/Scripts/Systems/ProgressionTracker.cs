using System;
using UnityEngine;

public class ProgressionTracker
{
    public string Id { get; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public float CurrentValue { get; private set; }
    public float TargetValue { get; private set; }
    public bool IsCompleted { get; private set; }
    public string RewardLabel { get; private set; }

    public float NormalizedProgress => TargetValue <= 0f ? 1f : Mathf.Clamp01(CurrentValue / TargetValue);

    public event Action<ProgressionTracker> Updated;
    public event Action<ProgressionTracker> Completed;
    public event Action<ProgressionTracker> Reseted;

    public ProgressionTracker(string id, string title, string description, float targetValue, string rewardLabel)
    {
        Id = id;
        Title = title;
        Description = description;
        TargetValue = Mathf.Max(1f, targetValue);
        RewardLabel = rewardLabel;
    }

    public void Configure(string title, string description, float targetValue, string rewardLabel)
    {
        Title = title;
        Description = description;
        TargetValue = Mathf.Max(1f, targetValue);
        RewardLabel = rewardLabel;
        CurrentValue = Mathf.Clamp(CurrentValue, 0f, TargetValue);
        Updated?.Invoke(this);
    }

    public void SetValue(float value)
    {
        CurrentValue = Mathf.Clamp(value, 0f, TargetValue);
        bool completedNow = CurrentValue >= TargetValue;

        if (completedNow && !IsCompleted)
        {
            IsCompleted = true;
            Updated?.Invoke(this);
            Completed?.Invoke(this);
            return;
        }

        if (!completedNow)
        {
            IsCompleted = false;
        }

        Updated?.Invoke(this);
    }

    public void AddProgress(float amount)
    {
        if (IsCompleted)
        {
            return;
        }

        SetValue(CurrentValue + Mathf.Max(0f, amount));
    }

    public void Reset()
    {
        CurrentValue = 0f;
        IsCompleted = false;
        Reseted?.Invoke(this);
        Updated?.Invoke(this);
    }
}
