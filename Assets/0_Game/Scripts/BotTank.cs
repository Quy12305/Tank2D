using UnityEngine;
using System.Collections.Generic;

public class BotTank : TankBase
{
    [Header("AI Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float attackDistance = 5f;
    public LayerMask obstacleLayer;

    [Header("Attack Settings")]
    public float shootCooldown = 0.1f;

    private float lastShootTime = -Mathf.Infinity;

    private DynamicFlowManager flowManager;
    private Transform player;
    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private Rigidbody2D rb;
    private bool isAttacking = false;

    private float pathUpdateTimer = 0f;
    private float pathUpdateInterval = 2f;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        flowManager = FindObjectOfType<DynamicFlowManager>();

        TankSpawner.OnPlayerSpawned += OnPlayerSpawned;
        EventManager.OnPathsUpdated += UpdatePath;

        GameObject foundPlayer = GameObject.FindWithTag("Player");
        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
            UpdatePath();
        }
    }

    private void OnPlayerSpawned(Transform playerTransform)
    {
        player = playerTransform;
        UpdatePath();
    }

    private void UpdatePath()
    {
        if (player == null || flowManager == null) return;

        Vector2Int fromGrid = flowManager.WorldToGridPosition(transform.position);
        List<Vector3> path = flowManager.GetBotPath(fromGrid);

        currentPath = path;
        currentPathIndex = 0;
    }

    private void FixedUpdate()
    {
        if (player == null) return;

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
        if (player == null) return;

        if (isAttacking && Time.time - lastShootTime >= shootCooldown)
        {
            lastShootTime = Time.time;
            Shoot();
        }

        // Debug ray
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Debug.DrawRay(rayStart, directionToPlayer * 100f, Color.red);
    }

    private void CheckPlayerVisibility()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float   distanceToPlayer  = Vector2.Distance(transform.position, player.position);

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer,
            distanceToPlayer,
            obstacleLayer
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
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotationSpeed * Time.deltaTime);
    }

    private void OnDestroy()
    {
        EventManager.OnPathsUpdated -= UpdatePath;
        TankSpawner.OnPlayerSpawned -= OnPlayerSpawned;
    }
}