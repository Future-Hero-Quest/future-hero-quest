# Handoff: New Content Integration

This handoff is for continuing the new-content work from another computer.

## GitHub State

Repository:

```text
https://github.com/Future-Hero-Quest/future-hero-quest.git
```

Protected accepted release:

```text
tag: v1.0-submit
accepted package: FutureHeroQuest-v1.0-windows-20260502-1530.zip
accepted zip SHA256: 99F930204A7DC76715D35DAAC057DA850E95E5F4015166888C7641233CD4D953
```

Current public `main`:

```text
origin/main: 98daed98669b3174934dd7b8453475b081c2c4a4
subject: docs(main): document dependencies and v12 snapshot
```

New-content branch:

```text
branch: feature/new-content-integration
base: origin/main @ 98daed9
contains:
- a24d839 docs(content): plan new Scene02 integration workspace
- dc87394 docs(content): add new-content handoff package
```

## What Main Already Contains

`origin/main` already includes:

- v1.0 accepted network flow and release documentation.
- Photon setup docs without committing the real Photon AppID.
- First-person local movement with mouse look.
- WASD / arrow movement relative to current view.
- Jump, sprint, crouch, and first/third-person toggle.
- Offline player spawning for direct Editor Play.
- Outline scene chain.
- Scene02 temporal fracture smoke and validation tooling.
- Minimal L1/L2/L3 BGM, fog, ambient, and emission polish.
- Existing ArtKit prefabs under `Assets/Prefabs/Art`.
- Existing third-party lowpoly art/audio under `Assets/ThirdParty`.

## Important Files In This Branch

Read these first:

```text
README.md
docs/DEPENDENCIES.md
docs/PHOTON_SETUP.md
docs/NEW_CONTENT_INTEGRATION_PLAN.md
docs/HANDOFF_NEW_CONTENT_INTEGRATION.md
docs/THREAD_PROMPT_SCENE02_CONSTRUCTION.md
```

The direct plan file is:

```text
docs/NEW_CONTENT_INTEGRATION_PLAN.md
```

The prompt to hand to another Codex thread is:

```text
docs/THREAD_PROMPT_SCENE02_CONSTRUCTION.md
```

## Setup On Another Computer

Recommended clone path:

```powershell
mkdir E:\黑客松
cd E:\黑客松
git clone https://github.com/Future-Hero-Quest/future-hero-quest.git FutureHeroQuest
cd FutureHeroQuest
git fetch origin --prune
git checkout feature/new-content-integration
```

If the branch has not been pushed yet, create it from main and copy this `docs` handoff manually:

```powershell
git checkout -b feature/new-content-integration origin/main
```

Unity:

```text
Unity Editor: 6000.4.2f1
Render Pipeline: Built-in
Target Platform: Windows x86_64
```

After opening Unity, wait for Package Manager to resolve `Packages/manifest.json`.

## Local-Only Large Asset Folder

The new-content source assets are not committed to Git.

Current source folder on this computer:

```text
C:\Users\9950x-1\Downloads\new
```

Shared handoff folder for moving to another computer:

```text
\\Gotham1\共享\黑客松\FHQ-new-content-handoff-20260503
```

Total size:

```text
about 1.22 GB
15 files
```

Copy that folder to the other computer. Recommended destination:

```text
C:\Users\<your-user>\Downloads\new
```

If the destination path differs, update any prompt text that references `C:\Users\9950x-1\Downloads\new`.

Do not commit this folder wholesale. It contains large zip packages and uncertain-license assets.

## Downloads/New Contents

| File | Size | Recommendation |
|---|---:|---|
| `construction-pack-low-poly.zip` | 3.7 MB | Best first import candidate. Small FBX + texture. |
| `construction-site-asset-pack.zip` | 125.3 MB | Good Scene02 construction prop source. Import only selected files. |
| `tunnel.zip` | 69 MB | Good Scene02 tunnel dressing candidate. Nested zip needs inspection. |
| `the-japanese-school-corridor.zip` | 122.4 MB | Future school/corridor candidate. License not found; do not push public main yet. |
| `场景asset.zip` | 432.7 MB | Aggregate/source index with duplicates. Do not import wholesale. |
| `Assets.zip` | 229 MB | Unity asset dump. Cherry-pick only, especially Sci-Fi Tomb props. |
| `Ch36_nonPBR@*.fbx` | 0.4-1.6 MB each | Animation prototype candidates. Do not touch player controller until proven. |
| `construction-equipment-lowpoly-free-3d-models.zip` | 86 MB | Contains `.blend`; convert externally before Unity import. |
| `construction-prop.zip` | 7.2 MB | Contains nested `.rar`; low priority. |
| `MMD4Mecanim_Beta_20200105.zip` | 168.6 MB | Do not use publicly. Readme has strict redistribution restrictions. |

## Recommended First Task

Build:

```text
Scene02 construction tunnel fracture prototype
```

Target scene:

```text
Assets/Scenes/Outline/Scene02_MountainTunnel.unity
```

Use only selected assets from:

```text
construction-pack-low-poly.zip
construction-site-asset-pack.zip
tunnel.zip
```

Suggested import destinations:

```text
Assets/ThirdParty/Art/PrototypeConstruction/
Assets/Prefabs/Art/PrototypeConstruction/
Assets/Materials/PrototypeConstruction/
```

Scene root:

```text
EnvironmentArt/PrototypeConstruction
```

## Boundaries

Do not edit:

```text
Assets/Scripts/Players/PlayerController.cs
Assets/Scripts/Players/PlayerSpawner.cs
Assets/Scripts/SceneFlow/Scene02TemporalFracturePlayback.cs
Assets/Photon/**
ProjectSettings/ProjectSettings.asset
```

Do not commit:

```text
Library/
Temp/
Logs/
build/
dist/
source zip files
.rar
.blend
PhotonServerSettings.asset
```

Do not delete or overwrite the v1.0 accepted package.

## Validation

Before committing:

```powershell
git status -sb
git diff --check
```

Unity:

```text
Recompile scripts: no C# error
FHQ / Scene02 / Validate Temporal Fracture Smoke
```

Editor Play checks:

- Mouse controls view.
- WASD / arrow keys move relative to current view.
- `E Interact` only appears near interactables.
- Scene02 fracture playback remains visible in the main first-person view.
- The right-top dual-view camera remains readable.
- New props do not block the player path or interaction trigger.

## Suggested Commit Shape

For docs-only intake:

```powershell
git add docs/NEW_CONTENT_ASSET_INTAKE.md
git commit -m "docs(content): record new asset intake findings"
```

For a small Scene02 visual slice:

```powershell
git add Assets/Scenes/Outline/Scene02_MountainTunnel.unity Assets/ThirdParty/Art/PrototypeConstruction Assets/Prefabs/Art/PrototypeConstruction Assets/Materials/PrototypeConstruction docs/NEW_CONTENT_ASSET_INTAKE.md
git commit -m "art(content): dress Scene02 construction tunnel prototype"
```

Push only the feature branch unless main-control explicitly asks otherwise:

```powershell
git push -u origin feature/new-content-integration
```
