using UnityEngine;

[CreateAssetMenu(fileName = "Bot Data", menuName = "Bot Data")]
public class BotData : ScriptableObject
{
    [Header("Basic Info")] public float speed;
    public                        float damage;
    public                        float health;
    public                        float attackDistance;
    public                        float attackCooldown;
}