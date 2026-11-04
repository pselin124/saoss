
                    SAOSS - Space Adventure of Small Ship

A frantic space shooter for up to 16 ships around a planet: up to 4 players on one computer
(keyboard or gamepads), computer pilots, and network games over LAN or the internet.

A modern port of the original Saoss (C++/SDL/OpenGL) by Paulus Selin and friends.


GETTING STARTED
---------------

Requirements: 64-bit Windows 10 or 11 with OpenGL graphics. Nothing else to install.

1. Unzip the whole folder anywhere.
2. Run Saoss.exe.
3. Press Enter or Escape to leave the intro, then pick "Local Game" and
   "Start With Random Ships" to jump straight in.

Menus: arrow keys move, Enter chooses, Escape goes back. A gamepad works too
(D-pad / left stick, A or Start = Enter, B or Back = Escape).

Settings are saved in %LOCALAPPDATA%\Saoss\settings.json. If the game crashes, the details go
to crash.log in the same folder.


THE BASICS
----------

- A planet sits in the middle of the arena. Its gravity pulls most ships in, and touching it
  hurts and bounces you off. Asteroids float around and can be shot.
- Each ship has a hull (energy) bar and a fuel bar in its corner of the screen. Both the main
  weapon and the special weapon use fuel; fuel refills by itself over time (with exceptions,
  see the ship list). Out of fuel = no shooting.
- Ships drift: there is no brake. Turn around and thrust to slow down.
- Destroyed ships respawn after a moment. Teammates' area weapons do not hurt each other.


CONTROLS
--------

Default keyboard layouts (from the original game):

              Thrust      Left        Right       Fire          Special
  Player 1    Up arrow    Left arrow  Right arrow Right Shift   Right Ctrl
  Player 2    W           A           D           Left Shift    Left Ctrl
  Player 3    I           J           L           B             N
  Player 4    Numpad 8    Numpad 4    Numpad 6    Numpad 0      Numpad .

Each layout also has a "Down" key (Down arrow, S, K, Numpad 5). Ships have no brake; Down is
only used to move the selection in the ship purchase menu.

Escape leaves a local match (choose "Resume Game" in the local game menu to continue it). In a
network game Escape opens the in-match menu while the match keeps running.

Remapping keys and using gamepads:
  Settings > Controls
  - Player:  choose which local player (1-4) to edit.
  - Device:  Keyboard or Gamepad 1-4. Each gamepad can belong to only one player; the keyboard
             can be shared by all four.
  - Thrust / Left / Right / Down / Fire / Special: press Enter, then the new key. Escape cancels.
             If the key is already used by any player, the two are swapped (a message shows it).
             Escape and Enter cannot be bound, they are reserved for the menus.
  - Reset All Keys: back to the original layouts above.
  Then choose "Apply and Save Settings" (or press Escape) in the settings menu.

Gamepad layout (fixed):
  Thrust   Left stick up, D-pad up or right shoulder (RB)
  Turn     Left stick or D-pad left/right
  Down     Left stick or D-pad down (purchase menu only)
  Fire     A or right trigger
  Special  B or left trigger


GAME MODES
----------

Local Game menu:
  Start With Random Ships          Free-for-all. Every respawn gives you a new random ship.
                                   Never ends - leave with Escape.
  Start With <N> Credits           Everyone starts with N credits and buys each ship from the
                                   purchase menu. You are out when you can no longer afford the
                                   cheapest ship (Wasp). The last team standing wins.
  Start With Credits & Random Ships  Like above, but each ship is a random one you can afford.
  Flower Domination                Two flower control points in opposite corners. Be the only
                                   team at a point to take it; held points score continuously.
  Capture The Flower               Steal the enemy team's flower and carry it to your own base
                                   while your own flower is at home. Dying costs points.
  Eat The Mushroom                 Touch the mushroom first to score; it then jumps elsewhere.
                                   Dying costs points.

In the point modes ships are random, and the first to reach the score limit wins (the "Credits"
setting doubles as the score limit).

Ship purchase menu (credits games): Thrust/Right and Down/Left move through the 24 ships and
"random ship", Fire or Enter buys. Ships cost credits (see the ship list); credits are not
earned back, so choose wisely. A local match pauses while a human chooses.


SETTINGS AND PLAYERS
--------------------

Settings menu: resolution, fullscreen, number of players (2-16), music and sound on/off and
volume, starting credits / score limit (20-1000), game speed ("Fps", 50-200, default 100),
Player Setup, Controls, AI level, camera (3d tilted or 2d top-down) and detail level (0-3).

Player Setup: for each player choose Human or CPU and a team. Only players 1-4 can be human
(one per keyboard layout / gamepad); the rest are computer pilots. Players with the same team
number are allies.

AI levels: Space Cadet (easy), Rocket Ace (medium), Galactic Terror (the original, hard).
Computer pilots fight well in the deathmatch and credits games, but they ignore flowers and
mushrooms - in those modes they just hunt other ships.

Music: drop your own .mp3 or .ogg files into Content\Music and they join the random playlist.


NETWORK GAMES
-------------

- Up to 16 player slots per game, and up to 4 players from each computer (each with their own
  keyboard layout or gamepad). Empty slots can be opened for humans, closed, or filled with CPU
  pilots by the host.
- Host Multiplayer: choose how many players are on this computer, set nicknames, then in the
  lobby set slots (open / closed / cpu), teams, game mode, maximum players, score limit and AI
  level, and start the game. The lobby has a chat.
- Join Multiplayer: "find games on LAN" lists games on your local network, or "change IP" to
  type the host's name or address (it is remembered), then "connect". Joined players can change
  their team in the lobby.
- The host runs the game; everyone else sends their controls and sees the host's world, so the
  host should be the computer with the best connection.
- Ports: the game uses TCP 9999. LAN game discovery uses UDP 9998. Allow Saoss through the
  Windows firewall when asked. To host over the internet, forward TCP port 9999 on your router
  to the hosting computer.
- All players must run the same version of Saoss. It cannot play with the original game.


THE SHIPS
---------

24 ships, listed in purchase-menu order. "Hull" is the ship's starting energy; "Cost" is its
price in credits games.

  Name           Hull Cost  Main weapon (Fire) / Special weapon / Notes
  -------------- ---- ----  --------------------------------------------------------------------
  Magneto          50   19  Fire:    Two shots at once, angled out from both sides of the nose.
                            Special: Shield for a short while; blocks shots and ramming.

  Tringu           15   16  Fire:    One big, slow, very strong shot that kicks you backwards.
                            Special: A ring of small shots flying out in every direction.

  Wasp             13    9  Fire:    Fast stream of small shots.
                            Special: Self-destruct! Hurts nearby ships (the closer, the more) and
                                     wipes out nearby shots. The cheapest ship.

  Subni            40   18  Fire:    Single shots, alternating left and right side.
                            Special: Drops a mine that creeps toward the nearest enemy and
                                     explodes on contact (easy to bump into yourself).

  Rollu            50   30  Fire:    A heavy bomb that flies while you hold Fire and bursts into a ring
                                     of 30 fragments when you release it (or when it runs out).
                            Special: Leeches that chase the nearest enemy and drain its fuel.

  Trek             60   33  Fire:    Big, slow shots with a little recoil.
                            Special: Two drones fly to the nearest enemy and zap it with lasers.
                                     Each borrows one point of your hull until it flies back.

  UFO              21   18  Fire:    Short laser that locks onto an enemy in range, or shoots down
                                     an enemy projectile.
                            Special: Teleport to a random spot.
                            Notes:   No gravity or inertia: moves only while Thrust is held and
                                     stops dead when released.

  Bomba            35   21  Fire:    Slow mines that home in on the nearest enemy.
                            Special: Short lightning bolts zap every enemy ship and shot close by.

  Rayska           30   16  Fire:    Rapid spray of small shots with random spread.
                            Special: Long-lived bouncing mines dropped behind; they knock back any
                                     ship they hit - you too.

  Peace Cannon     35   17  Fire:    Continuous green laser beam that burns every ship and shot
                                     along it.
                            Special: Slow black holes that pull in ships and shots and hurt
                                     whatever touches them.

  Bloddu           25   15  Fire:    Single straight shots.
                            Special: Bites hull points off nearby enemies (the closer, the more).
                                     Each point drops out as a little figure that heals whoever
                                     picks it up - go grab them.

  Shapperi         30   20  Fire:    Short blue laser bolt that hurts every ship it crosses and
                                     weakens shots.
                            Special: Transforms the ship (toggle). The other form fires pairs of
                                     straight long-range shots and accelerates harder, but turns
                                     very slowly and barely refuels.

  Moth             20   16  Fire:    Three shots at once: left, forward and right.
                            Special: Refills a third of the fuel tank - the Moth has no other
                                     fuel regeneration, so tap it often.
                            Notes:   50% chance to come straight back to life when destroyed.

  Power Vacuum     30   26  Fire:    Hold Fire to grow a ball at the nose (green -> red = stronger),
                                     release to launch it. Fuel does not refill while charging.
                            Special: Slime seekers that chase the nearest enemy and slow it down.

  Bulldog          33   19  Fire:    Slow rockets that keep accelerating and can be steered with
                                     your turn keys after launch.
                            Special: Forward boost for ramming.
                            Notes:   Ramming an enemy hurts it - harder the more fuel you have -
                                     but empties your fuel tank.

  Wiiii!           28   17  Fire:    Long-lived mines dropped around you that wander randomly.
                            Special: Boost mode: very fast, can't fire, and stays on until the
                                     fuel runs out.
                            Notes:   Ramming an enemy hurts it.

  Elephant         65   47  Fire:    Wobbling shots, alternating from both sides of the nose.
                            Special: Cluster bomb dropped behind: bursts into 20 bomblets that
                                     burst again into 15 each. Hurts everyone, you included!
                            Notes:   The heaviest, toughest and most expensive ship.

  Cubix            15   14  Fire:    Single shots from the nose.
                            Special: Drops a mine and jumps forward; hold it to lay a mine trail.
                            Notes:   The lightest ship.

  Electra          24   18  Fire:    Short lightning bolt that zaps the nearest enemy ship (or else
                                     an enemy projectile) in reach.
                            Special: Eats a nearby asteroid to refill the fuel tank.
                            Notes:   Always flying; no gravity or inertia. Thrust flips the ship
                                     180 degrees instead of accelerating.

  Tongu            28   17  Fire:    A pair of shots from both sides; the longer you hold Fire, the
                                     wider they spread and the farther, faster and stronger they go.
                            Special: Shoots out a tongue that hits hard on contact.

  Space Turtle     54   31  Fire:    Single wobbling shots that hit hard.
                            Special: A spinning ring of small shots sprayed all around the ship.

  Stick Insect     28   18  Fire:    A wide fan of 18 small shots in front of the ship.
                            Special: A bomb dropped behind that bursts into 30 drifting fragments.

  Star Worm        28   22  Fire:    Single spinning shots from its head.
                            Special: 25 drifting sparks sprayed backward from the tail.
                            Notes:   Never stops moving; no gravity or inertia. Has a long
                                     segmented tail.

  Popper           24   24  Fire:    Rapid long-lived bouncy shots that kick you back and keep
                                     hitting whatever they touch.
                            Special: A mine that explodes after a while, hurting nearby ships
                                     (you too) and shots.


CREDITS
-------

Original game: Paulus Selin (programming, music, graphics), Marita Selin and k-One (music),
Miika Sell (graphics).
