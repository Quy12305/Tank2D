using DG.Tweening;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private                  Rigidbody2D rb;
    [SerializeField] private float       speed      = 10f;
    [SerializeField] private float       timeToLive = 4f;
    [SerializeField] private float       damage     = 20f;
    private                  float       currentTime;
    private                  GameObject  owner;
    private                  int         ownerLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType               = RigidbodyType2D.Dynamic;
            rb.gravityScale           = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void OnEnable() { currentTime = timeToLive; }

    public void SetDirection(Vector2 direction)
    {
        if (rb != null)
        {
            transform.up = direction;
            rb.velocity  = direction.normalized * speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == ownerLayer) return;

        var tank = collision.gameObject.GetComponent<TankBase>();
        if (tank != null)
        {
            tank.TakeDamage(damage);

            var pool = FindObjectOfType<ObjectPool>();
            if (pool != null)
            {
                pool.ReturnObject(gameObject);
            }

            DOVirtual.DelayedCall(0.01f, () =>
            {
                UIManager.Instance.UpdateTextBotInMap();

                if (GameManager.Instance.IsState(GameState.GamePlay) && LevelManager.Instance.CurrentLevel.CheckWinModeBot())
                {
                    GameManager.Instance.ChangeState(GameState.Win);
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        LevelManager.Instance.OnFinish();
                    });
                }
            });
        }
    }

    private void Update()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            var pool = FindObjectOfType<ObjectPool>();
            if (pool != null)
            {
                pool.ReturnObject(gameObject);
            }
        }
    }

    public void Setup(GameObject bulletOwner)
    {
        owner           = bulletOwner;
        ownerLayer      = bulletOwner.layer;

        SetBulletLayer();
    }

    private void SetBulletLayer()
    {
        // Thiết lập layer đặc biệt cho đạn
        if (ownerLayer == LayerMask.NameToLayer("Player"))
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerProjectile");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("EnemyProjectile");
        }
    }

}