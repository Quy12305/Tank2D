using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Basic Info")] public float speed;
    public                        float health;
    public                        float damage;
    public                        Sprite tankImg;
    public                        int   cost;
}