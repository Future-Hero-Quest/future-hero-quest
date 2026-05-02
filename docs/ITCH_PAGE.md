# Itch.io Page Draft

## Short Description

A two-player time-collaboration puzzle demo: one player is trapped in 1996, the other in 2026. Communicate, change the timeline, and solve three compact rooms together.

## Project Description

Future Hero Quest is a 2.5D online co-op puzzle demo built for a hackathon. One player plays the Past role and the other plays the Future role. Each player sees a different version of the same timeline, and progress depends on sharing information across time.

The demo focuses on event-driven time interaction rather than long-form story. Past and Future do not continuously sync physics; instead, important actions are sent as semantic timeline events through Photon PUN2. This keeps the online experience simple enough for a game jam while still making one player's actions visibly change the other player's world.

## Included Demo Flow

- Launcher: create or join the Photon room.
- Level 01 Bridge: repair feedback across time.
- Level 02 Archive: missing, wrong, and correct archive state feedback.
- Level 03 Club Room: billiards result and final lock feedback.

## How To Play

This is a two-player online build.

1. Player 1 launches the game and clicks Create Room.
2. Player 2 launches the game and clicks Join Room.
3. Player 1 becomes Past. Player 2 becomes Future.
4. Talk to each other and solve each room.

Controls:

- WASD / arrow keys: move
- E: interact
- R: reset current level
- Number keys: send dialogue shortcuts when available

## Requirements

- Windows build
- Internet access for Photon
- Two running clients

## Known Limitations

- This is a hackathon demo, not a final commercial release.
- Photon room flow uses a shared demo room name.
- WebGL and Mac are planned stretch targets; the current submission target is Windows.
- Some visuals are white-box / low-poly by design.

## Credits

- Engine: Unity 6.4.2f1
- Networking: Photon PUN2
- Art and audio: Kenney and OpenGameArt CC0 assets, documented under `Assets/ThirdParty/README.md` and `Assets/ThirdParty/Licenses/`
- Additional tech: OpenFracture and TemporalPhysicsToolkit
- Inspiration: Titanfall 2's "Effect and Cause" and the Haruhi Suzumiya series

## Upload Checklist

- Upload a zip of the full `NetworkDemoWin` folder.
- Include 3-5 screenshots: Launcher, L1 Bridge, L2 Archive, L3 Club Room, and ideally a two-client view.
- Do not upload Unity `Library/`, `Temp/`, `Logs/`, build cache, or Photon AppID.
- Mention that two players and internet access are required.
