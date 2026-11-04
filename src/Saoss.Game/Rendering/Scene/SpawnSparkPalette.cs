using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Per-ship-type colors of the respawn sparks, so each ship materializes in its own colors
/// (the color switch of laitaAlkuSirpaleet).
/// </summary>
public static class SpawnSparkPalette
{
    /// <summary>Picks a random spark color for the given ship type.</summary>
    public static (float R, float G, float B) Pick(ShipType type, GameRandom random)
    {
        float C(float range, float offset) => (random.Next(range) / 100f) + offset;
        float Quarter() => random.Next(100) / 400f;
        (float, float, float) Grey(float value) => (value, value, value);

        // Bulldog: blue is a random fraction of red.
        (float, float, float) Bulldog(float red) => (red, red, (random.Next(100f * red) / 100f) + (red / 10f));

        // Wiiii: blue is rolled first and limits red and green.
        (float, float, float) Wiiii(float blue)
        {
            var red = (random.Next(100f * blue / 2f) / 100f) + 0.1f;
            return (red, (random.Next(100f * blue / 1.5f) / 100f) + 0.1f, blue);
        }

        var roll = random.Next(100);
        return type switch
        {
            ShipType.Magneto => roll < 80 ? (C(90, 0.6f), C(80, 0.4f), C(50, 0.3f)) : (C(20, 0.6f), C(20, 0.6f), C(20, 0.6f)),
            ShipType.Tringu => roll < 33 ? (C(30, 0.6f), C(30, 0.4f), C(20, 0f)) : roll < 66 ? (C(20, 0.8f), C(30, 0.4f), C(20, 0.8f)) : (C(20, 0.8f), C(20, 0.8f), C(20, 0.8f)),
            ShipType.Wasp => roll < 33 ? (C(30, 0.7f), C(20, 0f), C(20, 0f)) : roll < 66 ? (C(30, 0.7f), C(30, 0.7f), C(20, 0f)) : (C(50, 0.5f), C(30, 0f), C(30, 0.7f)),
            ShipType.Twinblade => roll < 60 ? (C(10, 0.45f), C(7, 0.36f), C(7, 0.36f)) : roll < 80 ? (C(30, 0.7f), C(20, 0f), C(20, 0f)) : (C(20, 0.7f), C(20, 0.7f), C(20, 0.7f)),
            ShipType.Leechmother => roll < 33 ? (C(30, 0.7f), C(30, 0.7f), C(30, 0.7f)) : roll < 66 ? (C(30, 0.7f), C(30, 0.7f), C(20, 0f)) : (C(20, 0f), C(20, 0f), C(30, 0.7f)),
            ShipType.Trek => roll < 33 ? (C(30, 0.6f), C(30, 0.6f), C(30, 0.6f)) : roll < 66 ? (C(30, 0.7f), C(30, 0.4f), C(30, 0.2f)) : (C(50, 0.5f), C(50, 0.3f), C(50, 0.1f)),
            ShipType.Thunderbolt => roll < 66 ? Grey(C(20, 0.7f)) : roll < 88 ? (C(30, 0.7f), C(20, 0f), C(20, 0f)) : (C(30, 0.7f), C(30, 0.7f), C(20, 0f)),
            ShipType.ScatterRay => roll < 33 ? (C(20, 0.7f), C(10, 0f), C(20, 0.4f)) : (C(20, 0.6f), C(20, 0.5f), C(20, 0.45f)),
            ShipType.PeaceCannon => (Quarter(), Quarter() + 0.75f, Quarter()),
            ShipType.Vampire => (C(30, 0.7f), C(15, 0.2f), 0f),
            ShipType.Shapper => (C(40, 0.6f), C(40, 0.6f), C(40, 0.6f)),
            ShipType.Moth => roll < 66 ? (C(15, 0f), C(15, 0f), C(40, 0.6f)) : Grey(C(20, 0.6f)),
            ShipType.PowerVacuum => (C(5, 0f), C(60, 0.3f), C(5, 0f)),
            ShipType.Bulldog => roll < 60 ? Bulldog(C(50, 0.1f)) : roll < 85 ? (C(20, 0.8f), C(40, 0f), C(40, 0f)) : (0f, C(50, 0.5f), 0f),
            ShipType.Wiiii => roll < 50 ? Wiiii(C(50, 0.5f)) : roll < 80 ? (C(20, 0.5f), C(10, 0f), C(10, 0f)) : (C(20, 0.8f), C(50, 0.1f), C(30, 0.4f)),
            ShipType.Elephant => roll < 50 ? BlueFirst(C(10, 0.05f), C(10, 0.05f), C(30, 0.7f)) : roll < 85 ? (C(30, 0.7f), C(10, 0.05f), C(10, 0.05f)) : (C(10, 0.05f), C(10, 0.05f), C(30, 0.7f)),
            ShipType.Cubix => roll < 60 ? BlueFirst(C(30, 0.5f), C(30, 0.5f), C(30, 0.5f)) : roll < 75 ? (C(30, 0.7f), C(10, 0.05f), C(10, 0.05f)) : (C(10, 0.05f), C(10, 0.05f), C(30, 0.7f)),
            ShipType.Electra => roll < 70 ? BlueFirst(C(10, 0f), C(30, 0.7f), C(10, 0f)) : (C(20, 0.5f), C(20, 0.5f), C(20, 0.5f)),
            ShipType.Tongu => roll < 80 ? BlueFirst(C(30, 0.7f), C(20, 0.1f), C(20, 0.1f)) : (C(30, 0.7f), C(10, 0f), C(10, 0f)),
            ShipType.SpaceTurtle => BlueGreenRed(C(20, 0.1f), C(40, 0.6f), C(20, 0.1f)),
            ShipType.StickInsect => roll < 35 ? BlueFirst(C(20, 0.35f), C(20, 0.5f), C(20, 0.35f)) : roll < 75 ? (C(10, 0.15f), C(30, 0.7f), C(10, 0.15f)) : (C(30, 0.7f), C(10, 0.05f), C(10, 0.05f)),
            ShipType.StarWorm => roll < 90 ? BlueFirst(C(20, 0.1f), C(20, 0.1f), C(70, 0.3f)) : (C(30, 0.7f), C(10, 0f), C(10, 0f)),
            ShipType.Popper => BlueGreenRed(C(20, 0.6f), C(35, 0.6f), C(20, 0.6f)),
            _ => (Quarter(), Quarter(), Quarter() + 0.75f), // Ufo, and the original's default.
        };
    }

    // Helpers for cases where the original rolled the channels in a different order than red, green, blue.
    // Rolled blue, red, green.
    private static (float, float, float) BlueFirst(float blue, float red, float green) => (red, green, blue);

    // Rolled blue, green, red.
    private static (float, float, float) BlueGreenRed(float blue, float green, float red) => (red, green, blue);
}
