namespace Saoss.Network.Discovery;

/// <summary>A decoded discovery reply.</summary>
/// <param name="Token">The token of the request it answers.</param>
/// <param name="InstanceId">Random id of the answering host, so one host reached on several addresses is listed once.</param>
/// <param name="Game">The host's game.</param>
public sealed record DiscoveryReply(int Token, int InstanceId, LanGameInfo Game);
