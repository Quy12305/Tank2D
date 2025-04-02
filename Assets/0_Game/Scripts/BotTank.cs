using UnityEngine;
using System.Collections.Generic;

public class BotTank : TankBase
{
    [Header("AI Settings")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;
    public float attackDistance = 5f;
    public LayerMask obstacleLayer;

    // private DynamicFlowManager flowManager;
    private Transform player;
    private List<Vector3> currentPath = new List<Vector3>();
    private int currentPathIndex = 0;
    private Rigidbody2D rb;
    private bool isAttacking = false;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        // flowManager = FindObjectOfType<DynamicFlowManager>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // EventManager.OnPathsUpdated += UpdatePath;
        // UpdatePath(); // Cập nhật đường đi ngay khi spawn
    }

    // private void UpdatePath()
    // {
    //     if (isAttacking) return;
    //     currentPath = flowManager.GetBotPath(flowManager.WorldToGridPosition(transform.position));
    //     currentPathIndex = 0;
    // }
    //
    // private void FixedUpdate()
    // {
    //     if (isAttacking)
    //     {
    //         rb.velocity = Vector2.zero;
    //         RotateTowardsPlayer();
    //         return;
    //     }
    //     FollowPath();
    // }
    //
    // private void Update()
    // {
    //     CheckPlayerVisibility();
    //     if (isAttacking) Shoot();
    // }
    //
    // private void CheckPlayerVisibility()
    // {
    //     Vector2 directionToPlayer = (player.position - transform.position).normalized;
    //     float distanceToPlayer = Vector2.Distance(transform.position, player.position);
    //     RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
    //
    //     if (isAttacking)
    //     {
    //         if (hit.collider == null || !hit.collider.CompareTag("Player") || distanceToPlayer > attackDistance)
    //         {
    //             isAttacking = false;
    //             UpdatePath();
    //         }
    //     }
    //     else
    //     {
    //         if (hit.collider != null && hit.collider.CompareTag("Player") && distanceToPlayer <= attackDistance)
    //             isAttacking = true;
    //     }
    // }
    //
    // private void FollowPath()
    // {
    //     if (currentPath == null || currentPathIndex >= currentPath.Count) return;
    //
    //     Vector3 targetPos = currentPath[currentPathIndex];
    //     Vector2 direction = (targetPos - transform.position).normalized;
    //     rb.velocity = direction * moveSpeed;
    //
    //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
    //     transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotationSpeed * Time.deltaTime);
    //
    //     if (Vector3.Distance(transform.position, targetPos) < 0.1f)
    //         currentPathIndex++;
    // }
    //
    // private void RotateTowardsPlayer()
    // {
    //     Vector2 direction = (player.position - transform.position).normalized;
    //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
    //     transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.AngleAxis(angle, Vector3.forward), rotationSpeed * Time.deltaTime);
    // }
    //
    // private void OnDestroy() =>
    //     EventManager.OnPathsUpdated -= UpdatePath;
}