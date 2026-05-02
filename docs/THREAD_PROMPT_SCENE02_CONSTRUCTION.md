# Prompt: Scene02 Construction Tunnel Prototype

You are a Codex thread working on Future Hero Quest new content. Work only in the new-content branch and create a small playable visual prototype for Scene02.

## Repository

```text
repo: https://github.com/Future-Hero-Quest/future-hero-quest.git
branch: feature/new-content-integration
base main commit: 98daed98669b3174934dd7b8453475b081c2c4a4
```

Clone or fetch:

```powershell
git clone https://github.com/Future-Hero-Quest/future-hero-quest.git FutureHeroQuest
cd FutureHeroQuest
git fetch origin --prune
git checkout feature/new-content-integration
```

Read first:

```text
docs/DEPENDENCIES.md
docs/NEW_CONTENT_INTEGRATION_PLAN.md
docs/HANDOFF_NEW_CONTENT_INTEGRATION.md
```

## Local Asset Source

The large asset source folder is local-only and must be copied separately:

```text
C:\Users\<your-user>\Downloads\new
```

If your path differs, adapt the commands.

Prioritize:

- `construction-pack-low-poly.zip`
- `construction-site-asset-pack.zip`
- `tunnel.zip`

Do not use publicly:

- `MMD4Mecanim_Beta_20200105.zip`
- uncertain-license school/corridor/library packages
- source `.zip`, `.rar`, `.blend` files as Git-tracked project files

## Goal

Turn this scene into a more readable first-person construction tunnel fracture prototype:

```text
Assets/Scenes/Outline/Scene02_MountainTunnel.unity
```

The prototype should show:

- tunnel/construction dressing,
- clear visual framing around `TunnelWiringInteractor`,
- visible fracture/falling debris area,
- readable right-top dual-view camera,
- no blocked first-person path.

## Allowed Files

You may edit:

```text
Assets/Scenes/Outline/Scene02_MountainTunnel.unity
docs/NEW_CONTENT_ASSET_INTAKE.md
```

You may add selected imported assets under:

```text
Assets/ThirdParty/Art/PrototypeConstruction/
Assets/Prefabs/Art/PrototypeConstruction/
Assets/Materials/PrototypeConstruction/
```

Keep the import small. Do not import the full 1.22 GB folder.

## Forbidden Files

Do not edit:

```text
Assets/Scripts/Players/PlayerController.cs
Assets/Scripts/Players/PlayerSpawner.cs
Assets/Scripts/SceneFlow/Scene02TemporalFracturePlayback.cs
Assets/Photon/**
ProjectSettings/ProjectSettings.asset
Assets/Scenes/Level01_Bridge.unity
Assets/Scenes/Level02_Archive.unity
Assets/Scenes/Level03_ClubRoom.unity
```

Do not commit:

```text
Library/
Temp/
Logs/
build/
dist/
*.zip
*.rar
*.blend
PhotonServerSettings.asset
```

## Implementation Notes

Use this hierarchy in Scene02:

```text
EnvironmentArt/PrototypeConstruction
```

Prefer static visual props:

- pipes,
- pallets,
- cones,
- planks,
- barriers,
- tunnel frame pieces,
- small construction clutter.

Keep colliders conservative. Visual props should not block:

- player spawn,
- path to `TunnelWiringInteractor`,
- fracture playback view,
- dual-view camera sightline.

## Validation

Run:

```powershell
git status -sb
git diff --check
```

Unity:

- Open with Unity `6000.4.2f1`.
- Recompile scripts: no C# errors.
- Run `FHQ / Scene02 / Validate Temporal Fracture Smoke`.
- Play smoke:
  - mouse look works,
  - WASD moves relative to current view,
  - `E Interact` appears only near interactables,
  - fracture playback is visible,
  - new construction props do not block the player route.

## Commit

If you only document intake:

```powershell
git add docs/NEW_CONTENT_ASSET_INTAKE.md
git commit -m "docs(content): record construction asset intake"
```

If you import lightweight assets and dress Scene02:

```powershell
git add Assets/Scenes/Outline/Scene02_MountainTunnel.unity Assets/ThirdParty/Art/PrototypeConstruction Assets/Prefabs/Art/PrototypeConstruction Assets/Materials/PrototypeConstruction docs/NEW_CONTENT_ASSET_INTAKE.md
git commit -m "art(content): dress Scene02 construction tunnel prototype"
```

Default: push only this feature branch, not `main` or `dev`.

## Report

Include:

1. Commit hash.
2. Imported asset list and source zip names.
3. License/source status.
4. Scene02 changes.
5. Validation result.
6. Whether it is worth turning into a v1.3 preview build.

