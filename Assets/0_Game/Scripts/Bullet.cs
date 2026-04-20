using DG.Tweening;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float timeToLive = 4f;
    [SerializeField] private float damage = 20f;
    [SerializeField] private BulletType bulletType = BulletType.Normal;
    [SerializeField] [Range(0.1f, 1f)] private float laserDamageMultiplier = 0.6f;

    private float currentTime;
    private GameObject owner;
    private int ownerLayer;

    public BulletType BulletType => bulletType;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
            tank.TakeDamage(damage);
            ReturnToPool();

            DOVirtual.DelayedCall(0.01f, () =>
            {
                UIManager.Instance.UpdateTextBotInMap();
            });
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

        tank.TakeDamage(damage);
        ReturnToPool();

        DOVirtual.DelayedCall(0.01f, () =>
        {
            UIManager.Instance.UpdateTextBotInMap();
        });
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
        ObjectPool pool = FindObjectOfType<ObjectPool>();
        if (pool != null)
        {
            pool.ReturnObject(gameObject);
        }
    }
}
