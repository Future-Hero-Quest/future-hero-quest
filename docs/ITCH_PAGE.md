# Itch.io Page Draft

## Title

Future Hero Quest: 都市怪谈篇

## Tagline

One player is in 1996. One player is in 2026. Solve the same urban legend from opposite ends.

## Short Description

A two-player online time puzzle demo. Communicate across 30 years, change the timeline, and clear three compact rooms in the Urban Legend Chapter.

## Long Description

```text
Future Hero Quest: 都市怪谈篇 is a 2.5D online co-op puzzle demo built for a hackathon.

One player becomes the Past. The other becomes the Future.
You are separated by 30 years, but your worlds are still connected.

Past can repair, place, and trigger events.
Future can inspect what changed, report feedback, and unlock the path forward.

Instead of synchronizing full physics, the game sends semantic timeline events through Photon PUN2:
bridge states, archive states, billiards results, and lock states.
The result is a compact three-room demo about communication, timing, and trusting what the other player can see.
```

## Demo Rooms

| Room | Theme | Co-op Moment |
|---|---|---|
| Echoes on the Broken Bridge / 《断桥回声》 | Broken bridge repair | Future reads the bridge outcome, Past chooses the repair |
| Archive 314 / 《314号档案》 | Missing archive file | Wrong and correct archive states change the Future path |
| The Last Club Room / 《最后的社团室》 | Billiards and final lock | Shot result drives the final unlock |

## How To Play

This build requires two online clients.

1. Player 1 launches the game and clicks **Create Room**.
2. Player 2 launches the game and clicks **Join Room**.
3. Player 1 becomes Past. Player 2 becomes Future.
4. Talk to each other and solve each room.

## Controls

| Input | Action |
|---|---|
| WASD / Arrow keys | Move |
| E | Interact |
| R | Reset current level |
| Number keys | Dialogue shortcuts when available |

## Requirements

- Windows
- Internet access for Photon
- Two running clients
- Voice chat or local conversation recommended

## Known Limitations

- Hackathon demo, not a final commercial release.
- Photon room flow uses a shared demo room name.
- Current submission target is Windows; WebGL/Mac are stretch goals.
- Some visuals are intentionally white-box / low-poly.

## Credits

- Engine: Unity 6.4.2f1
- Networking: Photon PUN2
- Art and audio: Kenney and OpenGameArt CC0 assets
- Additional tech: OpenFracture and TemporalPhysicsToolkit
- Inspiration: *Titanfall 2: Effect and Cause* and the *Haruhi Suzumiya* series

Detailed third-party notices are documented in `Assets/ThirdParty/README.md` and `Assets/ThirdParty/Licenses/`.

## Upload Checklist

- Upload a zip of the full `NetworkDemoWin` folder.
- Include screenshots for Launcher, 《断桥回声》, 《314号档案》, 《最后的社团室》, and ideally a two-client view.
- Do not upload Unity `Library/`, `Temp/`, `Logs/`, build cache, or Photon AppID.
- Mention clearly that two players and internet access are required.
