using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("Basic Info")] public float speed;
    public                        float damage;
    public                        float health;
    public                        float attackCooldown;
}