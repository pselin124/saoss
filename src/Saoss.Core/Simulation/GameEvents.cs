using Saoss.Core.Ships;

namespace Saoss.Core.Simulation;

/// <summary>
/// Something that happened during a tick that the presentation layer (sound, particles) reacts to.
/// The simulation itself never reads events.
/// </summary>
public abstract record GameEvent;

/// <summary>Sounds a ship type can make.</summary>
public enum ShipSound
{
    /// <summary>Primary weapon fired [ampumisaani].</summary>
    Fire,

    /// <summary>Primary weapon hit something [osumisaani].</summary>
    Hit,

    /// <summary>Special weapon used [eampumisaani].</summary>
    SpecialFire,

    /// <summary>Special weapon hit something [eosumisaani].</summary>
    SpecialHit,
}

/// <summary>A ship sound should play.</summary>
/// <param name="ShipIndex">Ship that made the sound; looping sounds are tracked per ship.</param>
/// <param name="SoundOf">Ship type whose sound set is used (not always the emitting ship's own type).</param>
/// <param name="Sound">Which sound of that set.</param>
/// <param name="Loop">Whether the sound loops until <see cref="ShipLoopStopped"/>.</param>
public sealed record ShipSoundPlayed(int ShipIndex, ShipType SoundOf, ShipSound Sound, bool Loop = false) : GameEvent;

/// <summary>A looping weapon sound of the ship should stop.</summary>
public sealed record ShipLoopStopped(int ShipIndex) : GameEvent;

/// <summary>A ship was destroyed at the given position.</summary>
public sealed record ShipExploded(int ShipIndex, float X, float Y) : GameEvent;

/// <summary>A destroyed Moth came straight back to life at a new position.</summary>
public sealed record ShipRevived(int ShipIndex) : GameEvent;

/// <summary>A ship got its new ship type and position and starts its respawn animation.</summary>
public sealed record ShipRespawning(int ShipIndex) : GameEvent;

/// <summary>An asteroid broke up at the given position.</summary>
public sealed record AsteroidExploded(float X, float Y) : GameEvent;

/// <summary>
/// Purely visual effects requested by weapons. Each value documents the particles the original spawned,
/// so the presentation layer can reproduce them.
/// </summary>
public enum EffectKind
{
    /// <summary>
    /// Tringu or Trek shot hit a ship. Original: 150 + 50 * detail sparks at the target, speed up to 0.2 in a disc
    /// plus the shot velocity, life 25-75 of max 75, size 0-0.6, red 0.5-2.5, green and blue 0-1.
    /// </summary>
    HeavyShotImpact,

    /// <summary>
    /// A Rollu leech crashed into the planet. Original: 50 sparks at the leech, each velocity component
    /// random(25) / 50 - 0.25 (rerolled until the speed is below 0.3), no z motion, life 50-150 of max 150,
    /// size not set, red, green and blue each 0-1.
    /// </summary>
    LeechPlanetImpact,

    /// <summary>
    /// A Rayska drifting mine hit a ship. Original: 150 + 50 * detail sparks at the target, each velocity component
    /// random(200) / 1000 - 0.1 (rerolled until the speed is at most 0.1) plus the mine velocity, no z motion,
    /// life 25-75 of max 75, size 0-0.4, grey: red 0.2-1.45 with green and blue equal to red.
    /// </summary>
    DriftMineImpact,
}

/// <summary>A visual effect at a position, optionally with a velocity it inherits.</summary>
public sealed record EffectEvent(EffectKind Kind, float X, float Y, float VelocityX = 0, float VelocityY = 0) : GameEvent;
