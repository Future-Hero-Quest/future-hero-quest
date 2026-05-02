# Third-Party Assets

This directory contains low-risk third-party art and audio assets for Future Hero Quest.
Assets are copied under `Assets/ThirdParty` so they can be imported by Unity without changing existing scenes.

## Import Status

| Resource | Use | Source | License | Imported to Unity |
| --- | --- | --- | --- | --- |
| Kenney Building Kit | Low-poly walls, floors, doors, bridge/corridor blockout pieces | https://kenney.nl/assets/building-kit | CC0 | Yes, FBX only |
| Kenney Furniture Kit | Bookshelves, desks, cabinets, room props | https://kenney.nl/assets/furniture-kit | CC0 | Yes, FBX only |
| Kenney Nature Kit | Plants, rocks, courtyard dressing | https://kenney.nl/assets/nature-kit | CC0 | Yes, FBX only |
| OpenGameArt Billiards Pack | Pool tables, balls, racks, cue sticks | https://opengameart.org/content/billiards-pack | CC0 | Yes, OBJ/MTL only |
| Kenney Interface Sounds | Button clicks, confirmations, UI feedback, key pickup placeholders | https://kenney.nl/assets/interface-sounds | CC0 | Yes, OGG |
| Kenney Digital Audio | Futuristic pings, success cues, time-link feedback | https://kenney.nl/assets/digital-audio | CC0 | Yes, OGG |
| Kenney Sci-fi Sounds | Door open/close, computers, force fields, energy/bridge cues | https://kenney.nl/assets/sci-fi-sounds | CC0 | Yes, OGG |
| Quantum Stone | Quiet mysterious loop for menu / connection screen | https://opengameart.org/content/quantum-stone | CC0 | Yes, OGG |
| Insistent | Dark quiet loop for level background music | https://opengameart.org/content/insistent-background-loop | CC0 | Yes, OGG |
| Scifi City Ambient Loop | Futuristic ambient bed for connection / future-side UI | https://opengameart.org/content/scifi-city-ambient-loop | CC0 | Yes, OGG |
| 75 CC0 Breaking / Falling / Hit SFX | Bridge stress, impact, glass, rubble, failure feedback | https://opengameart.org/content/75-cc0-breaking-falling-hit-sfx | CC0 | Yes, OGG |
| Door Open SFX | Old room / archive door open sound | https://opengameart.org/content/door-open-sfx | CC0 | Yes, WAV |
| Opening and Closing a Map Sounds | Paper, archive, document interaction | https://opengameart.org/content/opening-and-closing-a-map-sounds | CC0 | Yes, WAV |

## Level Fit

| Level | Directly Useful Assets |
| --- | --- |
| Level 1: Broken Bridge Corridor | Kenney Building Kit, Kenney Nature Kit, Kenney Sci-fi Sounds, 75 CC0 Breaking / Falling / Hit SFX |
| Level 2: Archive Room | Kenney Furniture Kit, Door Open SFX, Opening and Closing a Map Sounds, Kenney Interface Sounds |
| Level 3: Old Club Room | OpenGameArt Billiards Pack, Kenney Furniture Kit, 75 CC0 Breaking / Falling / Hit SFX, Kenney Sci-fi Sounds |

## Rescue Pass Usage Map

No new third-party source packages were added for the art/audio rescue pass. The new project prefabs under `Assets/Prefabs/Art` and `Assets/Prefabs/Audio` only reference the packages already listed above.

| New Project Asset | Third-Party Sources Used | License Files |
| --- | --- | --- |
| `Assets/Prefabs/Art/FHQ_L1_Bridge_ArtKit.prefab` | Kenney Nature Kit bridge, rock, grass, log, plank models | `Licenses/Kenney_Nature_Kit_CC0.md` |
| `Assets/Prefabs/Art/FHQ_L2_Archive_ArtKit.prefab` | Kenney Furniture Kit shelves, desk, books, boxes, terminal, doorway models | `Licenses/Kenney_Furniture_Kit_CC0.md` |
| `Assets/Prefabs/Art/FHQ_L3_ClubRoom_ArtKit.prefab` | OpenGameArt Billiards Pack table/balls/cue models, Kenney Furniture Kit furniture | `Licenses/OpenGameArt_Billiards_Pack_CC0.md`, `Licenses/Kenney_Furniture_Kit_CC0.md` |
| `Assets/Prefabs/Audio/FHQ_Global_AmbientLoop.prefab` | Quantum Stone, Scifi City Ambient Loop | `Licenses/OpenGameArt_Quantum_Stone_CC0.md`, `Licenses/OpenGameArt_Scifi_City_Ambient_Loop_CC0.md` |
| `Assets/Prefabs/Audio/FHQ_L1_Bridge_AudioCues.prefab` | Insistent, Kenney Interface Sounds, Kenney Digital Audio, 75 CC0 Breaking / Falling / Hit SFX | `Licenses/OpenGameArt_Insistent_CC0.md`, `Licenses/Kenney_Interface_Sounds_CC0.md`, `Licenses/Kenney_Digital_Audio_CC0.md`, `Licenses/OpenGameArt_Breaking_Falling_Hit_SFX_CC0.md` |
| `Assets/Prefabs/Audio/FHQ_L2_Archive_AudioCues.prefab` | Quantum Stone, Opening and Closing a Map Sounds, Door Open SFX, Kenney Interface Sounds | `Licenses/OpenGameArt_Quantum_Stone_CC0.md`, `Licenses/OpenGameArt_Map_Paper_Sounds_CC0.md`, `Licenses/OpenGameArt_Door_Open_SFX_CC0.md`, `Licenses/Kenney_Interface_Sounds_CC0.md` |
| `Assets/Prefabs/Audio/FHQ_L3_ClubRoom_AudioCues.prefab` | Scifi City Ambient Loop, 75 CC0 Breaking / Falling / Hit SFX, Kenney Digital Audio, Kenney Sci-fi Sounds | `Licenses/OpenGameArt_Scifi_City_Ambient_Loop_CC0.md`, `Licenses/OpenGameArt_Breaking_Falling_Hit_SFX_CC0.md`, `Licenses/Kenney_Digital_Audio_CC0.md`, `Licenses/Kenney_SciFi_Sounds_CC0.md` |

## Manual Processing Still Needed

These assets are imported as raw source assets only. They are not wired into scenes yet.

Recommended next steps:

1. Create project prefabs for frequently used models under `Assets/Prefabs/`.
2. Tune scale and materials for FBX/OBJ assets in Unity.
3. Assign BGM clips to level/menu AudioSources and set looping/volume.
4. Assign SFX clips to interaction scripts after final event names are stable.
5. Keep new third-party sources documented in `Assets/ThirdParty/Licenses/`.

## Repository Policy

- Do not commit raw downloaded `.zip` archives unless explicitly needed.
- Do not add assets with unclear license terms.
- For CC-BY sources, record author, source URL, and exact license before import.
- Keep Photon AppID and local Unity/MCP settings out of the repository.
