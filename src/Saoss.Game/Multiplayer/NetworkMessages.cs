using Saoss.Network.Protocol;
using Saoss.Network.Sessions;

namespace Saoss.Game.Multiplayer;

/// <summary>Texts of the network error screens, close to the original's messages.</summary>
public static class NetworkMessages
{
    /// <summary>Shown while the connection is being opened.</summary>
    public static string Connecting(string host) => $"connecting to {host}..";

    /// <summary>Hint under <see cref="Connecting"/>.</summary>
    public const string CancelHint = "press escape to cancel";

    /// <summary>The host could not listen (the original exited with SDLNet_TCP_Open's error).</summary>
    public static string CannotHost(int port) => $"cannot open port {port}";

    /// <summary>Why a client's session ended.</summary>
    public static string Disconnected(DisconnectReason? reason, RejectReason? rejected, string host) => reason switch
    {
        DisconnectReason.ConnectFailed => $"cannot connect to {host}",
        DisconnectReason.Rejected => rejected switch
        {
            RejectReason.LobbyFull => "the game is full",
            RejectReason.GameInProgress => "the game has already started",
            _ => "the host runs another version of saoss",
        },
        DisconnectReason.HostLeft => "the host quit the game",
        DisconnectReason.ProtocolError => "the host sent something strange",
        _ => "connection to the host was lost",
    };
}
