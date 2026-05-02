using System.Collections.Generic;
using UnityEngine;

public class TimerManager : Singleton<TimerManager>
{
    private readonly List<GameTimer> activeTimers = new List<GameTimer>();
    private readonly Dictionary<string, GameTimer> timersById = new Dictionary<string, GameTimer>();

    public GameTimer CreateTimer(
        string id,
        float duration,
        TimerDirection direction = TimerDirection.CountDown,
        bool autoStart = false,
        bool isLooping = false,
        bool useUnscaledTime = false)
    {
        if (timersById.TryGetValue(id, out GameTimer existingTimer))
        {
            RemoveTimer(id);
        }

        GameTimer timer = new GameTimer(id, duration, direction, isLooping, useUnscaledTime);
        RegisterTimer(timer);

        if (autoStart)
        {
            timer.Start();
        }

        return timer;
    }

    public void RegisterTimer(GameTimer timer)
    {
        if (timer == null)
        {
            return;
        }

        RemoveTimer(timer.Id);
        activeTimers.Add(timer);
        timersById[timer.Id] = timer;
    }

    public bool TryGetTimer(string id, out GameTimer timer)
    {
        return timersById.TryGetValue(id, out timer);
    }

    public GameTimer GetTimer(string id)
    {
        timersById.TryGetValue(id, out GameTimer timer);
        return timer;
    }

    public void RemoveTimer(string id)
    {
        if (!timersById.TryGetValue(id, out GameTimer timer))
        {
            return;
        }

        activeTimers.Remove(timer);
        timersById.Remove(id);
    }

    public void ClearAll()
    {
        activeTimers.Clear();
        timersById.Clear();
    }

    private void Update()
    {
        for (int i = activeTimers.Count - 1; i >= 0; i--)
        {
            GameTimer timer = activeTimers[i];
            if (timer == null)
            {
                activeTimers.RemoveAt(i);
                continue;
            }

            float deltaTime = timer.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            timer.Tick(deltaTime);
        }
    }
}
