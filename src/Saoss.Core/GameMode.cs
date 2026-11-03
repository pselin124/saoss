namespace Saoss.Core;

/// <summary>
/// Game modes selectable from the local game menu and the network lobby.
/// </summary>
public enum GameMode
{
    /// <summary>Free-for-all with ships chosen by the players.</summary>
    Deathmatch = 0,

    /// <summary>Free-for-all where every respawn gives a random ship.</summary>
    RandomShipsDeathmatch = 1,

    /// <summary>Teams score by holding flower control points.</summary>
    FlowerDomination = 2,

    /// <summary>Teams carry the enemy flower to their own base.</summary>
    CaptureTheFlower = 3,

    /// <summary>Players score by eating mushrooms.</summary>
    EatTheMushroom = 4,
}
