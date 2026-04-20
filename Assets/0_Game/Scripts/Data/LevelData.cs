using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level Data")]
public class LevelData : ScriptableObject
{
    public                int width;
    public                int height;
    public                int botCount;
    public                int distanceBetweenBots;
    public                int wallDensity      = 25;
    [Range(2, 8)]  public int minWallLength    = 3;
    [Range(3, 12)] public int maxWallLength    = 7;
    [Range(1, 3)]  public int maxWallThickness = 1;
    public                int gem;

    [Header("Advanced Spawn")]
    public int smartBotCount = 0;
    public int dumbBotCount = 0;
    public int sentryBotCount = 1;
    public int maxActiveMobileBots = 3;
    public int respawnThreshold = 1;
    public int spawnBatchSize = 2;
    public float spawnInterval = 2f;

    [Header("Map Expansion")]
    [Range(0, 100)] public int breakableWallDensity = 12;
    public int baseWallDensityReduction = 8;
}
