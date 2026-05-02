# Release Acceptance - v1.0 Chapter 1

Status: accepted for submission
Updated: 2026-05-02 15:39 UTC+8

## Baseline

- Branch: `dev`
- Accepted commit: `45e8a8f fix(level3): show exit prompt and completion`
- Unity: `6000.4.2f1`
- Target: Windows 64-bit development build
- Build output:
  - `E:\黑客松\FHQ-Workspace\build\NetworkDemoWin\FutureHeroQuest.exe`
  - `E:\黑客松\FHQ-Workspace\dist\FutureHeroQuest-v1.0-windows-20260502-1530.zip`
- Zip SHA256:
  - `99F930204A7DC76715D35DAAC057DA850E95E5F4015166888C7641233CD4D953`

## Accepted Flow

Two-client acceptance was run with:

- Unity Editor: host / Past player
- Windows exe: client / Future player
- Network: Photon PUN2 room flow through `Launcher`

Accepted path:

1. Launcher connects both clients to the same Photon room.
2. `Level01_Bridge`
   - Both players spawn and see the other player.
   - Past selects the correct green repair interaction.
   - Future bridge state repairs correctly.
   - Level advances to `Level02_Archive`.
3. `Level02_Archive`
   - Future reads archive clue 314.
   - Past places the correct archive file.
   - Future unlocks the exit path.
   - Level advances to `Level03_ClubRoom`.
4. `Level03_ClubRoom`
   - Past selects Shot C / Pocket 3.
   - Future unlocks the final door.
   - Future reaches the final exit.
   - `CHAPTER 1 COMPLETE / Level 03 Club Room` appears.

## Release Fixes Included

- `2b9e888 fix(release): prevent overlapping interactions`
- `584889f fix(release): consume one interaction per frame`
- `a55a2a1 fix(level3): make exit interaction reliable`
- `45e8a8f fix(level3): show exit prompt and completion`

These address the final P0 issues found during acceptance:

- L1 overlapping repair interactions could overwrite `BridgeState=Supported`.
- L3 final exit had no clear interaction prompt or completion feedback.

## Verification

- Unity script recompile: passed.
- Unity Console error log after final build: empty.
- EditMode test runner: no failed tests.
- Windows package rebuilt after final L3 fix.
- Final DLL timestamp in accepted build:
  - `FutureHeroQuest_Data\Managed\Assembly-CSharp.dll`
  - `2026-05-02 15:30:48 +08:00`

## Submission Notes

- Upload the full `NetworkDemoWin` folder as a zip.
- Do not upload Unity `Library/`, `Temp/`, `Logs/`, raw build cache folders, or repository source folders.
- Do not commit or publish Photon AppID screenshots/config files.
- Mention that the demo requires two clients and internet access for Photon.
- Known limitation: this is a hackathon demo with white-box / low-poly visuals and a shared demo room flow.

## Remaining Non-Blocking Items

- Capture final itch.io screenshots from the accepted build.
- Upload the zip to itch.io.
- Promote `dev` to `main` and tag `v1.0-submit` after this accepted state is preserved.
