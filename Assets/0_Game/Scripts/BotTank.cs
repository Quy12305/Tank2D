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
    private float blockedProbeTimer;
    private Vector2 blockedProbeDirection;
    private float sentryDetectRadius = 5.5f;
    private bool isFrozen;
    private GameTimer freezeTimer;
    private SpriteRenderer[] cachedRenderers;
    private Color[] defaultColors;
    private Vector3 lastStuckCheckPosition;
    private float stuckTimer;
    [SerializeField] private float breakableSearchRadius = 4.5f;
    [SerializeField] private float breakableAttackRange = 3.6f;
    [SerializeField] private float blockedMoveSpeedMultiplier = 0.65f;
    [SerializeField] private float blockedRandomFireRange = 2.8f;
    [SerializeField] private float waypointReachDistance = 0.35f;
    [SerializeField] private float dumbObstacleProbeDistance = 0.8f;
    [SerializeField] private float stuckCheckInterval = 0.45f;
    [SerializeField] private float stuckMoveThreshold = 0.08f;

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
        cachedRenderers = GetComponentsInChildren<SpriteRenderer>();
        defaultColors = new Color[cachedRenderers.Length];
        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            defaultColors[i] = cachedRenderers[i].color;
        }

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
        lastStuckCheckPosition = transform.position;
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
            currentPath.Clear();
            currentPathIndex = 0;
            return;
        }

        int nearestIndex = GetNearestUsablePathIndex(path);
        if (currentPath.Count > 0 &&
            currentPathIndex < currentPath.Count &&
            nearestIndex <= currentPathIndex + 1 &&
            Vector3.Distance(transform.position, currentPath[currentPathIndex]) > waypointReachDistance)
        {
            return;
        }

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

        if (isFrozen)
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

        TrackStuckState();

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
                UpdatePath();

                if (!FollowPath())
                {
                    HandleBlockedState();
                }
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

        if (isFrozen)
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

        if (distanceToPlayer > attackDistance + 0.45f)
        {
            isAttacking = false;
            return;
        }

        bool hasClearShot = HasClearShotToPlayer(directionToPlayer, distanceToPlayer);
        if (distanceToPlayer <= attackDistance && hasClearShot)
        {
            isAttacking = true;
            return;
        }

        if (!hasClearShot)
        {
            isAttacking = false;
        }
    }

    private bool FollowPath()
    {
        if (currentPathIndex >= currentPath.Count)
        {
            rb.velocity = Vector2.zero;
            return false;
        }

        while (currentPathIndex < currentPath.Count && Vector3.Distance(transform.position, currentPath[currentPathIndex]) < waypointReachDistance)
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
        if (IsDirectionBlocked(direction, 0.45f))
        {
            currentPathIndex++;
            rb.velocity = Vector2.zero;
            return false;
        }

        rb.velocity = direction * moveSpeed;

        if (Vector3.Distance(transform.position, targetPos) < waypointReachDistance)
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

        if (nearestIndex < path.Count - 1 && nearestDistance < waypointReachDistance)
        {
            nearestIndex++;
        }

        return nearestIndex;
    }

    private void HandleBlockedState()
    {
        if (HasUsablePathToPlayer())
        {
            return;
        }

        if (TryShootNearbyBreakableWall())
        {
            rb.velocity = Vector2.zero;
            return;
        }

        WanderWhileBlocked();
    }

    private bool TryShootNearbyBreakableWall()
    {
        if (HasUsablePathToPlayer())
        {
            cachedBreakableTarget = null;
            return false;
        }

        if (cachedBreakableTarget == null || !IsBreakableTargetValid(cachedBreakableTarget))
        {
            cachedBreakableTarget = FindNearestBreakableWall();
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

        if (!IsBreakableTargetValid(cachedBreakableTarget))
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
            if (IsDirectionBlocked(direction, dumbObstacleProbeDistance))
            {
                WanderRandomly(true);
                return;
            }

            rb.velocity = direction * moveSpeed;
            RotateTowards(player.position);
            return;
        }

        WanderRandomly(false);
    }

    private void WanderRandomly(bool forceNewDirection)
    {
        dumbMoveTimer -= Time.fixedDeltaTime;
        if (forceNewDirection || dumbMoveTimer <= 0f || IsDirectionBlocked(dumbMoveDirection, dumbObstacleProbeDistance))
        {
            dumbMoveTimer = Random.Range(1.2f, 2.4f);
            dumbMoveDirection = GetUnblockedRandomDirection();
        }

        rb.velocity = dumbMoveDirection * moveSpeed * 0.85f;
        RotateTowards((Vector2)transform.position + dumbMoveDirection);
    }

    private Vector2 GetUnblockedRandomDirection()
    {
        for (int i = 0; i < 8; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.1f)
            {
                direction = Vector2.up;
            }

            if (!IsDirectionBlocked(direction, dumbObstacleProbeDistance))
            {
                return direction;
            }
        }

        return dumbMoveDirection.sqrMagnitude > 0.1f ? -dumbMoveDirection.normalized : Vector2.up;
    }

    private bool IsDirectionBlocked(Vector2 direction, float distance)
    {
        if (direction.sqrMagnitude < 0.01f)
        {
            return false;
        }

        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.22f, direction.normalized, distance, obstacleLayer);
        return hit.collider != null &&
               !hit.collider.CompareTag("Player") &&
               !hit.transform.IsChildOf(transform);
    }

    private bool HasClearShotToPlayer(Vector2 directionToPlayer, float distanceToPlayer)
    {
        if (directionToPlayer.sqrMagnitude < 0.01f)
        {
            return false;
        }

        Vector2 origin = (Vector2)transform.position + directionToPlayer * 0.35f;
        float distance = Mathf.Max(0f, distanceToPlayer - 0.35f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, directionToPlayer, distance, obstacleLayer);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider == null || hitCollider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (hitCollider.CompareTag("Player"))
            {
                return true;
            }

            return false;
        }

        return true;
    }

    private void TrackStuckState()
    {
        if (rb == null || rb.velocity.sqrMagnitude < 0.05f)
        {
            stuckTimer = 0f;
            lastStuckCheckPosition = transform.position;
            return;
        }

        stuckTimer += Time.fixedDeltaTime;
        if (stuckTimer < stuckCheckInterval)
        {
            return;
        }

        float movedDistance = Vector3.Distance(transform.position, lastStuckCheckPosition);
        lastStuckCheckPosition = transform.position;
        stuckTimer = 0f;

        if (movedDistance >= stuckMoveThreshold)
        {
            return;
        }

        if (behaviorType == BotBehaviorType.Smart && currentPathIndex < currentPath.Count - 1)
        {
            currentPathIndex++;
            return;
        }

        if (behaviorType == BotBehaviorType.Dumb)
        {
            dumbMoveDirection = GetUnblockedRandomDirection();
            dumbMoveTimer = Random.Range(0.8f, 1.5f);
        }
    }

    private void WanderWhileBlocked()
    {
        blockedProbeTimer -= Time.fixedDeltaTime;
        if (blockedProbeTimer <= 0f || blockedProbeDirection.sqrMagnitude < 0.01f)
        {
            blockedProbeTimer = Random.Range(0.7f, 1.45f);
            blockedProbeDirection = Random.insideUnitCircle.normalized;
            if (blockedProbeDirection.sqrMagnitude < 0.1f)
            {
                blockedProbeDirection = Vector2.up;
            }
        }

        rb.velocity = blockedProbeDirection * moveSpeed * blockedMoveSpeedMultiplier;
        RotateTowards((Vector2)transform.position + blockedProbeDirection);

        if (Time.time - lastShootTime < shootCooldown * Random.Range(0.85f, 1.15f))
        {
            return;
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, blockedProbeDirection, blockedRandomFireRange, obstacleLayer);
        BreakableWall breakableWall = hit.collider != null ? hit.collider.GetComponent<BreakableWall>() : null;
        if (breakableWall == null)
        {
            return;
        }

        lastShootTime = Time.time;
        Shoot(1, BulletType.Normal);
    }

    private BreakableWall FindNearestBreakableWall()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, breakableSearchRadius, obstacleLayer);
        float closestDistance = float.MaxValue;
        BreakableWall bestTarget = null;
        Vector2Int botGridPosition = MazeGenerator.Instance.WorldToGridPosition(transform.position);

        for (int i = 0; i < colliders.Length; i++)
        {
            BreakableWall breakableWall = colliders[i].GetComponent<BreakableWall>();
            if (breakableWall == null)
            {
                continue;
            }

            float distance = Vector2.Distance(transform.position, breakableWall.transform.position);
            if (distance > breakableAttackRange || distance >= closestDistance)
            {
                continue;
            }

            Vector2Int wallGridPosition = MazeGenerator.Instance.WorldToGridPosition(breakableWall.transform.position);
            if (!MazeGenerator.Instance.IsBreakableAdjacentToSameRegion(wallGridPosition, botGridPosition) ||
                !HasClearLineOfFire(breakableWall))
            {
                continue;
            }

            closestDistance = distance;
            bestTarget = breakableWall;
        }

        return bestTarget;
    }

    private bool IsBreakableTargetValid(BreakableWall breakableWall)
    {
        return breakableWall != null &&
               Vector2.Distance(transform.position, breakableWall.transform.position) <= breakableAttackRange &&
               MazeGenerator.Instance.IsBreakableAdjacentToSameRegion(
                   MazeGenerator.Instance.WorldToGridPosition(breakableWall.transform.position),
                   MazeGenerator.Instance.WorldToGridPosition(transform.position)) &&
               HasClearLineOfFire(breakableWall);
    }

    private bool HasClearLineOfFire(BreakableWall breakableWall)
    {
        if (breakableWall == null)
        {
            return false;
        }

        Vector2 direction = ((Vector2)breakableWall.transform.position - (Vector2)transform.position).normalized;
        float distance = Vector2.Distance(transform.position, breakableWall.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance + 0.1f, obstacleLayer);
        return hit.collider != null && hit.collider.GetComponent<BreakableWall>() == breakableWall;
    }

    private bool HasUsablePathToPlayer()
    {
        return currentPath != null && currentPathIndex < currentPath.Count;
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

        if (freezeTimer != null)
        {
            freezeTimer.Completed -= HandleFreezeCompleted;
            TimerManager.Instance.RemoveTimer(GetFreezeTimerId());
        }
    }

    protected override void OnDeath()
    {
        TankSpawner.Instance?.NotifyBotDestroyed(this);
        base.OnDeath();
    }

    public void ApplyFreeze(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        if (freezeTimer != null)
        {
            freezeTimer.Completed -= HandleFreezeCompleted;
        }

        freezeTimer = TimerManager.Instance.CreateTimer(
            GetFreezeTimerId(),
            duration,
            TimerDirection.CountDown,
            false,
            false,
            false);
        freezeTimer.Completed += HandleFreezeCompleted;
        freezeTimer.Start();

        isFrozen = true;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        SetFrozenVisual(true);
    }

    private void HandleFreezeCompleted(GameTimer timer)
    {
        if (freezeTimer != null)
        {
            freezeTimer.Completed -= HandleFreezeCompleted;
        }

        freezeTimer = null;
        isFrozen = false;
        SetFrozenVisual(false);
    }

    private void SetFrozenVisual(bool frozen)
    {
        if (cachedRenderers == null)
        {
            return;
        }

        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            cachedRenderers[i].color = frozen
                ? new Color(0.58f, 0.88f, 1f, defaultColors[i].a)
                : defaultColors[i];
        }
    }

    private string GetFreezeTimerId()
    {
        return $"freeze-bot-{GetInstanceID()}";
    }
}
