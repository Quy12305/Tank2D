using UnityEngine;

public class PlayerSupportTurretSkill : MonoBehaviour
{
    [SerializeField] private PlayerTank owner;
    [SerializeField] private Vector3 localOffset;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float shootCooldown = 0.55f;

    private float lastShootTime = -Mathf.Infinity;
    private SpriteRenderer spriteRenderer;

    public void Initialize(PlayerTank playerTank, Vector3 offset)
    {
        owner = playerTank;
        localOffset = offset;
        transform.SetParent(playerTank.transform, false);
        transform.localPosition = offset;
        transform.localRotation = Quaternion.identity;

        spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = owner.SupportSkillSprite;
        spriteRenderer.color = new Color(0.99f, 0.76f, 0.26f, 1f);
        transform.localScale = Vector3.one * 0.32f;
    }

    private void Update()
    {
        if (owner == null || !GameManager.Instance.IsState(GameState.GamePlay))
        {
            return;
        }

        transform.position = owner.transform.position + owner.transform.TransformDirection(localOffset);

        BotTank target = FindNearestTarget();
        if (target == null)
        {
            return;
        }

        Vector2 shootDirection = (target.transform.position - transform.position).normalized;
        if (shootDirection.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (Time.time - lastShootTime < shootCooldown)
        {
            return;
        }

        lastShootTime = Time.time;
        owner.SpawnSupportBullet(transform.position, shootDirection, BulletType.Normal);
    }

    private BotTank FindNearestTarget()
    {
        BotTank[] bots = FindObjectsOfType<BotTank>();
        BotTank bestTarget = null;
        float bestDistance = detectionRadius * detectionRadius;

        for (int i = 0; i < bots.Length; i++)
        {
            BotTank bot = bots[i];
            if (bot == null)
            {
                continue;
            }

            float distance = (bot.transform.position - transform.position).sqrMagnitude;
            if (distance > bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            bestTarget = bot;
        }

        return bestTarget;
    }
}
