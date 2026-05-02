using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

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
    private                              BulletType         currentBulletType = BulletType.Normal;
    private                              float              baseShootCooldown;
    private                              Sprite             supportSkillSprite;
    private readonly                     List<GameTimer>    barrelUpgradeTimers = new List<GameTimer>();
    private                              int                temporaryBarrelUpgradeStacks;
    private                              GameTimer          shieldTimer;
    private                              GameTimer          sideTurretTimer;
    private                              GameTimer          orbitBladeTimer;
    private                              GameObject         shieldVisual;
    private                              GameObject         sideTurretRoot;
    private                              GameObject         orbitBladeRoot;
    public                               int                Gem => this.gem;
    public                               Sprite             SupportSkillSprite => supportSkillSprite;
    public                               int                CurrentBarrelLevel => rayShootCount;
    public                               bool               HasActiveShield => shieldTimer != null && shieldTimer.IsRunning && !shieldTimer.IsCompleted;

    [SerializeField] private List<GameObject> barrel;
    [SerializeField] private List<GameObject> tankCore;

    protected override void Start()
    {
        base.Start();
        flowManager          = FindObjectOfType<DynamicFlowManager>();
        currentPlayerGridPos = flowManager.WorldToGridPosition(transform.position);
        variableJoystick     = FindObjectOfType<VariableJoystick>();
        UIManager.Instance.shootButton.onClick.RemoveAllListeners();
        UIManager.Instance.shootButton.onClick.AddListener(TryShoot);
        UIManager.Instance.BindPlayer(this);
        UIManager.Instance.UpdateAmmoMode(currentBulletType);
        UIManager.Instance.RefreshGameplayHud();
        baseShootCooldown = shootCooldown;
        SetTankData(TankManager.Instance.currentTankIndex, TankManager.Instance.currentSpeed, TankManager.Instance.currentHealth,TankManager.Instance.currentDamage);
        ResolveSupportSprite();
        SkillSystemManager.Instance.BindPlayer(this);
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
                ApplyBarrelVisual();
                ChangeSkin();
                moveSpeed   = 5f;
                boosterTime = 11f;
            }
        }
    }

    public void SetTankData(int index, float speed, float health, float damage)
    {
        for (int i = 0; i < tankCore.Count; i++)
        {
            tankCore[i].SetActive(i == index);
        }
        this.moveSpeed = speed;
        this.maxHealth = health;
        this.damage    = damage;
        supportSkillSprite = ResolveSpriteFromActiveModel();
    }

    private void HandleMovement()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsNotificationOpen())
        {
            rb.velocity = Vector2.zero;
            SoundManager.Instance.OnStopMove();
            return;
        }

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

        // Cập nhật khi người chơi di chuyển sang ô khác
        if (newPlayerGridPos != currentPlayerGridPos)
        {
            // Thêm delay để tránh update liên tục
            if (Time.time - lastPathUpdateTime > 0.5f)
            {
                List<BotTank> enemyBots = new List<BotTank>(Array.FindAll(FindObjectsOfType<BotTank>(), bot => bot != null && bot.IsMobile));
                flowManager.UpdatePathsIfNeeded(enemyBots, newPlayerGridPos);
                currentPlayerGridPos = newPlayerGridPos;
                lastPathUpdateTime   = Time.time;
            }
        }
    }

    private void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.C) && Time.time - lastShootTime >= shootCooldown)
        {
            TryShoot();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ToggleBulletType();
        }
    }

    public void ToggleBulletType()
    {
        BulletType[] bulletOrder =
        {
            BulletType.Normal,
            BulletType.Laser,
            BulletType.Freeze
        };

        int currentIndex = Array.IndexOf(bulletOrder, currentBulletType);
        currentIndex = (currentIndex + 1) % bulletOrder.Length;
        currentBulletType = bulletOrder[currentIndex];
        UIManager.Instance.UpdateAmmoMode(currentBulletType);
    }

    private void TryShoot()
    {
        if (!GameManager.Instance.IsState(GameState.GamePlay))
        {
            return;
        }

        if (Time.time - lastShootTime < shootCooldown)
        {
            return;
        }

        lastShootTime = Time.time;
        Shoot(rayShootCount, currentBulletType);
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
            rayShootCount = Mathf.Min(3, rayShootCount + 1);
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
            UIManager.Instance.UpdateGem(this);
            SoundManager.Instance.OnCoin();

            if (GameManager.Instance.IsState(GameState.GamePlay) && LevelManager.Instance.CurrentLevel.CheckWinModeGem(this.gem))
            {
                GameManager.Instance.ChangeState(GameState.Win);
                DOVirtual.DelayedCall(2f, () =>
                {
                    LevelManager.Instance.OnFinish();
                });
            }
        }
        else if (other.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            Coin.Instance.AddCoin(1);
            SoundManager.Instance.OnCoin();
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        SoundManager.Instance.OnStopMove();
        if (!GameManager.Instance.IsState(GameState.GamePlay)) return;

        GameManager.Instance.ChangeState(GameState.Lose);
        DOVirtual.DelayedCall(2f, () =>
        {
            LevelManager.Instance.OnLose();
        });
    }

    public void ApplyRapidFireSkill(float cooldownMultiplier = 0.75f)
    {
        shootCooldown = Mathf.Max(0.08f, baseShootCooldown * cooldownMultiplier);
    }

    public void ResetProgressionRewards()
    {
        shootCooldown = baseShootCooldown;
    }

    public bool TryBlockEnemyProjectile()
    {
        return HasActiveShield;
    }

    public void ApplyTimedSkill(SkillType skillType, float duration)
    {
        switch (skillType)
        {
            case SkillType.Shield:
                ActivateShield(duration);
                break;

            case SkillType.BarrelUpgrade:
                ActivateBarrelUpgrade(duration);
                break;

            case SkillType.SideTurrets:
                ActivateSideTurrets(duration);
                break;

            case SkillType.OrbitBlades:
                ActivateOrbitBlades(duration);
                break;
        }
    }

    private void ActivateShield(float duration)
    {
        RestartSkillTimer(ref shieldTimer, "skill-shield", duration, DeactivateShield);

        if (shieldVisual == null)
        {
            shieldVisual = CreateSkillVisual("ShieldVisual", new Color(0.24f, 0.84f, 1f, 0.36f), 1.55f);
            shieldVisual.AddComponent<PlayerShieldSkillVisual>();
        }

        shieldVisual.SetActive(true);
    }

    private void DeactivateShield(GameTimer timer)
    {
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }

        shieldTimer = null;
    }

    private void ActivateBarrelUpgrade(float duration)
    {
        if (rayShootCount >= 3)
        {
            return;
        }

        temporaryBarrelUpgradeStacks++;
        rayShootCount = Mathf.Min(3, rayShootCount + 1);
        ChangeSkin();

        GameTimer timer = TimerManager.Instance.CreateTimer(
            $"skill-barrel-{GetInstanceID()}-{Time.frameCount}-{barrelUpgradeTimers.Count}",
            duration,
            TimerDirection.CountDown,
            false,
            false,
            false);
        timer.Completed += HandleBarrelUpgradeExpired;
        timer.Start();
        barrelUpgradeTimers.Add(timer);
    }

    private void HandleBarrelUpgradeExpired(GameTimer timer)
    {
        if (timer != null)
        {
            timer.Completed -= HandleBarrelUpgradeExpired;
            TimerManager.Instance.RemoveTimer(timer.Id);
        }

        barrelUpgradeTimers.Remove(timer);
        temporaryBarrelUpgradeStacks = Mathf.Max(0, temporaryBarrelUpgradeStacks - 1);
        ApplyBarrelVisual();
    }

    private void ActivateSideTurrets(float duration)
    {
        RestartSkillTimer(ref sideTurretTimer, "skill-side-turrets", duration, DeactivateSideTurrets);

        if (sideTurretRoot == null)
        {
            sideTurretRoot = new GameObject("SideTurrets");
            sideTurretRoot.transform.SetParent(transform, false);

            CreateSupportTurret(sideTurretRoot.transform, new Vector3(-1.2f, 0f, 0f));
            CreateSupportTurret(sideTurretRoot.transform, new Vector3(1.2f, 0f, 0f));
        }

        sideTurretRoot.SetActive(true);
    }

    private void DeactivateSideTurrets(GameTimer timer)
    {
        if (sideTurretRoot != null)
        {
            sideTurretRoot.SetActive(false);
        }

        sideTurretTimer = null;
    }

    private void ActivateOrbitBlades(float duration)
    {
        RestartSkillTimer(ref orbitBladeTimer, "skill-orbit-blades", duration, DeactivateOrbitBlades);

        if (orbitBladeRoot == null)
        {
            orbitBladeRoot = new GameObject("OrbitBlades");
            PlayerOrbitBladeSkill orbitSkill = orbitBladeRoot.AddComponent<PlayerOrbitBladeSkill>();
            orbitSkill.Initialize(this);
        }

        orbitBladeRoot.SetActive(true);
    }

    private void DeactivateOrbitBlades(GameTimer timer)
    {
        if (orbitBladeRoot != null)
        {
            orbitBladeRoot.SetActive(false);
        }

        orbitBladeTimer = null;
    }

    private void RestartSkillTimer(ref GameTimer timer, string skillKey, float duration, Action<GameTimer> onComplete)
    {
        if (timer != null)
        {
            timer.Completed -= onComplete;
            TimerManager.Instance.RemoveTimer(timer.Id);
        }

        timer = TimerManager.Instance.CreateTimer(
            $"{skillKey}-{GetInstanceID()}",
            duration,
            TimerDirection.CountDown,
            false,
            false,
            false);
        timer.Completed += onComplete;
        timer.Start();
    }

    private void CreateSupportTurret(Transform parent, Vector3 offset)
    {
        GameObject turretObject = new GameObject($"SupportTurret_{offset.x}");
        PlayerSupportTurretSkill turretSkill = turretObject.AddComponent<PlayerSupportTurretSkill>();
        turretSkill.Initialize(this, offset);
        turretObject.transform.SetParent(parent, false);
    }

    private GameObject CreateSkillVisual(string objectName, Color color, float radius)
    {
        GameObject visual = new GameObject(objectName);
        visual.transform.SetParent(transform, false);

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = SupportSkillSprite;
        renderer.color = color;
        visual.transform.localScale = Vector3.one * radius;
        return visual;
    }

    private void ApplyBarrelVisual()
    {
        int baseBarrelLevel = isBoosterActive ? Mathf.Max(1, rayShootCount) : 1;
        rayShootCount = Mathf.Clamp(baseBarrelLevel + temporaryBarrelUpgradeStacks, 1, 3);
        ChangeSkin();
    }

    private void ResolveSupportSprite()
    {
        supportSkillSprite = ResolveSpriteFromActiveModel();

        if (supportSkillSprite == null)
        {
            SpriteRenderer spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                supportSkillSprite = spriteRenderer.sprite;
            }
        }
    }

    private Sprite ResolveSpriteFromActiveModel()
    {
        for (int i = 0; i < barrel.Count; i++)
        {
            SpriteRenderer spriteRenderer = barrel[i] != null ? barrel[i].GetComponentInChildren<SpriteRenderer>() : null;
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                return spriteRenderer.sprite;
            }
        }

        for (int i = 0; i < tankCore.Count; i++)
        {
            if (!tankCore[i].activeInHierarchy)
            {
                continue;
            }

            SpriteRenderer spriteRenderer = tankCore[i].GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                return spriteRenderer.sprite;
            }
        }

        return null;
    }

    private void OnDestroy()
    {
        CleanupTimer(ref shieldTimer, DeactivateShield);
        CleanupTimer(ref sideTurretTimer, DeactivateSideTurrets);
        CleanupTimer(ref orbitBladeTimer, DeactivateOrbitBlades);

        for (int i = 0; i < barrelUpgradeTimers.Count; i++)
        {
            if (barrelUpgradeTimers[i] == null)
            {
                continue;
            }

            barrelUpgradeTimers[i].Completed -= HandleBarrelUpgradeExpired;
            TimerManager.Instance.RemoveTimer(barrelUpgradeTimers[i].Id);
        }

        barrelUpgradeTimers.Clear();
    }

    private void CleanupTimer(ref GameTimer timer, Action<GameTimer> onComplete)
    {
        if (timer == null)
        {
            return;
        }

        timer.Completed -= onComplete;
        TimerManager.Instance.RemoveTimer(timer.Id);
        timer = null;
    }
}
