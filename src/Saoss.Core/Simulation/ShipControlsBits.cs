namespace Saoss.Core.Simulation;

/// <summary>
/// Packs <see cref="ShipControls"/> into one byte, like the original's per-player button byte [nappulat].
/// </summary>
public static class ShipControlsBits
{
    private const byte ThrustBit = 1;
    private const byte LeftBit = 2;
    private const byte RightBit = 4;
    private const byte FireBit = 8;
    private const byte SpecialBit = 16;

    /// <summary>Bits that may be set in a valid byte.</summary>
    public const byte AllBits = ThrustBit | LeftBit | RightBit | FireBit | SpecialBit;

    /// <summary>Packs the buttons.</summary>
    public static byte ToBits(ShipControls controls) => (byte)(
        (controls.Thrust ? ThrustBit : 0)
        | (controls.Left ? LeftBit : 0)
        | (controls.Right ? RightBit : 0)
        | (controls.Fire ? FireBit : 0)
        | (controls.Special ? SpecialBit : 0));

    /// <summary>Unpacks the buttons; bits outside <see cref="AllBits"/> are ignored.</summary>
    public static ShipControls FromBits(byte bits) => new()
    {
        Thrust = (bits & ThrustBit) != 0,
        Left = (bits & LeftBit) != 0,
        Right = (bits & RightBit) != 0,
        Fire = (bits & FireBit) != 0,
        Special = (bits & SpecialBit) != 0,
    };
}
