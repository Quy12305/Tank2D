using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Level Data")]
public class LevelData : ScriptableObject
{
    public                int width;
    public                int height;
    public                int botCount;
    public                int distanceBetweenBots;
    public                int wallDensity         = 25;
    [Range(2, 8)]  public int minWallLength       = 3;
    [Range(3, 12)] public int maxWallLength       = 7;
    [Range(1, 3)]  public int maxWallThickness    = 1;
}