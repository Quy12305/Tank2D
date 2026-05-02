using DG.Tweening;
using UnityEngine;

public abstract class TankBase : MonoBehaviour
{
    [Header("Common Settings")] public Transform  bulletSpawnPoint;
    public                             GameObject vfxFire;
    public                             float      maxHealth;
    public                             HealthBar  healthBar;
    public                             float      shootCooldown;
    public                             float      lastShootTime = -Mathf.Infinity;
    public                             float      damage;

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

    protected void Shoot(int t, BulletType bulletType = BulletType.Normal)
    {
        if (bulletSpawnPoint == null || objectPool == null) return;

        Vector2 shootDir = transform.up;
        Vector3 spawnPos = bulletSpawnPoint.position;

        switch (t)
        {
            case 1: SpawnBullet(spawnPos, shootDir, bulletType); break;

            case 2:
            {
                float   offset = 0.1f;
                Vector3 right  = transform.right * offset;
                SpawnBullet(spawnPos + right, shootDir, bulletType);
                SpawnBullet(spawnPos - right, shootDir, bulletType);
                break;
            }

            case 3:
            {
                float angleOffset = 15f * Mathf.Deg2Rad;

                float currentAngle = Mathf.Atan2(shootDir.y, shootDir.x);

                Vector2 dirLeft = new Vector2(
                    Mathf.Cos(currentAngle - angleOffset),
                    Mathf.Sin(currentAngle - angleOffset)
                );

                Vector2 dirRight = new Vector2(
                    Mathf.Cos(currentAngle + angleOffset),
                    Mathf.Sin(currentAngle + angleOffset)
                );

                Vector2 dirMiddle = shootDir;

                SpawnBullet(spawnPos, dirLeft.normalized, bulletType);
                SpawnBullet(spawnPos, dirMiddle.normalized, bulletType);
                SpawnBullet(spawnPos, dirRight.normalized, bulletType);
                break;
            }
        }

        SoundManager.Instance.OnShoot();
    }

    private void SpawnBullet(Vector3 position, Vector2 direction, BulletType bulletType)
    {
        GameObject bullet = objectPool.GetObject(bulletType);
        if (bullet == null)
        {
            return;
        }

        bullet.transform.position = position;
        bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.SetDirection(direction);
            bulletComponent.Setup(gameObject);
        }
    }

    public void SpawnSupportBullet(Vector3 position, Vector2 direction, BulletType bulletType, GameObject bulletOwnerOverride = null)
    {
        if (objectPool == null)
        {
            objectPool = FindObjectOfType<ObjectPool>();
        }

        if (objectPool == null)
        {
            return;
        }

        GameObject bullet = objectPool.GetObject(bulletType);
        if (bullet == null)
        {
            return;
        }

        bullet.transform.position = position;
        bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);

        Bullet bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            bulletComponent.SetDirection(direction);
            bulletComponent.Setup(bulletOwnerOverride != null ? bulletOwnerOverride : gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);

        if (healthBar != null)
        {
            healthBar.SetNewHp(currentHealth);
        }

        SoundManager.Instance.OnTakeDamage();

        if (currentHealth <= 0)
        {
            this.OnDeath();
            SoundManager.Instance.OnDeath();
        }
    }

    protected virtual void OnDeath()
    {
        Destroy(this.healthBar.gameObject);
        Destroy(this.gameObject);
        GameObject fire = Instantiate(vfxFire, transform.position, Quaternion.identity);
    }
}
