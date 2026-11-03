# SAOSS port plan: C++ (2003–2005) to C# (.NET 10, Windows x64)

## 1. Source selection

| | `original/saoss/src` | `original/saoss2` |
|---|---|---|
| Last source edit | 2005-11-20 (`scene.cpp`), most files Aug 2003 | 2003-04-18 (single snapshot) |
| Extra features | Dedicated-server entry point (`dmenu.cpp`), Main/Menu/Player/Alustuksia fixes | – |
| Lines of game code | ~25,600 | ~25,400 |

**Decision: `original/saoss` is the reference.** `saoss2` is an older snapshot of the same CVS module, and `saoss` contains every later change.
`saoss2` is still useful because it holds the runtime DLLs (`SDL.dll`, `fmod.dll`, ...), which may let us run the original exe to get reference visuals.

Note: the build files are Visual C++ 6 (`.dsp/.dsw`, `vc60.pdb`) plus a Linux `Makefile`. No Watcom-specific code was found.

## 2. Feasibility

**The port is feasible.** Nothing in the codebase blocks it:

- About 25k lines of game code. A lot of it is copy-paste (24 ship stat blocks, 4 copies of the input code, per-ship draw code), so the C# version should end up much smaller.
- There is no assembler and no custom binary file format. Assets are PNG, PCX, WAV, MP3, OGG, and XM.
- Rendering is OpenGL 1.x fixed-function: immediate mode, display lists, GLU quadrics, and one directional light. A small mesh builder that works like the old GL calls can reproduce it closely.
- The simulation is a simple 2D plane at a fixed tick: gravity wells, friction, and circle collisions. It ports almost formula for formula.
- Networking is TCP and host-authoritative, with raw struct `memcpy`. We will replace it with an explicit binary protocol. Compatibility with old clients is not needed.

Main risks: hand-translating the ship geometry (24 ships, about 290 GLU primitive calls and 76 `glBegin` blocks), the 3D intro scene (`lippu.cpp`, 1.8k lines: waving cloth flag, flowers, mushrooms, planar shadows), and very large functions with lots of global state.

The original `Release/saoss.exe` (Nov 2003 build) still runs on Windows 11 with the DLLs from `saoss2`. Reference screenshots are in `docs/reference/`.

## 3. Technology

| Area | Choice | Reason |
|---|---|---|
| Runtime | **.NET 10 (LTS)**, `net10.0` / `net10.0-windows`, x64 | The .NET 10 SDK is installed, and VS2026 supports it |
| Game framework | **MonoGame 3.8.x DesktopGL** | Mature C# game framework (window, input, 3D, sprites, audio). Runs on Windows 10 and 11. Builds with `dotnet build` and VS2026 |
| 3D | MonoGame `BasicEffect` (vertex color + 1 directional light) + procedural meshes | Close match to GL fixed-function lighting with `GL_COLOR_MATERIAL` |
| 2D / text | `SpriteBatch` + bitmap font from `textures/teksti.png` | Keeps the original font look |
| Assets | Load at runtime from files (`Texture2D.FromStream`, `SoundEffect.FromStream`). No MGCB content pipeline | Simpler build. Players can still drop music into `Music/` like in the original |
| Music | **NVorbis** (OGG) + **NLayer** (MP3) streamed into `DynamicSoundEffectInstance`. XM is converted to OGG once | Fully managed. No FMOD license or native DLLs |
| Networking | `System.Net.Sockets` TCP (NoDelay), versioned binary messages via `BinaryWriter`/`BinaryReader` | Same model as the original, but safe. The transport is behind an interface, so UDP can be added later |
| Settings | JSON (`System.Text.Json`) in `%LOCALAPPDATA%\Saoss\settings.json` | Replaces `sets.txt` |
| Tests | MSTest or xUnit for `Saoss.Core` | Locks simulation behavior while refactoring |
| Packaging | `dotnet publish -r win-x64 --self-contained` | Single folder, no install needed |

Rejected options: **Raylib-cs** (closest to immediate mode and plays XM, but native and less idiomatic C#), **Silk.NET/OpenTK** (everything built by hand: shaders, audio decoding, UI), **Unity/Godot** (heavy, and they work against a clean code port).

No company (Vilant client) libraries are used. The solution is fully standalone.

## 4. Target architecture (full refactor)

```
Saoss.slnx
 src/Saoss.Core      net10.0      Simulation only: no graphics/audio/IO. Deterministic 100 Hz tick, seeded RNG.
                                  World, Ship, ShipDefinition (24, data-driven), Projectile, Debris, ExhaustTrail,
                                  Asteroid, Planet, BlackHole, physics, collisions, AI pilot, IGameMode
                                  (Deathmatch, RandomDeathmatch, Domination, CaptureTheFlag, Mushroom), events.
 src/Saoss.Network   net10.0      Protocol messages, serializer, host/client sessions, lobby, chat.
 src/Saoss.Game      net10.0-windows  MonoGame app: screens (menu, setup, lobby, game, results), rendering,
                                  camera, mesh builder, ship models, audio, input mapping, settings.
 tests/Saoss.Tests   net10.0-windows  Unit tests for Core and the testable parts of Game.
```

Principles:
- No global state. A `World` instance owns all entities, and screens own their services.
- The simulation emits events (`ShotFired`, `ShipHit`, `ShipDestroyed`, ...). Rendering and audio react to them, so the simulation stays headless and testable.
- The fixed 100 Hz tick keeps the original per-frame constants (`friction 0.006`, `gravity 0.0004`, ...). Rendering runs separately from the tick.
- Ship stats live in a single table, and each ship model is one small class built once at startup (this replaces the display lists).
- Object pools replace the fixed `malloc` arrays.
- All identifiers, comments, UI text, and asset file names are in English (see glossary).

## 5. Gap analysis

| Feature | Original | Port | Gap / action |
|---|---|---|---|
| Platforms | Win32 (VC6) + Linux | Windows 10/11 x64 only | Drop the `#ifdef` branches |
| Window / resolution | SDL 1.2 `SetVideoMode`, any res, fullscreen flag | Windowed / borderless fullscreen, any res | Widescreen shows a larger part of the playfield: the vertical FOV stays fixed and the horizontal FOV grows with the aspect ratio. No letterboxing |
| 3D rendering | GL 1.x immediate, display lists, GLU sphere/cylinder/disk, glLight | MonoGame meshes + BasicEffect | Write a `MeshBuilder` with GL-like matrix stack and quadrics. Translate the 24 ships carefully |
| Detail levels | `detail` setting 0–2 | Same setting | Map to the quadric tessellation / particle count |
| 256-color mode | `mode256`, `*_256.png` textures | Not needed | **Drop** |
| Text | Bitmap font `teksti.png`, Finnish/English UI | BitmapFont via SpriteBatch, English only | Translate all UI strings |
| Starfield, planet, black hole, asteroids, small figures, trails, debris | Custom GL code | Ported per effect | Straightforward |
| Game modes | Deathmatch, random-ship deathmatch, Flower Domination, Capture The Flower, Eat The Mushroom | Same, as `IGameMode` implementations | – || Intro scene | `lippu.cpp`: 3D title scene with cloth flag, flowers, mushrooms, planar shadows | Same scene in Game | Medium risk. It is the first thing the player sees |
| Menus | Drawn in 3D: perspective text in the bitmap font, rotating arrow models, 4:3 title picture | Same approach (done for the main menu) | – |
| Credits mode | "Start With 400 Credits" variants in the local game menu (ships cost credits) | Same | Not yet analyzed |
| AI | `Player::ailiiku()` | `AiPilot` producing the same input flags as a human | Behavior parity is checked by watching games |
| Local multiplayer | Up to 4 humans on one keyboard + AI up to 16 | Same | Key remapping is a **new** optional feature. Gamepads are **optional** |
| Camera | Follows/zooms to fit players, `cammode` | Same | – |
| Sound effects | FMOD samples, `a_<ship>_<event>.wav` | MonoGame `SoundEffect` | Rename files (e.g. `ship05_fire.wav`) |
| Music | FMOD: MP3, OGG, XM, random playlist from the music folder | OGG + MP3 streaming player | Convert the 2 XM files to OGG once (approved) |
| Network play | TCP port 9999, struct memcpy, host-authoritative, lobby, chat, `lasthost.txt` | TCP, versioned binary protocol, same model. Main scenario: 2 PCs on the same LAN, and the host also plays | Not compatible with old clients (OK). Remember the last host in settings. Windows Firewall asks for permission the first time you host. Optional: find LAN hosts automatically via UDP broadcast |
| Chat clipboard paste | Win32 `GetClipboardData` | `net10.0-windows` clipboard | Small |
| Dedicated server | `-d` flag, but `dmenu()` is an empty infinite loop (not functional) | Not needed | Dropped. The host plays from the hosting machine |
| Settings | `sets.txt` key=value | JSON | Old settings are not migrated |
| Timing | Frame counter at `fps` (100), catch-up by skipping frames | Fixed 100 Hz accumulator, vsync rendering | Keep 100 Hz so gameplay feel stays the same |
| Memory/safety | malloc without free, raw buffers | Managed, pooled | Fixed by the rewrite |
| Code language | Finnish/English mix | English | Glossary below |

## 6. Phases

Each phase ends with a build that compiles and runs.

0. **Preparation** (done)
   - Local git repo, `original/` committed untouched.
   - The original runs. Reference screenshots are in `docs/reference/`.
   - Assets are copied and renamed to English as each feature is ported, not all up front. XM to OGG conversion happens in phase 5.
1. **Skeleton** (done): `Saoss.slnx`, Core/Game/Tests projects, central package versions, warnings as errors, `.editorconfig`. The main menu is ported: title picture, bitmap font, 3D arrows, `MeshBuilder` with a GL-style matrix stack and `gluSphere`.
2. **Core simulation**, in sub-steps:
   - 2a (done): ship catalog for all 24 types, generated from `Alustuksia.cpp` by a script.
   - 2b (done): `World` step, movement (`liiku`), planet gravity and impacts, ship and asteroid collisions, health pickups, death and respawn, presentation events. Visual-only code (debris, exhaust, spawn sparks) moves to the Game layer.
   - 2c (done): primary weapons (`ammu`, `liikutaPadit`), one behavior class per ship type, plus a generic port of projectile-vs-projectile clashes (`tarkistaHajosikoPadit`).
   - 2d (done): special weapons (`ease`, `liikutaePadit`).
   - Every ship's weapon and special type equals its ship index, so behaviors are keyed by `ShipType`.
   - Result: ~5,700 lines of C# in `Saoss.Core` and 102 tests, including a smoke test that flies every ship type for 600 ticks. Gameplay quirks of the original (e.g. damage applied twice) are kept and marked with comments.
   - Deliberate changes from the original:
     - Ships that are exploded or respawning cannot be hit and don't collide with asteroids or pick up health (the original let invisible wrecks absorb shots).
     - A few loops that could overrun arrays or read uninitialised variables now stop safely.
   - Follow-ups for later phases:
     - The Moth special clears the held special button in the original. The input layer must make specials trigger on key press, not while held.
     - Effects (debris, sparks, exhaust) are `EffectEvent`s whose docs list the original particle parameters, to be drawn in phase 3.
3. **Rendering** (done):
   - 3a: an OpenGL 1.x emulation layer (`Rendering/Legacy/LegacyGl`): matrix stacks, immediate mode, display lists, GLU quadrics with Mesa's tessellation, and fixed-function lighting computed on the CPU (point lights, specular, color material), batched to MonoGame. This lets the original drawing code be translated call by call.
   - Scene (`drawFrame`): camera, galaxies, stars incl. the "THP" letters, planet, asteroids, pickups, border stars and flowers, particles.
   - 3b: all 24 ship models (`aluspiirto.cpp`, `Alus::piirra`); developer gallery with `Saoss.exe --ship-gallery`.
   - 3c: projectiles, slime, beams, exhaust trails, respawn sparks, thrust and damage particles, explosions, all weapon effects, HUD.
   - Result: one human against idle ships is playable from Main menu > Local Game.
   - Left for later phases: game-mode markers and HUD (phase 4), credits line in the HUD, nicknames (phase 6), beam line width (MonoGame has no wide lines).
4. **AI + game modes + local game**: AI pilot, all 5 modes including flag cloth and mushrooms, 4-player keyboard, results screen.
5. **Menus, settings, audio**: main/setup menus, JSON settings, SFX on simulation events, and the music player.
6. **Networking**: protocol, host lobby (slots human/AI/closed, teams, mode), join, state sync, chat, and disconnect handling.
7. **Polish and release**: fullscreen/resolution options, widescreen decision, performance check, and a self-contained publish.
8. **Optional**: gamepads, key remapping, LAN host discovery.

## 7. Decisions

1. Widescreen: show a larger part of the playfield (no letterbox).
2. UI text: English only.
3. XM music: convert to OGG.
4. Dedicated server: not needed. Typical setup is 2 PCs on a LAN, and the host plays too.

## 8. Glossary (Finnish to English)

| Finnish | English | | Finnish | English |
|---|---|---|---|---|
| alus / aluksia | ship / ships | | pati | projectile (main weapon) |
| epati, ease | special-weapon projectile, special weapon | | lima | slime |
| ampumis(aani) | fire (sound) | | osumis(aani) | hit (sound) |
| ammu | shoot | | liiku / liikuta | move |
| piirra | draw | | laske | update / calculate |
| tormaa | collides | | uudestaan | respawn |
| asetaAlus | set ship | | ihmispelaaja | human player |
| pelaaja | player | | tiimi | team |
| pisteet | score / points | | energia | energy (health) |
| kulma / asekulma | angle / weapon angle | | x/yvauhti | x/y velocity |
| kuolema | death (timer) | | kuolemisaika / syntymisaika | death time / spawn time |
| suojapaalla | shield active | | ampumisviive | fire cooldown |
| kaasujalki | exhaust trail | | viivat / viivakasa | line trail |
| sirpale / alkusirpale | debris / spawn debris | | hanta / palikkamaara | tail / tail segment count |
| kitka | friction | | sade / sadetoiseen | radius / radius squared |
| planeetta / pmassa | planet / planet mass | | aukko | black hole |
| pikkumies | little man (small figure) | | asteroidi | asteroid |
| alustaso | ship plane (z level) | | tausta | background |
| lippu | flag | | sieni | mushroom |
| kukka | flower | | nuoli | arrow |
| osa | part | | salama | lightning |
| muna | egg | | jalki | trail |
| peliloppu | game over / results | | alustuksia | initialization |
| aluspiirto | ship drawing | | teksti | text |
| alkukuva | title image | | nimi | name |
| propelli | propeller | | teleportattu | teleported |
| CPiste / DPiste / SPiste | CTF point / domination point / mushroom point | | netpeli | network game |
