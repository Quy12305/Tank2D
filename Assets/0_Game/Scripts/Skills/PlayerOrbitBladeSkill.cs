using System.Collections.Generic;
using UnityEngine;

public class PlayerOrbitBladeSkill : MonoBehaviour
{
    [SerializeField] private PlayerTank owner;
    [SerializeField] private float orbitRadius = 1.45f;
    [SerializeField] private float orbitSpeed = 180f;
    [SerializeField] private float damage = 18f;
    [SerializeField] private float hitCooldown = 0.4f;

    private readonly List<Transform> blades = new List<Transform>();
    private readonly Dictionary<BotTank, float> lastHitTimeByBot = new Dictionary<BotTank, float>();
    private float currentAngle;

    public void Initialize(PlayerTank playerTank)
    {
        owner = playerTank;
        transform.SetParent(playerTank.transform, false);
        transform.localPosition = Vector3.zero;
        CreateBlades();
    }

    private void Update()
    {
        if (owner == null || !GameManager.Instance.IsState(GameState.GamePlay))
        {
            return;
        }

        currentAngle += orbitSpeed * Time.deltaTime;

        for (int i = 0; i < blades.Count; i++)
        {
            float angle = currentAngle + (360f / blades.Count) * i;
            Vector3 offset = Quaternion.Euler(0f, 0f, angle) * Vector3.up * orbitRadius;
            Transform blade = blades[i];
            blade.localPosition = offset;
            blade.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }

        CleanupCache();
    }

    public void TryDamage(BotTank botTank)
    {
        if (botTank == null)
        {
            return;
        }

        if (lastHitTimeByBot.TryGetValue(botTank, out float lastHitTime) &&
            Time.time - lastHitTime < hitCooldown)
        {
            return;
        }

        lastHitTimeByBot[botTank] = Time.time;
        botTank.TakeDamage(damage);
    }

    private void CreateBlades()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject bladeObject = new GameObject($"Blade_{i + 1}");
            bladeObject.transform.SetParent(transform, false);
            bladeObject.transform.localScale = Vector3.one * 0.36f;

            SpriteRenderer renderer = bladeObject.AddComponent<SpriteRenderer>();
            renderer.sprite = owner.SupportSkillSprite;
            renderer.color = new Color(1f, 0.36f, 0.28f, 1f);

            CircleCollider2D collider = bladeObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.65f;

            OrbitBladeDamageDealer damageDealer = bladeObject.AddComponent<OrbitBladeDamageDealer>();
            damageDealer.Initialize(this);

            blades.Add(bladeObject.transform);
        }
    }

    private void CleanupCache()
    {
        List<BotTank> invalidKeys = null;

        foreach (KeyValuePair<BotTank, float> pair in lastHitTimeByBot)
        {
            if (pair.Key != null)
            {
                continue;
            }

            invalidKeys ??= new List<BotTank>();
            invalidKeys.Add(pair.Key);
        }

        if (invalidKeys == null)
        {
            return;
        }

        for (int i = 0; i < invalidKeys.Count; i++)
        {
            lastHitTimeByBot.Remove(invalidKeys[i]);
        }
    }
}
