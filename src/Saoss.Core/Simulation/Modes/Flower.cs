namespace Saoss.Core.Simulation.Modes;

/// <summary>A team's flower and base in Capture The Flower [CPiste, alkupiste].</summary>
/// <param name="team">Owning team; also the flower's index.</param>
/// <param name="homeX">Base position.</param>
/// <param name="homeY">Base position.</param>
public sealed class Flower(int team, float homeX, float homeY)
{
    /// <summary>Owning team [numero].</summary>
    public int Team { get; } = team;

    /// <summary>Base position [alkupiste].</summary>
    public float HomeX { get; } = homeX;

    /// <inheritdoc cref="HomeX"/>
    public float HomeY { get; } = homeY;

    /// <summary>Whether the team takes part in the match; flowers of absent teams are ignored [enabled].</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Where the flower is.</summary>
    public FlowerState State { get; private set; }

    /// <summary>Index of the carrying ship while <see cref="State"/> is <see cref="FlowerState.Carried"/>.</summary>
    public int CarrierIndex { get; private set; } = -1;

    /// <summary>Flower position; follows the base while at home and is set when dropped [x, y].</summary>
    public float X { get; private set; } = homeX;

    /// <inheritdoc cref="X"/>
    public float Y { get; private set; } = homeY;

    /// <summary>Overwrites the flower's state with values read from a snapshot.</summary>
    internal void Restore(bool isEnabled, FlowerState state, int carrierIndex, float x, float y)
    {
        IsEnabled = isEnabled;
        State = state;
        CarrierIndex = carrierIndex;
        X = x;
        Y = y;
    }

    /// <summary>Puts the flower back on its base.</summary>
    public void ReturnHome()
    {
        State = FlowerState.AtHome;
        CarrierIndex = -1;
        X = HomeX;
        Y = HomeY;
    }

    /// <summary>The flower is picked up by a ship.</summary>
    public void PickUp(int shipIndex)
    {
        State = FlowerState.Carried;
        CarrierIndex = shipIndex;
    }

    /// <summary>The carrier was destroyed; the flower stays at its position.</summary>
    public void Drop(float x, float y)
    {
        State = FlowerState.Dropped;
        CarrierIndex = -1;
        X = x;
        Y = y;
    }
}
