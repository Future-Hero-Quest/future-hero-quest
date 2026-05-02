# Future Hero Quest

2.5D 双人合作时空解谜 demo。一个玩家在 1996 年的过去，另一个玩家在 2026 年的未来；两端通过 Photon PUN2 同步离散语义事件，让过去和未来互相改写。

当前目标是黑客松可提交版本，不再扩写关卡范围。最新集成基线在 `origin/dev`。

## 当前状态

- Unity: `6.4.2f1` / `6000.4.2f1`
- Render Pipeline: Built-in
- Networking: Photon PUN2
- Primary build target: Windows
- Current integrated branch: `origin/dev`
- Latest gameplay integration commit: `c4d742d merge: integrate art audio rescue pass`
- Submission DDL: `2026-05-03 19:00 UTC+8`

Implemented demo flow:

1. `Launcher`
2. `Level01_Bridge`
3. `Level02_Archive`
4. `Level03_ClubRoom`

`ProjectSettings/EditorBuildSettings.asset` should contain only those four enabled scenes.

## Playtest

Use two clients:

- Unity Editor: open `Launcher`, press Play, click **Create Room**. This client becomes Past.
- Windows exe: launch `FutureHeroQuest.exe`, click **Join Room**. This client becomes Future.

Controls:

- Move: WASD or arrow keys
- Interact: `E`
- Reset current level: `R`
- Dialogue shortcuts: number keys when options are shown

Expected acceptance path:

- Launcher connects both clients to the same Photon room.
- L1 Bridge advances to L2 Archive.
- L2 Archive advances to L3 ClubRoom.
- L3 ClubRoom completes without red Console errors.

Detailed release checklist: [`docs/RELEASE_ACCEPTANCE.md`](docs/RELEASE_ACCEPTANCE.md).

## Build

Use Unity menu:

```text
FHQ/Build Windows Network Demo
```

The build menu now reads the final scene list and no longer regenerates or rewrites `Launcher.unity` / `Level01_Tree.unity`.

Expected final package path when building from the main Unity worktree:

```text
E:\黑客松\FHQ-Workspace\build\NetworkDemoWin\FutureHeroQuest.exe
```

Zip the whole `NetworkDemoWin` folder for itch.io, not only the `.exe`.

## Important Rules

- Do not commit Photon AppID or `PhotonServerSettings.asset`.
- Do not commit `Library/`, `Temp/`, `Logs/`, or build outputs.
- Do not force-push `main` or `dev`.
- Avoid parallel edits to the same `.unity` scene file.
- Promote `dev` to `main` only after Editor + exe acceptance passes.

## Documentation

- [`docs/PLAN.md`](docs/PLAN.md): original design plan plus current release adjustment.
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md): technical architecture.
- [`docs/RELEASE_ACCEPTANCE.md`](docs/RELEASE_ACCEPTANCE.md): acceptance and release checklist.
- [`docs/ITCH_PAGE.md`](docs/ITCH_PAGE.md): itch.io page copy.
- [`docs/thread-prompts/`](docs/thread-prompts/): handoff prompts for parallel AI threads.

## Credits

- Unity Technologies
- Photon Engine
- Kenney assets and audio, CC0
- OpenGameArt audio assets, CC0
- OpenFracture
- TemporalPhysicsToolkit
- Inspirations: Respawn's *Titanfall 2: Effect and Cause* and Nagaru Tanigawa's *Haruhi Suzumiya* series

Third-party asset details live under `Assets/ThirdParty/README.md` and `Assets/ThirdParty/Licenses/`.
