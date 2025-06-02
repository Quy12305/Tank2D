using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTank : TankBase
{
    [Header("Movement Settings")] public float              moveSpeed       = 3.5f;
    public                               float              rotateSpeed     = 300f;
    private                              int                rayShootCount   = 1;
    private                              float              boosterTime     = 11f;
    private                              bool               isBoosterActive = false;
    private                              DynamicFlowManager flowManager;
    private                              Vector2Int         currentPlayerGridPos;
    private                              float              lastPathUpdateTime = 0f;
    public                               VariableJoystick   variableJoystick;
    private                              int                gem = 0;
    public                               int                Gem => this.gem;

    [SerializeField] private List<GameObject> barrel;

    protected override void Start()
    {
        base.Start();
        flowManager          = FindObjectOfType<DynamicFlowManager>();
        currentPlayerGridPos = flowManager.WorldToGridPosition(transform.position);
        variableJoystick     = FindObjectOfType<VariableJoystick>();
        UIManager.Instance.shootButton.onClick.AddListener(() => Shoot(rayShootCount));
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();

        if (isBoosterActive)
        {
            boosterTime -= Time.deltaTime;
            if (boosterTime <= 0f)
            {
                isBoosterActive = false;
                rayShootCount   = 1;
                ChangeSkin();
                moveSpeed   = 5f;
                boosterTime = 11f;
            }
        }
    }

    private void HandleMovement()
    {
        //Di chuyển bằng mũi tên
        // float moveInput   = Input.GetAxis("Vertical") * moveSpeed;
        // float rotateInput = Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime;
        //
        // rb.velocity = transform.up * moveInput;
        // transform.Rotate(0, 0, -rotateInput);

        //Di chuyển bằng joystick
        Vector2 direction = variableJoystick.Direction;

        if (direction.magnitude > 0.1f)
        {
            //Xoay tank
            float      targetAngle    = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0, 0, -targetAngle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            // Di chuyển tank
            rb.velocity = transform.up * moveSpeed * direction.magnitude;
            SoundManager.Instance.OnMove();
        }
        else
        {
            rb.velocity = Vector2.zero;
            SoundManager.Instance.OnStopMove();
        }

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
        if (Input.GetKeyDown(KeyCode.C) && Time.time - lastShootTime >= shootCooldown)
        {
            lastShootTime = Time.time;
            Shoot(rayShootCount);
        }
    }

    private void ChangeSkin()
    {
        switch (rayShootCount)
        {
            case 1:
            {
                barrel[0].SetActive(true);
                barrel[1].SetActive(false);
                barrel[2].SetActive(false);
                break;
            }

            case 2:
            {
                barrel[0].SetActive(false);
                barrel[1].SetActive(true);
                barrel[2].SetActive(false);
                break;
            }

            case 3:
            {
                barrel[0].SetActive(false);
                barrel[1].SetActive(false);
                barrel[2].SetActive(true);
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BoosterShoot"))
        {
            isBoosterActive = true;
            rayShootCount++;
            ChangeSkin();
            Destroy(other.gameObject);
            SoundManager.Instance.OnBooster();
        }
        else if (other.CompareTag("BoosterSpeed"))
        {
            isBoosterActive =  true;
            moveSpeed       += 2f;
            Destroy(other.gameObject);
            SoundManager.Instance.OnBooster();
        }
        else if (other.CompareTag("BoosterHealth"))
        {
            this.TakeDamage(-25f);
            Destroy(other.gameObject);
            SoundManager.Instance.OnBooster();
        }
        else if (other.CompareTag("Boom"))
        {
            this.TakeDamage(15f);
            Destroy(other.gameObject);
            SoundManager.Instance.OnBoom();
        }
        else if (other.CompareTag("Gem"))
        {
            this.gem++;
            Destroy(other.gameObject);
            UIManager.Instance.UpdateCoin(this);
            SoundManager.Instance.OnCoin();

            if (LevelManager.Instance.CurrentLevel.CheckWinModeGem(this.gem))
            {
                DOVirtual.DelayedCall(2f, () =>
                {
                    LevelManager.Instance.OnLose();
                });
            }
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        SoundManager.Instance.OnStopMove();
        DOVirtual.DelayedCall(2f, () =>
        {
            LevelManager.Instance.OnLose();
        });
    }
}