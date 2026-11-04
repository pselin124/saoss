namespace Saoss.Network.Protocol;

/// <summary>Data from the network breaks the protocol; the connection is dropped.</summary>
public sealed class ProtocolException : Exception
{
    /// <summary>Creates the exception with a description of the problem.</summary>
    public ProtocolException(string message)
        : base(message)
    {
    }

    /// <summary>Creates the exception with a description and the error that revealed it.</summary>
    public ProtocolException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
