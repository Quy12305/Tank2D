using UnityEngine;

// Lớp cơ sở TankBase
public abstract class TankBase : MonoBehaviour
{
    [Header("Common Settings")] public Transform bulletSpawnPoint;
    public                             float     maxHealth = 100f;
    public                             HealthBar healthBar;

    protected ObjectPool  objectPool;
    protected Rigidbody2D rb;
    protected float       currentHealth;

    protected virtual void Start()
    {
        objectPool    = FindObjectOfType<ObjectPool>();
        rb            = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.OnInit(maxHealth, transform);
        }

        SetupRigidbody();
    }

    private void SetupRigidbody()
    {
        if (rb != null)
        {
            rb.bodyType               = RigidbodyType2D.Dynamic;
            rb.gravityScale           = 0f;
            rb.drag                   = 1f;
            rb.angularDrag            = 1f;
            rb.constraints            = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    protected void Shoot()
    {
        if (bulletSpawnPoint != null && objectPool != null)
        {
            GameObject bullet = objectPool.GetObject();
            bullet.transform.position = bulletSpawnPoint.position;
            bullet.transform.rotation = transform.rotation;

            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                bulletComponent.SetDirection(transform.up);
                bulletComponent.Setup(gameObject);
            }
        }
    }

    public virtual void TakeDamage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.SetNewHp(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}