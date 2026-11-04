using Saoss.Core.Ships;

namespace Saoss.Network.Protocol;

/// <summary>A client buys a ship for its pending choice (netav.cpp).</summary>
/// <param name="ShipIndex">The choosing ship; must be the world's pending choice and belong to the client.</param>
/// <param name="ShipType">The type, or null for a random affordable one.</param>
public sealed record ShipChoiceRequestMessage(int ShipIndex, ShipType? ShipType) : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.ShipChoiceRequest;
}
