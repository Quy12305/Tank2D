public enum SkillType
{
    Shield,
    BarrelUpgrade,
    SideTurrets,
    OrbitBlades
}

public static class SkillTypeExtensions
{
    public static string GetTitle(this SkillType skillType)
    {
        return skillType switch
        {
            SkillType.Shield => "Energy Shield",
            SkillType.BarrelUpgrade => "Barrel Upgrade",
            SkillType.SideTurrets => "Side Turrets",
            SkillType.OrbitBlades => "Orbit Blades",
            _ => "Unknown Skill"
        };
    }

    public static string GetDescription(this SkillType skillType)
    {
        return skillType switch
        {
            SkillType.Shield => "Blocks enemy bullets while the shield is active.",
            SkillType.BarrelUpgrade => "Increase barrel level by 1. Max level is 3.",
            SkillType.SideTurrets => "Summon 2 helper turrets that auto-fire at nearby bots.",
            SkillType.OrbitBlades => "Summon 3 orbiting blades that damage bots on contact.",
            _ => string.Empty
        };
    }
}
