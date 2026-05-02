using System;
using System.Collections.Generic;

public class ProgressionManager : Singleton<ProgressionManager>
{
    private readonly Dictionary<string, ProgressionTracker> trackers = new Dictionary<string, ProgressionTracker>();

    public event Action<ProgressionTracker> TrackerRegistered;
    public event Action<string> TrackerRemoved;

    public ProgressionTracker CreateOrReplaceTracker(
        string id,
        string title,
        string description,
        float targetValue,
        string rewardLabel)
    {
        if (trackers.TryGetValue(id, out ProgressionTracker tracker))
        {
            tracker.Configure(title, description, targetValue, rewardLabel);
            tracker.Reset();
            TrackerRegistered?.Invoke(tracker);
            return tracker;
        }

        tracker = new ProgressionTracker(id, title, description, targetValue, rewardLabel);
        trackers[id] = tracker;
        TrackerRegistered?.Invoke(tracker);
        return tracker;
    }

    public bool TryGetTracker(string id, out ProgressionTracker tracker)
    {
        return trackers.TryGetValue(id, out tracker);
    }

    public ProgressionTracker GetTracker(string id)
    {
        trackers.TryGetValue(id, out ProgressionTracker tracker);
        return tracker;
    }

    public void RemoveTracker(string id)
    {
        if (!trackers.Remove(id))
        {
            return;
        }

        TrackerRemoved?.Invoke(id);
    }

    public void Clear()
    {
        string[] ids = new string[trackers.Count];
        trackers.Keys.CopyTo(ids, 0);

        trackers.Clear();

        for (int i = 0; i < ids.Length; i++)
        {
            TrackerRemoved?.Invoke(ids[i]);
        }
    }
}
