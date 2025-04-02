using System.Collections.Generic;
using UnityEngine;

// Lớp PlayerTank giữ nguyên logic di chuyển ban đầu
public class PlayerTank : TankBase
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public  float              rotateSpeed = 200f;
    // private DynamicFlowManager flowManager;
    private Vector2Int         currentPlayerGridPos;

    protected override void Start()
    {
        base.Start();
        // flowManager          = FindObjectOfType<DynamicFlowManager>();
        // currentPlayerGridPos = flowManager.WorldToGridPosition(transform.position);
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        float moveInput   = Input.GetAxis("Vertical") * moveSpeed;
        float rotateInput = Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime;

        rb.velocity = transform.up * moveInput;
        transform.Rotate(0, 0, -rotateInput);

        // // Cập nhật đường đi nếu player di chuyển sang ô mới
        // Vector2Int newPlayerGridPos = flowManager.WorldToGridPosition(transform.position);
        // if (newPlayerGridPos != currentPlayerGridPos)
        // {
        //     List<Vector2Int> enemyPositions = new List<Vector2Int>();
        //     foreach (var enemy in FindObjectsOfType<BotTank>())
        //         enemyPositions.Add(flowManager.WorldToGridPosition(enemy.transform.position));
        //
        //     flowManager.UpdatePathsIfNeeded(enemyPositions, newPlayerGridPos);
        //     currentPlayerGridPos = newPlayerGridPos;
        // }
    }

    private void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Shoot();
        }
    }
}