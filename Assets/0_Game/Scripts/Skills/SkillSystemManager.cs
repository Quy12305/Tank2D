using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillSystemManager : Singleton<SkillSystemManager>
{
    private const string SkillProgressionId = "skill-offer-progression";

    [SerializeField] private SkillSystemConfig config;
    [SerializeField] private int fallbackKillsPerOffer = 3;

    private readonly Dictionary<SkillType, float> durationsBySkill = new Dictionary<SkillType, float>();
    private ProgressionTracker tracker;
    private PlayerTank boundPlayer;
    private bool awaitingSelection;

    public event Action<ProgressionTracker> ProgressionTrackerChanged;
    private void OnEnable()
    {
        TankSpawner.OnBotDestroyed += HandleBotDestroyed;
    }

    private void OnDisable()
    {
        TankSpawner.OnBotDestroyed -= HandleBotDestroyed;
    }

    private void Awake()
    {
        RebuildDurations();
    }

    public void BindPlayer(PlayerTank playerTank)
    {
        boundPlayer = playerTank;
    }

    public void InitializeForGameplayMode()
    {
        awaitingSelection = false;

        if (LevelManager.Instance.CurrentMode != Mode.TankWarfare)
        {
            tracker = null;
            ProgressionTrackerChanged?.Invoke(null);
            return;
        }

        tracker = ProgressionManager.Instance.CreateOrReplaceTracker(
            SkillProgressionId,
            "Skill Progress",
            $"Destroy {GetKillsPerOffer()} bots to draw 3 skills",
            GetKillsPerOffer(),
            "Skill Choice");
        ProgressionTrackerChanged?.Invoke(tracker);
    }

    public void CompleteSkillSelection(SkillType selectedSkill)
    {
        awaitingSelection = false;
        EnsureBoundPlayer();

        if (boundPlayer != null)
        {
            float duration = GetDuration(selectedSkill);
            boundPlayer.ApplyTimedSkill(selectedSkill, duration);
        }
        else
        {
            Debug.LogWarning($"Cannot apply skill {selectedSkill}: PlayerTank is not bound and no Player tag object was found.");
        }

        if (tracker != null)
        {
            tracker.Reset();
        }
    }

    private void EnsureBoundPlayer()
    {
        if (boundPlayer != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindWithTag("Player");
        boundPlayer = playerObject != null ? playerObject.GetComponent<PlayerTank>() : null;
    }

    public float GetSkillDuration(SkillType skillType)
    {
        return GetDuration(skillType);
    }

    public string GetSkillTitle(SkillType skillType)
    {
        SkillSystemConfig.SkillEntry entry = config != null ? config.GetEntry(skillType) : null;
        return entry != null && !string.IsNullOrWhiteSpace(entry.title)
            ? entry.title
            : skillType.GetTitle();
    }

    public string GetSkillDescription(SkillType skillType)
    {
        SkillSystemConfig.SkillEntry entry = config != null ? config.GetEntry(skillType) : null;
        return entry != null && !string.IsNullOrWhiteSpace(entry.description)
            ? entry.description
            : skillType.GetDescription();
    }

    public Sprite GetSkillIcon(SkillType skillType)
    {
        SkillSystemConfig.SkillEntry entry = config != null ? config.GetEntry(skillType) : null;
        return entry != null ? entry.icon : null;
    }

    private void HandleBotDestroyed(BotTank bot)
    {
        if (LevelManager.Instance == null ||
            LevelManager.Instance.CurrentMode != Mode.TankWarfare ||
            tracker == null ||
            awaitingSelection)
        {
            return;
        }

        tracker.AddProgress(1f);

        if (!tracker.IsCompleted)
        {
            return;
        }

        SkillType[] choices = GenerateChoices();
        bool didShowChoices = UIManager.Instance != null && UIManager.Instance.ShowSkillChoices(choices);
        if (didShowChoices)
        {
            awaitingSelection = true;
            return;
        }

        Debug.LogWarning("Skill choices could not be shown. Applying the first generated skill as fallback.");
        CompleteSkillSelection(choices[0]);
    }

    private SkillType[] GenerateChoices()
    {
        List<SkillType> pool = new List<SkillType>
        {
            SkillType.Shield,
            SkillType.BarrelUpgrade,
            SkillType.SideTurrets,
            SkillType.OrbitBlades
        };

        for (int i = 0; i < pool.Count; i++)
        {
            int swapIndex = UnityEngine.Random.Range(i, pool.Count);
            (pool[i], pool[swapIndex]) = (pool[swapIndex], pool[i]);
        }

        return pool.GetRange(0, Mathf.Min(3, pool.Count)).ToArray();
    }

    private void RebuildDurations()
    {
        durationsBySkill.Clear();

        SkillSystemConfig.SkillEntry[] skills = config != null ? config.skills : null;
        if (skills == null || skills.Length == 0)
        {
            AddFallbackDurations();
            return;
        }

        for (int i = 0; i < skills.Length; i++)
        {
            SkillSystemConfig.SkillEntry entry = skills[i];
            if (entry == null)
            {
                continue;
            }

            durationsBySkill[entry.skillType] = Mathf.Max(1f, entry.duration);
        }

        if (durationsBySkill.Count == 0)
        {
            AddFallbackDurations();
        }
    }

    private float GetDuration(SkillType skillType)
    {
        if (durationsBySkill.TryGetValue(skillType, out float duration))
        {
            return duration;
        }

        return 10f;
    }

    private int GetKillsPerOffer()
    {
        if (config != null)
        {
            return Mathf.Max(1, config.killsPerOffer);
        }

        return Mathf.Max(1, fallbackKillsPerOffer);
    }

    private void AddFallbackDurations()
    {
        durationsBySkill[SkillType.Shield] = 12f;
        durationsBySkill[SkillType.BarrelUpgrade] = 18f;
        durationsBySkill[SkillType.SideTurrets] = 14f;
        durationsBySkill[SkillType.OrbitBlades] = 14f;
    }
}
