using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Builds stand-in ships for menu and end screens, so a ship can be drawn at a chosen spot without touching the
/// match's own ships.
/// </summary>
public static class StandInShips
{
    /// <summary>
    /// A freshly spawned ship of <paramref name="owner"/>'s player and team with the given type, at
    /// (<paramref name="x"/>, <paramref name="y"/>) facing <paramref name="heading"/> degrees, gun forward and
    /// with a still exhaust tail.
    /// </summary>
    public static Ship Create(Ship owner, ShipDefinition definition, float x, float y, float heading, GameRandom random)
    {
        var ship = new Ship(owner.Index, owner.Team, owner.IsHuman);
        ship.Respawn(definition, random);
        ship.X = x;
        ship.Y = y;
        ship.Angle = heading;
        ship.WeaponAngle = 0;
        for (var i = 0; i < GameConstants.TailLength; i++)
        {
            ship.ShiftTail(recordCurrent: true);
        }

        return ship;
    }
}
