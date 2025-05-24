using System.Collections.Generic;
using UnityEngine;

// Lớp PlayerTank giữ nguyên logic di chuyển ban đầu
public class PlayerTank : TankBase
{
    [Header("Movement Settings")] public float              moveSpeed   = 5f;
    public                               float              rotateSpeed = 200f;
    private                              DynamicFlowManager flowManager;
    private                              Vector2Int         currentPlayerGridPos;
    private                              float              lastPathUpdateTime = 0f;
    public                               VariableJoystick   variableJoystick;

    protected override void Start()
    {
        base.Start();
        flowManager          = FindObjectOfType<DynamicFlowManager>();
        currentPlayerGridPos = flowManager.WorldToGridPosition(transform.position);
        variableJoystick     = FindObjectOfType<VariableJoystick>();
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        //Di chuyển bằng mũi tên
        float moveInput   = Input.GetAxis("Vertical") * moveSpeed;
        float rotateInput = Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime;

        rb.velocity = transform.up * moveInput;
        transform.Rotate(0, 0, -rotateInput);

        //Di chuyển bằng joystick
        // Vector2 direction = variableJoystick.Direction;
        //
        // if (direction.magnitude > 0.1f)
        // {
        //     //Xoay tank
        //     float targetAngle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        //     Quaternion targetRotation = Quaternion.Euler(0, 0, -targetAngle);
        //     transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        //
        //     // Di chuyển tank
        //     rb.velocity = transform.up * moveSpeed * direction.magnitude;
        // }
        // else
        // {
        //     // Dừng lại khi không có input
        //     rb.velocity = Vector2.zero;
        // }

        Vector2Int newPlayerGridPos = flowManager.WorldToGridPosition(transform.position);
        if (newPlayerGridPos != currentPlayerGridPos)
        {
            // Thêm delay để tránh update liên tục
            if (Time.time - lastPathUpdateTime > 0.5f)
            {
                List<Vector2Int> enemyPositions = new List<Vector2Int>();
                foreach (var enemy in FindObjectsOfType<BotTank>()) enemyPositions.Add(flowManager.WorldToGridPosition(enemy.transform.position));

                flowManager.UpdatePathsIfNeeded(enemyPositions, newPlayerGridPos);
                currentPlayerGridPos = newPlayerGridPos;
                lastPathUpdateTime   = Time.time;
            }
        }
    }

    private void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Shoot();
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        LevelManager.Instance.OnLose();
    }
}