namespace Saoss.Core.Simulation.Snapshots;

/// <summary>
/// A world snapshot is truncated, malformed or does not fit the world it is applied to.
/// </summary>
public sealed class SnapshotFormatException : Exception
{
    /// <summary>Creates the exception with a description of the problem.</summary>
    public SnapshotFormatException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a description and the error that revealed it.</summary>
    public SnapshotFormatException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
