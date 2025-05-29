using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class BotTank : TankBase
{
    [Header("AI Settings")] public float     moveSpeed      = 2f;
    public                         float     rotationSpeed  = 5f;
    public                         float     attackDistance = 5f;
    public                         LayerMask obstacleLayer;

    public  DynamicFlowManager flowManager;
    public  Transform          player;
    private List<Vector3>      currentPath      = new List<Vector3>();
    private int                currentPathIndex = 0;
    private bool               isStartMoving         = false;
    private bool               isAttacking      = false;

    private float pathUpdateTimer    = 0f;
    private float pathUpdateInterval = 2f;

    protected override void Start()
    {
        base.Start();
        rb          = GetComponent<Rigidbody2D>();
        flowManager = FindObjectOfType<DynamicFlowManager>();

        TankSpawner.OnPlayerSpawned       += OnPlayerSpawned;
        DynamicFlowManager.OnPathsUpdated += UpdatePath;

        DOVirtual.DelayedCall(0.1f, () =>
        {
            GameObject foundPlayer = GameObject.FindWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer.transform;
                UpdatePath();
            }
        });
    }

    private void OnPlayerSpawned(Transform playerTransform)
    {
        player = playerTransform;
        UpdatePath();
    }

    private void UpdatePath()
    {
        if (player == null || flowManager == null) return;

        Vector2Int    fromGrid = flowManager.WorldToGridPosition(transform.position);
        List<Vector3> path     = flowManager.GetBotPath(fromGrid);

        currentPath      = path;
        currentPathIndex = 0;
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsState(GameState.GamePlay))
        {
            rb.velocity = Vector2.zero;
        }

        if (player == null) return;

        if (!this.isStartMoving && this.player.gameObject.GetComponent<Rigidbody2D>().velocity.magnitude > 0.1f)
        {
            this.isStartMoving = true;
        }

        if (!this.isStartMoving) return;

        pathUpdateTimer += Time.fixedDeltaTime;
        if (pathUpdateTimer >= pathUpdateInterval)
        {
            pathUpdateTimer = 0f;
            UpdatePath();
        }

        CheckPlayerVisibility();

        if (isAttacking)
        {
            rb.velocity = Vector2.zero;
            RotateTowardsPlayer();
        }
        else
        {
            FollowPath();
        }
    }

    private void Update()
    {
        if (player == null || !this.isStartMoving) return;

        if (isAttacking && Time.time - lastShootTime >= shootCooldown)
        {
            lastShootTime = Time.time;
            Shoot(1);
        }

        // Vẽ BoxCast
        Vector2 boxSize           = new Vector2(0.16f, 0.16f);
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float   angle             = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        Vector3[] points = new Vector3[8];
        Vector2   right  = Quaternion.Euler(0, 0, angle) * Vector2.right * boxSize.x / 2;
        Vector2   up     = Quaternion.Euler(0, 0, angle) * Vector2.up * boxSize.y / 2;

        points[0] = (Vector2)transform.position + right + up;
        points[1] = (Vector2)transform.position + right - up;
        points[2] = (Vector2)transform.position - right - up;
        points[3] = (Vector2)transform.position - right + up;

        for (int i = 0; i < 4; i++)
        {
            Debug.DrawLine(points[i], points[i] + (Vector3)(directionToPlayer * attackDistance), Color.red);
        }
    }

    private void CheckPlayerVisibility()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float   distanceToPlayer  = Vector2.Distance(transform.position, player.position);

        Vector2 boxSize = new Vector2(0.16f, 0.16f);
        // Góc xoay của box cast theo hướng bắn
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position, // Điểm bắt đầu
            boxSize,            // Kích thước box
            angle,              // Góc xoay của box
            directionToPlayer,  // Hướng bắn
            distanceToPlayer,   // Khoảng cách tối đa
            obstacleLayer       // Layer mask
        );

        bool hasObstacle = hit.collider != null && !hit.collider.CompareTag("Player");

        // Sửa logic điều kiện tấn công
        if (!hasObstacle && distanceToPlayer <= attackDistance)
        {
            isAttacking = true;
            rb.velocity = Vector2.zero;
        }
        else
        {
            isAttacking = false;
        }
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPathIndex >= currentPath.Count) return;

        Vector3 targetPos = currentPath[currentPathIndex];
        Vector2 direction = (targetPos - transform.position).normalized;

        rb.velocity = direction * moveSpeed;

        // Giảm ngưỡng kiểm tra điểm đến
        if (Vector3.Distance(transform.position, targetPos) < 0.3f)
        {
            currentPathIndex++;
        }

        // Giữ nguyên logic xoay
        if (direction.magnitude > 0.1f)
        {
            float      angle     = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRot = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private void RotateTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float   angle     = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotationSpeed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        DynamicFlowManager.OnPathsUpdated -= UpdatePath;
        TankSpawner.OnPlayerSpawned       -= OnPlayerSpawned;
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        Debug.Log("BotTank is dead");
    }
}