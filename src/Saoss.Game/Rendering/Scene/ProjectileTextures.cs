using Microsoft.Xna.Framework.Graphics;
using Saoss.Core.Ships;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Which texture each ship type's projectiles use, from the per-ship assignments at the end of glapi.cpp
/// (patitexture, epatitexture). Unassigned or never-loaded slots mean "no texture" (null).
/// </summary>
public static class ProjectileTextures
{
    /// <summary>Texture of primary projectiles [alus.patitexture].</summary>
    public static Texture2D? Primary(ShipType type, SceneTextures t) => type switch
    {
        ShipType.Magneto or ShipType.Wasp or ShipType.Subni or ShipType.Rollu or ShipType.Popper => t.Shot0,
        ShipType.Tringu or ShipType.Cubix => t.Shot1,
        ShipType.Trek => t.Shot6,
        ShipType.Bomba => t.Missile,
        ShipType.Rayska => t.Shot8,
        ShipType.Bloddu => t.Trail,
        ShipType.Shapperi or ShipType.Bulldog => t.Shot15,
        ShipType.Moth or ShipType.PowerVacuum or ShipType.Elephant or ShipType.Tongu or ShipType.StickInsect => t.Flare,
        ShipType.SpaceTurtle => t.Shot20,
        ShipType.StarWorm => t.Shot22,
        _ => null,
    };

    /// <summary>Texture of special projectiles [alus.epatitexture].</summary>
    public static Texture2D? Special(ShipType type, SceneTextures t) => type switch
    {
        ShipType.Tringu or ShipType.Subni => t.Shot0,
        ShipType.Rollu => t.Sucker,
        ShipType.Trek => t.LittleMan,
        ShipType.Rayska => t.Egg,
        ShipType.PeaceCannon => t.BlackHole,
        ShipType.PowerVacuum or ShipType.Elephant or ShipType.StickInsect or ShipType.StarWorm => t.Flare,
        ShipType.Cubix => t.Special18,
        ShipType.SpaceTurtle => t.Special20,
        ShipType.Popper => t.Shot22,
        _ => null,
    };
}
