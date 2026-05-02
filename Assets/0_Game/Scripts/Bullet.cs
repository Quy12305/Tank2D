using DG.Tweening;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    private ObjectPool objectPool;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float timeToLive = 4f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private BulletType bulletType = BulletType.Normal;
    [SerializeField] [Range(0.1f, 1f)] private float laserDamageMultiplier = 0.6f;
    [SerializeField] private float freezeDuration = 5f;

    private float currentTime;
    private GameObject owner;
    private int ownerLayer;

    public BulletType BulletType => bulletType;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        objectPool = FindObjectOfType<ObjectPool>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void OnEnable()
    {
        currentTime = timeToLive;
    }

    private void OnDisable()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void SetDamage(float damageValue)
    {
        damage = damageValue;
    }

    public void SetDirection(Vector2 direction)
    {
        if (rb == null)
        {
            return;
        }

        transform.up = direction;
        rb.velocity = direction.normalized * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == ownerLayer)
        {
            return;
        }

        BreakableWall breakableWall = collision.gameObject.GetComponent<BreakableWall>();
        if (breakableWall != null)
        {
            breakableWall.RegisterHit();
            ReturnToPool();
            return;
        }

        TankBase tank = collision.gameObject.GetComponent<TankBase>();
        if (tank != null)
        {
            ApplyHitToTank(tank);
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (bulletType != BulletType.Laser)
        {
            return;
        }

        if (other.gameObject.layer == ownerLayer)
        {
            return;
        }

        TankBase tank = other.GetComponent<TankBase>();
        if (tank == null)
        {
            return;
        }

        ApplyHitToTank(tank);
        ReturnToPool();
    }

    private void Update()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0f)
        {
            ReturnToPool();
        }
    }

    public void Setup(GameObject bulletOwner)
    {
        owner = bulletOwner;
        ownerLayer = bulletOwner.layer;

        float ownerDamage = bulletOwner.GetComponent<TankBase>().damage;
        if (bulletType == BulletType.Laser)
        {
            ownerDamage *= laserDamageMultiplier;
        }

        SetDamage(ownerDamage);
        SetBulletLayer();
    }

    private void SetBulletLayer()
    {
        if (ownerLayer == LayerMask.NameToLayer("Player"))
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("EnemyProjectile");
        }
    }

    private void ReturnToPool()
    {
        if (objectPool == null)
        {
            objectPool = FindObjectOfType<ObjectPool>();
        }

        if (objectPool != null)
        {
            objectPool.ReturnObject(gameObject);
        }
    }

    private void ApplyHitToTank(TankBase tank)
    {
        if (tank is PlayerTank playerTank &&
            ownerLayer == LayerMask.NameToLayer("Enemy") &&
            playerTank.TryBlockEnemyProjectile())
        {
            return;
        }

        tank.TakeDamage(damage);

        if (bulletType == BulletType.Freeze && tank is BotTank botTank)
        {
            botTank.ApplyFreeze(freezeDuration);
        }

        DOVirtual.DelayedCall(0.01f, () =>
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateTextBotInMap();
            }
        });
    }
}
