# New Content Integration Plan

This branch is a clean workspace for turning the existing v1.2 progress into new playable content.

```text
worktree: E:\黑客松\FHQ-Workspace\worktrees\new-content-integration
branch: feature/new-content-integration
base: origin/main @ 98daed9 docs(main): document dependencies and v12 snapshot
```

## Current Project Base

The branch already includes:

- v1.0 accepted network flow and release docs.
- First-person local movement with mouse look.
- WASD / arrow movement relative to current view.
- Jump, sprint, crouch, and first/third-person toggle.
- Offline player spawning for direct Editor Play.
- Outline scene chain.
- Scene02 temporal fracture smoke and validation tooling.
- Minimal L1/L2/L3 BGM, fog, ambient, and emission polish.
- Existing ArtKit prefabs under `Assets/Prefabs/Art`.
- Existing third-party lowpoly art/audio under `Assets/ThirdParty`.

## Downloads/New Asset Intake

Source folder:

```text
C:\Users\9950x-1\Downloads\new
```

Total size is about `1.22 GB`. Do not import it wholesale.

| Asset | Size | Usefulness | Notes |
|---|---:|---|---|
| `construction-pack-low-poly.zip` | 3.7 MB | High | Small FBX + texture. Best first import candidate for Scene02 props. |
| `construction-site-asset-pack.zip` | 125.3 MB | High | Construction props, pipes, pallets, tools. Good for Scene02 visual upgrade. |
| `tunnel.zip` | 69 MB | Medium / High | Useful for Scene02 tunnel dressing; nested source zip needs inspection. |
| `the-japanese-school-corridor.zip` | 122.4 MB | Medium | Useful for future school/corridor content, but license is not present in archive. |
| `场景asset.zip` | 432.7 MB | Medium | Aggregates several scene zips, including duplicates. Use only as a source index. |
| `Assets.zip` | 229 MB | Medium | Unity asset dump with Sci-Fi Tomb, StarterAssets, dice, scripts. Cherry-pick only. |
| `Ch36_nonPBR@*.fbx` | 0.4-1.6 MB each | Medium | Character animation clips: idle, walk, run, carrying, button pushing. |
| `construction-equipment-lowpoly-free-3d-models.zip` | 86 MB | Low | Contains `.blend`; convert externally before Unity import. |
| `construction-prop.zip` | 7.2 MB | Low | Contains nested `.rar`; not a clean Unity import path. |
| `MMD4Mecanim_Beta_20200105.zip` | 168.6 MB | Do not use | Readme has redistribution and public upload restrictions. Do not commit. |

## Recommended First Slice

Build a v1.3 prototype around Scene02:

```text
Scene02 construction tunnel fracture pass
```

Why this is the best first slice:

- It directly uses the existing first-person controls.
- It uses the Scene02 fracture playback already validated in `main`.
- Construction and tunnel assets match the collapsed mountain tunnel theme.
- It can become a visible new-content demo without disturbing the v1.0 three-level flow.

Target scene:

```text
Assets/Scenes/Outline/Scene02_MountainTunnel.unity
```

Suggested content:

- Dress the tunnel with selected construction props.
- Add visual framing around the fracture trigger and falling debris area.
- Keep the right-side dual-view camera readable.
- Keep the first-person route, interact prompt, and fracture trigger unobstructed.
- Do not change Photon or the accepted v1.0 build settings.

## Import Policy

Do not commit source zips from `Downloads\new`.

Allowed in Git after license and smoke checks:

- Selected `.fbx`, `.png`, `.jpg`, `.mat`, `.prefab`, and `.meta` files under a scoped folder.
- A small Unity prefab kit assembled from those selected assets.
- Documentation describing source path and license uncertainty.

Recommended import destination:

```text
Assets/ThirdParty/Art/PrototypeConstruction/
Assets/Prefabs/Art/PrototypeConstruction/
```

For uncertain-license assets, use this staging path first and do not push public main until cleared:

```text
E:\黑客松\FHQ-Workspace\asset-intake\new-content-integration\
```

## Do Not Use Publicly Yet

- `MMD4Mecanim_Beta_20200105.zip`
- Any MMD converted model or motion without author permission.
- Any nested source archive whose license is not known.
- Any single file over 100 MB unless Git LFS is explicitly enabled.

## Candidate Thread Split

### Thread A: Scene02 Construction Dressing

Owns:

- `Assets/Scenes/Outline/Scene02_MountainTunnel.unity`
- selected construction/tunnel prefabs only

Goal:

- Create a playable first-person construction tunnel mood pass around the fracture demo.

### Thread B: Asset Intake and License Notes

Owns:

- `docs/NEW_CONTENT_ASSET_INTAKE.md`
- optional staged import notes

Goal:

- Identify which `Downloads\new` packages can be committed, which must stay local, and which need external sharing.

### Thread C: Character Animation Prototype

Owns:

- a new prototype animator/prefab path only

Goal:

- Test `Ch36_nonPBR@Standing Idle.fbx`, `Walking.fbx`, and `Medium Run.fbx` against the local player visual.

This should not touch `PlayerController.cs` until the visual prototype is proven.

## Validation

Before any push:

```powershell
git status -sb
git diff --check
```

Unity:

- Recompile scripts with no C# error.
- Run `FHQ / Scene02 / Validate Temporal Fracture Smoke`.
- Editor Play Scene02 or Level01 smoke:
  - mouse look works,
  - WASD follows current view,
  - `E Interact` only appears near interactables,
  - fracture playback remains visible,
  - no blocking props on player path.

## Current Recommendation

Proceed with Thread A first, but only import a small selected subset from:

```text
construction-pack-low-poly.zip
construction-site-asset-pack.zip
tunnel.zip
```

Keep school/corridor/library assets as future chapter material until license/source is documented.
