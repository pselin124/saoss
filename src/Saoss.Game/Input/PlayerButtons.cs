namespace Saoss.Game.Input;

/// <summary>
/// Which of one player's buttons are down (or went down), whatever device they come from.
/// </summary>
/// <param name="Thrust">Thrust.</param>
/// <param name="Left">Turn left.</param>
/// <param name="Right">Turn right.</param>
/// <param name="Down">Ship shop selection down.</param>
/// <param name="Fire">Main weapon.</param>
/// <param name="Special">Special weapon.</param>
public readonly record struct PlayerButtons(bool Thrust, bool Left, bool Right, bool Down, bool Fire, bool Special)
{
    /// <summary>The buttons down now that were up in <paramref name="previous"/>.</summary>
    public PlayerButtons PressedSince(PlayerButtons previous) => new(
        Thrust && !previous.Thrust,
        Left && !previous.Left,
        Right && !previous.Right,
        Down && !previous.Down,
        Fire && !previous.Fire,
        Special && !previous.Special);
}
