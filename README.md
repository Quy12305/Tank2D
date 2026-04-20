# Tank2D

## Quick Start

1. Cai dat Unity version phu hop voi project.
2. Clone repository.
3. Mo thu muc project bang Unity Hub.
4. Mo scene `Assets/0_Game/Scenes/SampleScene.unity`.
5. Nhan `Play` de chay game.

## Project Context

Truoc khi lam viec voi codebase, doc file sau:

- [PROJECT_CONTEXT.md](./PROJECT_CONTEXT.md)

File nay da duoc cap nhat theo he thong hien tai cua project, bao gom:
- spawn bot theo nguong
- `Smart`, `Dumb`, `Sentry`
- `NormalBullet` va `LaserBullet` la 2 prefab rieng
- breakable wall generate theo `breakableWallDensity`
- minimap marker-layer setup
- HUD combat chi hien trong mode `TankWarfare`

## Maintenance Note

Khi project co thay doi lon ve gameplay, spawn, map, bot, UI hoac data flow, nen cap nhat:

- `PROJECT_CONTEXT.md`
- `README.md` neu thay doi anh huong entry point hoac cach khoi dong project
