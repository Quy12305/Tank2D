# Tank2D Project Context

## Muc dich

File nay la tai lieu tong quan de mot session AI moi co the doc nhanh va nam ngay:
- cau truc project
- flow gameplay hien tai
- cac he thong da duoc mo rong
- nhung cho nao can config trong Unity Editor

Khi bat dau mot session moi, nen doc file nay truoc.

## Tong quan project

- Engine: Unity 2D
- Solution: `Tank2D.sln`
- Scene chinh: `Assets/0_Game/Scenes/SampleScene.unity`
- Thu muc code chinh: `Assets/0_Game/Scripts`

Gameplay loop hien tai:
- vao menu
- chon mode
- vao gameplay
- generate map
- spawn player
- spawn bot theo logic cua mode
- win / lose theo level

## Thu muc quan trong

- `Assets/0_Game/Scripts`
  - gameplay scripts
- `Assets/0_Game/Scripts/Manager`
  - manager tong
- `Assets/0_Game/Scripts/Data`
  - `ScriptableObject` va data level/player
- `Assets/0_Game/Prefabs`
  - bullet, tank, wall, booster...
- `Assets/0_Game/Scenes`
  - scene hien tai
- `Assets/0_Game/Data`
  - level data assets, player data assets

## Script chinh

### Core gameplay

- `Assets/0_Game/Scripts/TankBase.cs`
  - base class cho player va bot
  - HP, ban dan, death

- `Assets/0_Game/Scripts/PlayerTank.cs`
  - movement bang joystick
  - nhat booster / gem / coin
  - doi loai dan bang `Q`
  - chi ban khi dang trong `GamePlay`

- `Assets/0_Game/Scripts/BotTank.cs`
  - 3 loai bot:
    - `Smart`
    - `Dumb`
    - `Sentry`
  - `Smart` dung pathfinding va duoi player
  - `Smart` co waypoint tolerance va stuck recovery de giam rung/giat tai cho
  - `Dumb` di chuyen don gian hon, co check obstacle truoc mat va doi huong khi bi ket
  - `Sentry` nam tren wall, chi active sau khi player bat dau di chuyen
  - `Sentry` hien tai ban `Laser`

- `Assets/0_Game/Scripts/Bullet.cs`
  - logic dan
  - moi prefab dan co `BulletType` rieng
  - `Normal`: va cham vat ly thong thuong
  - `Laser`: trigger projectile, xuyen tuong, khong xuyen bot
  - `Freeze`: giong dan thuong, nhung neu trung bot thi dong bang bot 5 giay

- `Assets/0_Game/Scripts/ObjectPool.cs`
  - pool rieng cho `NormalBullet`, `LaserBullet`, `FreezeBullet`

- `Assets/0_Game/Scripts/BreakableWall.cs`
  - wall pha huy duoc
  - hit 1: nut
  - hit 2: vo

- `Assets/0_Game/Scripts/GameplayTypes.cs`
  - enum:
    - `BulletType`
    - `BotBehaviorType`
    - `CellType`

### Map va pathfinding

- `Assets/0_Game/Scripts/MazeGenerator.cs`
  - generate map base voi wall thuong
  - dam bao lien thong bang `EnsureConnectivity()`
  - sau do rai `BreakableWall` tren cac o trong con lai theo `breakableWallDensity`
  - `wallDensity` va `breakableWallDensity` la 2 config doc lap
  - neu o la breakable wall thi instantiate them `pathPrefab` o ben duoi de khi vo khong lo background

- `Assets/0_Game/Scripts/DynamicFlowManager.cs`
  - pathfinding cho bot di dong
  - chi tinh path tren `CellType.Empty`
  - rebuild graph khi breakable wall bi pha

### Spawn va level flow

- `Assets/0_Game/Scripts/Manager/TankSpawner.cs`
  - spawn player
  - spawn `Sentry` tu dau
  - mobile bot spawn dan theo queue
  - giu so mobile bot active theo `maxActiveMobileBots`
  - khi mobile bot song `<= respawnThreshold` thi spawn them de lap slot trong
  - `Sentry` co loc spawn:
    - khong spawn o viền map
    - khong qua gan player
    - khong qua sat nhau

- `Assets/0_Game/Scripts/Manager/LevelManager.cs`
  - doc `LevelData`
  - day data sang `MazeGenerator` va `TankSpawner`

- `Assets/0_Game/Scripts/Level.cs`
  - clear object cu
  - kiem tra win/lose

### UI va minimap

- `Assets/0_Game/Scripts/Manager/UIManager.cs`
  - quan ly cac UI scene
  - HUD combat:
    - `shootButton`
    - `switchAmmoButton`
    - `switchAmmoText`
    - `miniMapController`
  - progression HUD runtime
  - fake loading overlay scene-based
  - daily reward panel scene-based
  - skill selection panel scene-based
  - chi mode `TankWarfare` moi hien button ban, doi dan va minimap
  - text nut doi dan hien tai:
    - `Bullet`
    - `Laser`
    - `Freeze`

- `Assets/0_Game/Scripts/Systems/GameTimer.cs`
  - doi tuong dong ho dung lai duoc
  - ho tro count up / count down
  - start, restart, pause, resume, stop, reset
  - add time, subtract time, set duration, set elapsed

- `Assets/0_Game/Scripts/Systems/TimerManager.cs`
  - manager update timer theo `Time.deltaTime` hoac `unscaledDeltaTime`
  - bot freeze dang dung he thong nay

- `Assets/0_Game/Scripts/Systems/ProgressionTracker.cs`
  - tracker progression dung lai duoc
  - co title, description, target, current value, reward label
  - co event update / complete / reset

- `Assets/0_Game/Scripts/Systems/ProgressionManager.cs`
  - registry tracker progression runtime

- `Assets/0_Game/Scripts/Systems/DailyRewardManager.cs`
  - daily reward 7 ngay
  - moi 24h unlock them 1 ngay
  - doc data reward tu `DailyRewardConfig`
  - trang thai:
    - lock
    - unlock co the nhan
    - claimed
  - khi claim het 7 ngay va qua 24h tiep theo se reset ve ngay 1 unlock

- `Assets/0_Game/Scripts/Data/DailyRewardConfig.cs`
  - `ScriptableObject` config reward cho 7 ngay
  - dung de chinh `rewardAmount` tung ngay trong Inspector

- `Assets/0_Game/Scripts/Skills/SkillSystemManager.cs`
  - dung progression de mo man chon skill lap lai trong tran
  - du kill threshold se goi truc tiep `UIManager.ShowSkillChoices(...)`
  - neu panel hien thanh cong se pause game va dua ra 3 skill ngau nhien
  - player chi chon 1 skill
  - doc title / description / icon / duration tu `SkillSystemConfig`

- `Assets/0_Game/Scripts/UI/SkillSelectionPanelUI.cs`
  - panel 3 the skill scene-based
  - giu nguyen hierarchy/layout card trong scene
  - dung `SetUpdate(true)` de animation UI van chay khi `Time.timeScale = 0`
  - chon 1 the:
    - 2 the con lai fade out
    - the duoc chon di vao giua man hinh
    - zoom len va fade roi an panel

- `Assets/0_Game/Scripts/MiniMapController.cs`
  - minimap dung:
    - `Camera miniMapCamera`
    - `RawImage miniMapImage`
    - `RenderTexture renderTexture`
    - `LayerMask miniMapLayerMask`
  - script da ho tro camera chi render layer minimap
  - muc tieu la minimap schematic theo marker layer, khong phai render full world art

### Utility va manager khac

- `Assets/0_Game/Scripts/Manager/GameManager.cs`
  - state machine co ban
  - da config ignore collision giua:
    - `PlayerProjectile`
    - `EnemyProjectile`

- `Assets/0_Game/Scripts/HealthBar.cs`
  - HP bar world-space

## Data level quan trong

Script:
- `Assets/0_Game/Scripts/Data/LevelData.cs`

Field quan trong:
- `width`, `height`
- `wallDensity`
- `minWallLength`, `maxWallLength`, `maxWallThickness`
- `smartBotCount`
- `dumbBotCount`
- `sentryBotCount`
- `maxActiveMobileBots`
- `respawnThreshold`
- `breakableWallDensity`

Luu y:
- `wallDensity` la ti le spawn wall thuong
- `breakableWallDensity` la ty le tren cac o trong con lai sau khi map base da generate xong
- `wallDensity` va `breakableWallDensity` doc lap, khong tru lan nhau
- khong con dung `breakableWallCount`
- khong con dung `spawnBatchSize`, `spawnInterval`, `baseWallDensityReduction`

## Thiet ke gameplay hien tai

### Bot spawning

- `Sentry`
  - spawn het tu dau
  - dung tren wall
  - chi active sau khi player bat dau di chuyen
  - ban `Laser`

- `Smart` + `Dumb`
  - spawn dan theo queue
  - khi so bot di dong song `<= respawnThreshold` thi spawn them
  - tong so mobile bot active khong vuot `maxActiveMobileBots`
  - moi lan respawn se lap cac slot trong toi khi dat `maxActiveMobileBots` hoac het queue
  - chi spawn tren `CellType.Empty`
  - uu tien vung rong, nhieu o trong xung quanh
  - co khoang cach an toan toi player khi spawn

### Weapon system

- `NormalBullet.prefab`
  - `BulletType = Normal`
  - damage cao hon
  - va cham thong thuong

- `LaserBullet.prefab`
  - `BulletType = Laser`
  - damage thap hon
  - xuyen wall
  - khong xuyen bot
  - can collider trigger

- `FreezeBullet.prefab`
  - `BulletType = Freeze`
  - di va cham nhu dan thuong
  - khi trung bot thi bot bi freeze 5 giay

Player doi dan bang:
- phim `Q`
- button HUD trong `TankWarfare`
- thu tu vong tron:
  - `Normal`
  - `Laser`
  - `Freeze`

### Progression

- da co he thong progression dung lai duoc
- da duoc noi vao he thong skill trong `TankWarfare`
  - tieu diet du bot theo threshold se day progression
  - khi day se mo panel 3 skill ngau nhien
  - trong luc panel skill dang mo, game pause de player khong bi bot ban
  - nguoi choi chon 1 skill roi game tiep tuc
- HUD progression la UI scene-based, can tao object va gan reference Inspector

### Skill system

- 4 skill runtime hien tai:
  - `Energy Shield`
    - tao vong shield di theo player
    - chan dan bot trong thoi gian skill
  - `Barrel Upgrade`
    - tang so nong len them 1
    - toi da 3
    - het thoi gian se giam ve muc truoc do
  - `Side Turrets`
    - 2 turret di theo player o 2 ben
    - co radius tim muc tieu va tu ban
  - `Orbit Blades`
    - 3 blade quay quanh player
    - bot cham vao se bi tru mau
- cac skill deu co thoi gian
- `SkillSystemConfig.asset`
  - config `killsPerOffer`
  - moi skill co `skillType`, `title`, `description`, `icon`, `duration`
- chon trung:
  - shield / turret / blade se refresh duration
  - barrel se tang tiep toi da 3
- `BoosterShoot` khong con duoc spawn ngau nhien tren map nua
- panel skill selection la UI scene-based, khong con build runtime

### Fake loading

- khi vao app / nhan Play trong Unity
  - hien loading overlay scene-based
  - loading chay theo thoi gian tong config duoc
  - progress fake co random checkpoint / pause / fast-slow
  - xong moi vao `Home`
- `Play`, `Replay`, `Next`
  - khong con chen loading nua
  - vao luong gameplay thang

### Daily reward

- button nam trong `Home`
- panel popup scene-based co 7 button ngay
- moi o co:
  - reward amount
  - lock state
  - unlock claimable state
  - claimed state
- reward amount lay tu `DailyRewardConfig`
- du lieu duoc save vao `gamedata.json`

### Breakable wall

- base map duoc tao truoc de lien thong
- sau do `BreakableWall` duoc dat ngau nhien tren cac o trong con lai
- hit 1: nut
- hit 2: vo
- khi vo se update map/pathfinding
- ben duoi breakable wall co san `pathPrefab`

### Minimap

Muc tieu thiet ke hien tai:
- minimap schematic
- background mau rieng
- object hien qua marker tren layer `MiniMap`
- khong render full art cua map lon

De dat duoc dieu nay, can setup layer/prefab trong Unity Editor.

## Config can lam trong Unity Editor

### Bắt buoc

1. `ObjectPool`
- gan:
  - `normalBulletPrefab`
  - `laserBulletPrefab`
  - `freezeBulletPrefab`

2. `UIManager`
- gan:
  - `shootButton`
  - `switchAmmoButton`
  - `switchAmmoText`
  - `miniMapController`
  - `loadingOverlay`
  - `dailyRewardPanel`
  - `progressionHud`
  - `skillSelectionPanel`
  - `Canvas_Skill` nen co `GraphicRaycaster`
  - card skill nen nam trong parent/layout group cua panel va gan vao array `cards`

3. `MazeGenerator`
- gan:
  - `wallPrefab`
  - `breakableWallPrefab`
  - `pathPrefab`

4. `TankSpawner`
- gan:
  - `enemyTankPrefab`
  - `sentryTankPrefab`
  - `playerTankPrefab`
  - `healthBarPrefab`

### Khong bat buoc nhung nen kiem tra

- `FreezeBullet.prefab`
  - mo prefab va check collider / sprite neu muon doi visual
- `DailyRewardManager`
  - gan `DailyRewardConfig`
- `SkillSystemManager`
  - gan `SkillSystemConfig`
- UI loading / daily reward / progression / skill selection
  - deu la object scene-based
  - can tu tao UI va gan reference Inspector

### Minimap setup

1. Tao layer moi: `MiniMap`
2. `MiniMapController`
- gan:
  - `miniMapCamera`
  - `miniMapImage`
  - `renderTexture`
- `miniMapLayerMask` chi chon `MiniMap`

3. Tao marker con cho cac prefab can hien tren minimap:
- player
- smart/dumb bot
- sentry
- wall / breakable wall / path neu muon thay layout map

Moi marker:
- `SpriteRenderer`
- `Layer = MiniMap`
- mau rieng theo loai

## Thu tu nen doc code khi vao session moi

1. `PROJECT_CONTEXT.md`
2. `Assets/0_Game/Scripts/Data/LevelData.cs`
3. `Assets/0_Game/Scripts/Manager/LevelManager.cs`
4. `Assets/0_Game/Scripts/MazeGenerator.cs`
5. `Assets/0_Game/Scripts/Manager/TankSpawner.cs`
6. `Assets/0_Game/Scripts/PlayerTank.cs`
7. `Assets/0_Game/Scripts/BotTank.cs`
8. `Assets/0_Game/Scripts/Bullet.cs`
9. `Assets/0_Game/Scripts/ObjectPool.cs`
10. `Assets/0_Game/Scripts/DynamicFlowManager.cs`
11. `Assets/0_Game/Scripts/Manager/UIManager.cs`
12. `Assets/0_Game/Scripts/MiniMapController.cs`

## Khi can sua gi

### Them bot moi
- xem `GameplayTypes.cs`
- sua `BotTank.Configure(...)`
- neu can spawn theo level, sua `LevelData` va `TankSpawner`

### Them loai dan moi
- them prefab dan moi
- cap nhat `BulletType`
- cap nhat `ObjectPool`
- cap nhat `TankBase.Shoot(...)`
 - neu dan co status effect, uu tien noi vao `Bullet.cs`

### Sua pacing tran dau
- uu tien sua:
  - `LevelData`
  - `TankSpawner`

### Sua map generation
- sua `MazeGenerator`
- neu co anh huong pathfinding, sua them `DynamicFlowManager`

## Ghi chu bao tri

Moi khi thay doi lon lien quan den:
- bot archetype
- bullet prefab / weapon flow
- map generation
- breakable wall
- spawn heuristics / pathfinding 4 huong
- minimap setup
- HUD / mode-specific UI
- timer / progression / daily reward

thi nen cap nhat lai file nay.
