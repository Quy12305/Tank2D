using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillSystemConfig", menuName = "0_Game/Skill System Config")]
public class SkillSystemConfig : ScriptableObject
{
    [Serializable]
    public class SkillEntry
    {
        public SkillType skillType;
        public string title;
        [TextArea(2, 4)] public string description;
        public Sprite icon;
        [Min(1)] public float duration = 10f;
    }

    [Min(1)] public int killsPerOffer = 3;

    public SkillEntry[] skills = new SkillEntry[]
    {
        new SkillEntry
        {
            skillType = SkillType.Shield,
            title = "Energy Shield",
            description = "Blocks enemy bullets while the shield is active.",
            duration = 12f
        },
        new SkillEntry
        {
            skillType = SkillType.BarrelUpgrade,
            title = "Barrel Upgrade",
            description = "Increase barrel level by 1. Max level is 3.",
            duration = 18f
        },
        new SkillEntry
        {
            skillType = SkillType.SideTurrets,
            title = "Side Turrets",
            description = "Summon 2 helper turrets that auto-fire at nearby bots.",
            duration = 14f
        },
        new SkillEntry
        {
            skillType = SkillType.OrbitBlades,
            title = "Orbit Blades",
            description = "Summon 3 orbiting blades that damage bots on contact.",
            duration = 14f
        }
    };

    public SkillEntry GetEntry(SkillType skillType)
    {
        if (skills == null)
        {
            return null;
        }

        for (int i = 0; i < skills.Length; i++)
        {
            SkillEntry entry = skills[i];
            if (entry != null && entry.skillType == skillType)
            {
                return entry;
            }
        }

        return null;
    }
}
