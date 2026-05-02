# Release Acceptance Checklist

Updated: 2026-05-02 11:20 UTC+8

## Baseline

- `origin/dev`: `c4d742d merge: integrate art audio rescue pass`
- `origin/main`: `07f2910 docs(workflow): add thread handoff prompts`
- Unity: `6.4.2f1`
- Target submission: Windows build on itch.io
- DDL: `2026-05-03 19:00 UTC+8`

Do not promote `dev` to `main` until Editor + exe acceptance passes.

## Preflight

- Confirm no other thread is using the main Unity worktree before switching branches.
- Fast-forward the main Unity worktree to `origin/dev`; do not force-push.
- Confirm Photon AppID remains local and untracked.
- Confirm Build Settings contains only:
  - `Assets/Scenes/Launcher.unity`
  - `Assets/Scenes/Level01_Bridge.unity`
  - `Assets/Scenes/Level02_Archive.unity`
  - `Assets/Scenes/Level03_ClubRoom.unity`
- Build from the main Unity worktree for the final standard package path:
  - `E:\黑客松\FHQ-Workspace\build\NetworkDemoWin\FutureHeroQuest.exe`

## Two-Client Acceptance

Use two clients:

- Unity Editor: open `Launcher`, press Play, click Create Room. This client becomes Past.
- Windows exe: launch `FutureHeroQuest.exe`, click Join Room. This client becomes Future.

Controls:

- Move: WASD or arrow keys.
- Interact: `E` near prompt objects.
- Reset current level: `R`.
- Dialogue quick buttons: number keys when dialogue options are present.

Acceptance path:

- Launcher: both clients connect to Photon and load `Level01_Bridge`.
- L1 Bridge: both clients spawn, see each other moving, solve bridge repair feedback, then advance to `Level02_Archive`.
- L2 Archive: verify missing/wrong/correct archive feedback, place Archive 314, unlock the future door, then advance to `Level03_ClubRoom`.
- L3 ClubRoom: verify billiards result feedback, align/unlock the final door, and reach `L3_Exit`.
- Whole run: no red Console errors, no scene desync, no wrong-role interaction.

Stop and report if either client disconnects, scenes desync, role-only interactions work for the wrong player, or a red Console error appears.

## Release Gate

- Unity Console red errors: none.
- Windows build: launches on a clean run.
- Photon: both clients can join the same room.
- Gameplay: L1 -> L2 -> L3 path is completable.
- Zip contains the full `NetworkDemoWin` folder.
- Zip does not contain `Library/`, `Temp/`, `Logs/`, raw build cache, or Photon AppID.
- `dev` can be promoted to `main` only after the accepted build is reproducible.
