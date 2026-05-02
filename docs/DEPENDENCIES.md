# Dependencies and Local Setup

This document records what a teammate needs to install after cloning `main`.

## Required

| Item | Version / Source | Notes |
|---|---|---|
| Unity Editor | `6000.4.2f1` | Use Unity Hub or the local editor path if already installed. |
| Render Pipeline | Built-in | Do not migrate this project to URP/HDRP for the current branch. |
| Target Platform | Windows x86_64 | Install Windows Build Support if you need to make a local `.exe`. |
| Photon PUN2 | Embedded under `Assets/Photon` | The real AppID is local-only and must not be committed. |

After opening the project, Unity Package Manager resolves the dependencies from `Packages/manifest.json` and `Packages/packages-lock.json`.

## Unity Package Manager

Direct package dependencies:

| Package | Version / Source | Purpose |
|---|---|---|
| `com.dgreenheck.openfracture` | Git: `https://github.com/dgreenheck/OpenFracture.git` | Fracture tooling used by the temporal fracture prototype. |
| `com.gamelovers.mcp-unity` | Git: `https://github.com/CoderGamester/mcp-unity.git` | Editor automation helper for local Codex/Unity workflows. |
| `com.unity.cinemachine` | `2.10.7` | Camera helpers. |
| `com.unity.ugui` | `2.0.0` | Unity UI. |
| `com.unity.multiplayer.center` | `1.0.1` | Unity multiplayer package metadata/tooling. |

Transitive packages currently resolved by `packages-lock.json` include:

- `com.unity.nuget.newtonsoft-json` `3.2.2`
- `com.unity.editorcoroutines` `1.0.1`
- `com.unity.test-framework` `1.6.0`
- `com.unity.testtools.codecoverage` `1.3.0`
- `com.unity.settings-manager` `2.1.1`

## Photon Setup

`Assets/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset` is intentionally ignored by Git.

To run online co-op locally:

1. Open the project in Unity.
2. Open `Window / Photon Unity Networking / PUN Wizard`.
3. Enter the team-provided PUN Realtime AppID.
4. Run `FHQ / Check Photon Setup`.

See `docs/PHOTON_SETUP.md` for the full checklist.

## Current Main Snapshot

The accepted public package remains the v1.0 Windows zip recorded in `docs/RELEASE_ACCEPTANCE.md`.

The current `main` progress snapshot also includes v1.2 work-in-progress:

- First-person local controller with mouse look.
- WASD / arrow movement relative to the current view.
- Jump, sprint, crouch, first/third-person toggle.
- Editor direct-play offline player spawning.
- Outline scenes and Scene02 temporal fracture smoke.
- Lightweight generated fracture meshes and playback scripts.
- Minimal L1/L2/L3 BGM, fog, ambient, and emission polish.

## Large Assets

No Git-tracked file in this snapshot is over GitHub's 100 MB file limit. Git LFS is not required for the current committed project state.

The large 1 GB+ fracture acceleration import is not present as a Git-tracked source asset in this snapshot. If that source asset needs to be shared, use one of these routes instead of committing it directly to `main`:

- GitHub Release asset.
- Shared cloud drive.
- Git LFS, after the team agrees to enable LFS for the repository.

Keep Unity `Library/`, `Temp/`, `Logs/`, `build/`, `dist/`, and local exported packages out of Git.
