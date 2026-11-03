namespace Saoss.Core;

/// <summary>
/// Global simulation constants carried over from the original <c>Define.h</c>.
/// Per-tick values assume <see cref="TicksPerSecond"/> simulation steps per second.
/// </summary>
public static class GameConstants
{
    /// <summary>Simulation rate; the original game's default <c>fps</c> setting.</summary>
    public const int TicksPerSecond = 100;

    /// <summary>Maximum number of player slots (human, AI or network).</summary>
    public const int MaxPlayers = 16;

    /// <summary>Number of distinct ship types.</summary>
    public const int ShipTypeCount = 24;

    /// <summary>Velocity damping applied every tick.</summary>
    public const float Friction = 0.006f;

    /// <summary>Arena bounds in world units.</summary>
    public const float ArenaMinX = -160f;

    /// <inheritdoc cref="ArenaMinX"/>
    public const float ArenaMaxX = 160f;

    /// <inheritdoc cref="ArenaMinX"/>
    public const float ArenaMinY = -120f;

    /// <inheritdoc cref="ArenaMinX"/>
    public const float ArenaMaxY = 120f;

    /// <summary>Z coordinate of the plane the ships fly on.</summary>
    public const float ShipPlaneZ = -35f;
}
