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
  - `Dumb` di chuyen don gian hon
  - `Sentry` nam tren wall, chi active sau khi player bat dau di chuyen
  - `Sentry` hien tai ban `Laser`

- `Assets/0_Game/Scripts/Bullet.cs`
  - logic dan
  - moi prefab dan co `BulletType` rieng
  - `Normal`: va cham vat ly thong thuong
  - `Laser`: trigger projectile, xuyen tuong, khong xuyen bot

- `Assets/0_Game/Scripts/ObjectPool.cs`
  - pool rieng cho `NormalBullet` va `LaserBullet`

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
  - chi mode `TankWarfare` moi hien button ban, doi dan va minimap
  - text nut doi dan hien tai:
    - `Bullet`
    - `Lazer`

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
- `spawnBatchSize`
- `spawnInterval`
- `breakableWallDensity`
- `baseWallDensityReduction`

Luu y:
- `breakableWallDensity` la ty le tren cac o trong con lai sau khi map base da generate xong
- khong con dung `breakableWallCount`

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

Player doi dan bang:
- phim `Q`
- button HUD trong `TankWarfare`

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

2. `UIManager`
- gan:
  - `shootButton`
  - `switchAmmoButton`
  - `switchAmmoText`
  - `miniMapController`

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
- minimap setup
- HUD / mode-specific UI

thi nen cap nhat lai file nay.
