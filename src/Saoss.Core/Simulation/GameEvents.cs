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
