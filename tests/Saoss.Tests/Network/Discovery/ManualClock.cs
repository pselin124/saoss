namespace Saoss.Tests.Network.Discovery;

/// <summary>A clock that only moves when told to.</summary>
internal sealed class ManualClock : TimeProvider
{
    private long _ticks;

    /// <inheritdoc/>
    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    /// <inheritdoc/>
    public override long GetTimestamp() => _ticks;

    /// <summary>Moves the clock forward.</summary>
    public void Advance(TimeSpan time) => _ticks += time.Ticks;
}
