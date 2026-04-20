using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BotTank : TankBase
{
    [Header("AI Settings")] public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float attackDistance = 5f;
    public LayerMask obstacleLayer;

    public DynamicFlowManager flowManager;
    public Transform player;

    [SerializeField] private BotBehaviorType behaviorType = BotBehaviorType.Smart;

    private readonly List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex;
    private bool isStartMoving;
    private bool isAttacking;
    private float pathUpdateTimer;
    private float pathUpdateInterval = 2f;
    private float dumbMoveTimer;
    private Vector2 dumbMoveDirection;
    private BreakableWall cachedBreakableTarget;
    private float sentryDetectRadius = 5.5f;

    public BotBehaviorType BehaviorType => behaviorType;
    public bool IsMobile => behaviorType != BotBehaviorType.Sentry;

    public void Configure(BotBehaviorType botBehaviorType)
    {
        behaviorType = botBehaviorType;

        switch (behaviorType)
        {
            case BotBehaviorType.Smart:
                moveSpeed = 1.9f;
                rotationSpeed = 4.25f;
                attackDistance = 4.2f;
                shootCooldown = 1.15f;
                maxHealth = 240f;
                damage = 20f;
                pathUpdateInterval = 1.8f;
                break;

            case BotBehaviorType.Dumb:
                moveSpeed = 1.45f;
                rotationSpeed = 2.8f;
                attackDistance = 3.1f;
                shootCooldown = 1.65f;
                maxHealth = 180f;
                damage = 16f;
                pathUpdateInterval = 3.4f;
                break;

            case BotBehaviorType.Sentry:
                moveSpeed = 0f;
                rotationSpeed = 5.5f;
                attackDistance = 5.5f;
                sentryDetectRadius = 5.5f;
                shootCooldown = 1.3f;
                maxHealth = 140f;
                damage = 12f;
                break;
        }
    }

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();

        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
        }

        if (behaviorType == BotBehaviorType.Sentry)
        {
            SetupSentryMode();
        }
        else
        {
            flowManager = FindObjectOfType<DynamicFlowManager>();
            DynamicFlowManager.OnPathsUpdated += UpdatePath;
        }

        TankSpawner.OnPlayerSpawned += OnPlayerSpawned;
        UpdatePath();
    }

    private void SetupSentryMode()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.sortingOrder += 3;
        }
    }

    private void OnPlayerSpawned(Transform playerTransform)
    {
        player = playerTransform;
        UpdatePath();
    }

    private void UpdatePath()
    {
        if (behaviorType == BotBehaviorType.Sentry)
        {
            return;
        }

        if (player == null || flowManager == null)
        {
            return;
        }

        List<Vector3> path = flowManager.GetBotPath(this);
        if (path == null || path.Count == 0)
        {
            return;
        }

        int nearestIndex = GetNearestUsablePathIndex(path);
        currentPath.Clear();
        currentPath.AddRange(path);
        currentPathIndex = nearestIndex;
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.IsState(GameState.GamePlay))
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            return;
        }

        if (player == null)
        {
            return;
        }

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (!isStartMoving && playerRb != null && playerRb.velocity.magnitude > 0.1f)
        {
            isStartMoving = true;
        }

        if (behaviorType == BotBehaviorType.Sentry)
        {
            if (!isStartMoving)
            {
                isAttacking = false;
                return;
            }

            HandleSentry();
            return;
        }

        if (!isStartMoving)
        {
            return;
        }

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
            RotateTowards(player.position);
            return;
        }

        if (behaviorType == BotBehaviorType.Smart)
        {
            if (!FollowPath())
            {
                HandleBlockedState();
            }
        }
        else
        {
            HandleDumbMovement();
        }
    }

    private void Update()
    {
        if (player == null)
        {
            return;
        }

        if (behaviorType != BotBehaviorType.Sentry && !isStartMoving)
        {
            return;
        }

        if (isAttacking && Time.time - lastShootTime >= shootCooldown)
        {
            lastShootTime = Time.time;
            Shoot(1, behaviorType == BotBehaviorType.Sentry ? BulletType.Laser : BulletType.Normal);
        }
    }

    private void HandleSentry()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        isAttacking = distanceToPlayer <= sentryDetectRadius;

        if (isAttacking)
        {
            RotateTowards(player.position);
        }
    }

    private void CheckPlayerVisibility()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        Vector2 boxSize = new Vector2(0.16f, 0.16f);
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

        RaycastHit2D hit = Physics2D.BoxCast(
            transform.position,
            boxSize,
            angle,
            directionToPlayer,
            distanceToPlayer,
            obstacleLayer
        );

        bool hasObstacle = hit.collider != null && !hit.collider.CompareTag("Player");
        isAttacking = !hasObstacle && distanceToPlayer <= attackDistance;
    }

    private bool FollowPath()
    {
        if (currentPathIndex >= currentPath.Count)
        {
            rb.velocity = Vector2.zero;
            return false;
        }

        while (currentPathIndex < currentPath.Count && Vector3.Distance(transform.position, currentPath[currentPathIndex]) < 0.18f)
        {
            currentPathIndex++;
        }

        if (currentPathIndex >= currentPath.Count)
        {
            rb.velocity = Vector2.zero;
            return false;
        }

        Vector3 targetPos = currentPath[currentPathIndex];
        Vector2 direction = (targetPos - transform.position).normalized;

        rb.velocity = direction * moveSpeed;

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            currentPathIndex++;
        }

        if (direction.magnitude > 0.1f)
        {
            RotateTowards(targetPos);
        }

        return true;
    }

    private int GetNearestUsablePathIndex(List<Vector3> path)
    {
        int nearestIndex = 0;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < path.Count; i++)
        {
            float distance = Vector3.Distance(transform.position, path[i]);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        if (nearestIndex < path.Count - 1 && nearestDistance < 0.2f)
        {
            nearestIndex++;
        }

        return nearestIndex;
    }

    private void HandleBlockedState()
    {
        if (TryShootNearbyBreakableWall())
        {
            rb.velocity = Vector2.zero;
            return;
        }

        WanderRandomly();
    }

    private bool TryShootNearbyBreakableWall()
    {
        if (cachedBreakableTarget == null)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 2.6f, obstacleLayer);
            float closestDistance = float.MaxValue;

            foreach (Collider2D collider in colliders)
            {
                BreakableWall breakableWall = collider.GetComponent<BreakableWall>();
                if (breakableWall == null)
                {
                    continue;
                }

                float distance = Vector2.Distance(transform.position, breakableWall.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    cachedBreakableTarget = breakableWall;
                }
            }
        }

        if (cachedBreakableTarget == null)
        {
            return false;
        }

        RotateTowards(cachedBreakableTarget.transform.position);

        if (Time.time - lastShootTime >= shootCooldown)
        {
            lastShootTime = Time.time;
            Shoot(1, BulletType.Normal);
        }

        if (Vector2.Distance(transform.position, cachedBreakableTarget.transform.position) > 3f)
        {
            cachedBreakableTarget = null;
        }

        return true;
    }

    private void HandleDumbMovement()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackDistance + 1f)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
            RotateTowards(player.position);
            return;
        }

        WanderRandomly();
    }

    private void WanderRandomly()
    {
        dumbMoveTimer -= Time.fixedDeltaTime;
        if (dumbMoveTimer <= 0f)
        {
            dumbMoveTimer = Random.Range(1.2f, 2.4f);
            dumbMoveDirection = Random.insideUnitCircle.normalized;
            if (dumbMoveDirection.sqrMagnitude < 0.1f)
            {
                dumbMoveDirection = Vector2.up;
            }
        }

        rb.velocity = dumbMoveDirection * moveSpeed * 0.85f;
        RotateTowards((Vector2)transform.position + dumbMoveDirection);
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.AngleAxis(angle, Vector3.forward),
            rotationSpeed * Time.deltaTime
        );
    }

    private void OnDestroy()
    {
        DynamicFlowManager.OnPathsUpdated -= UpdatePath;
        TankSpawner.OnPlayerSpawned -= OnPlayerSpawned;
    }

    protected override void OnDeath()
    {
        TankSpawner.Instance?.NotifyBotDestroyed(this);
        base.OnDeath();
    }
}
