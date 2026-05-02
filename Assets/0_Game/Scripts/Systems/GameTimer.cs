using System;
using UnityEngine;

public enum TimerDirection
{
    CountUp,
    CountDown
}

public class GameTimer
{
    public string Id { get; }
    public float Duration { get; private set; }
    public float ElapsedTime { get; private set; }
    public bool IsLooping { get; private set; }
    public bool UseUnscaledTime { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsCompleted { get; private set; }
    public TimerDirection Direction { get; private set; }

    public float RemainingTime => Mathf.Max(0f, Duration - ElapsedTime);
    public float Progress => Duration <= 0f ? 1f : Mathf.Clamp01(ElapsedTime / Duration);
    public float CurrentTime => Direction == TimerDirection.CountDown ? RemainingTime : ElapsedTime;

    public event Action<GameTimer> Started;
    public event Action<GameTimer> Updated;
    public event Action<GameTimer> Paused;
    public event Action<GameTimer> Resumed;
    public event Action<GameTimer> Stopped;
    public event Action<GameTimer> Completed;
    public event Action<GameTimer> Reseted;

    public GameTimer(
        string id,
        float duration,
        TimerDirection direction = TimerDirection.CountDown,
        bool isLooping = false,
        bool useUnscaledTime = false)
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        Duration = Mathf.Max(0f, duration);
        Direction = direction;
        IsLooping = isLooping;
        UseUnscaledTime = useUnscaledTime;
    }

    public void Start()
    {
        if (IsRunning && !IsPaused)
        {
            return;
        }

        if (IsCompleted)
        {
            Reset();
        }

        IsRunning = true;
        IsPaused = false;
        Started?.Invoke(this);
    }

    public void Restart()
    {
        Reset();
        Start();
    }

    public void Pause()
    {
        if (!IsRunning || IsPaused || IsCompleted)
        {
            return;
        }

        IsPaused = true;
        Paused?.Invoke(this);
    }

    public void Resume()
    {
        if (!IsRunning || !IsPaused || IsCompleted)
        {
            return;
        }

        IsPaused = false;
        Resumed?.Invoke(this);
    }

    public void Stop()
    {
        if (!IsRunning && !IsPaused)
        {
            return;
        }

        IsRunning = false;
        IsPaused = false;
        Stopped?.Invoke(this);
    }

    public void Reset()
    {
        ElapsedTime = 0f;
        IsCompleted = false;
        IsPaused = false;
        IsRunning = false;
        Reseted?.Invoke(this);
        Updated?.Invoke(this);
    }

    public void SetDuration(float duration)
    {
        Duration = Mathf.Max(0f, duration);
        ElapsedTime = Mathf.Clamp(ElapsedTime, 0f, Duration);
        Updated?.Invoke(this);
    }

    public void SetDirection(TimerDirection direction)
    {
        Direction = direction;
        Updated?.Invoke(this);
    }

    public void SetLooping(bool isLooping)
    {
        IsLooping = isLooping;
    }

    public void SetElapsedTime(float elapsedTime)
    {
        ElapsedTime = Mathf.Clamp(elapsedTime, 0f, Duration);
        IsCompleted = Mathf.Approximately(ElapsedTime, Duration);
        Updated?.Invoke(this);
    }

    public void AddTime(float time)
    {
        SetElapsedTime(ElapsedTime + Mathf.Max(0f, time));
    }

    public void SubtractTime(float time)
    {
        SetElapsedTime(ElapsedTime - Mathf.Max(0f, time));
    }

    public void Tick(float deltaTime)
    {
        if (!IsRunning || IsPaused || IsCompleted)
        {
            return;
        }

        if (Duration <= 0f)
        {
            Finish();
            return;
        }

        ElapsedTime = Mathf.Min(Duration, ElapsedTime + Mathf.Max(0f, deltaTime));
        Updated?.Invoke(this);

        if (ElapsedTime >= Duration)
        {
            Finish();
        }
    }

    private void Finish()
    {
        IsCompleted = true;
        Completed?.Invoke(this);

        if (IsLooping)
        {
            ElapsedTime = 0f;
            IsCompleted = false;
            Updated?.Invoke(this);
            return;
        }

        IsRunning = false;
        IsPaused = false;
    }
}
