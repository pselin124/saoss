namespace Saoss.Game.Input;

/// <summary>
/// The buttons of one local player, in the order the controls menu lists them.
/// </summary>
public enum PlayerAction
{
    /// <summary>Thrust; also moves the ship shop selection up.</summary>
    Thrust,

    /// <summary>Turn left; also moves the ship shop selection down.</summary>
    Left,

    /// <summary>Turn right; also moves the ship shop selection up.</summary>
    Right,

    /// <summary>Moves the ship shop selection down; ships have no brake.</summary>
    Down,

    /// <summary>Fire the main weapon; also buys in the ship shop.</summary>
    Fire,

    /// <summary>Use the special weapon.</summary>
    Special,
}
