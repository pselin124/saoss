namespace Saoss.Network.Protocol;

/// <summary>A client's buttons for its ships [nap]; the host uses the newest until the next one arrives.</summary>
/// <param name="Tick">The client's latest world tick, so older input arriving late is ignored.</param>
/// <param name="Inputs">One entry per local ship, at most four.</param>
public sealed record PlayerInputMessage(int Tick, IReadOnlyList<ShipInput> Inputs) : Message
{
    /// <inheritdoc/>
    public override MessageType Type => MessageType.PlayerInput;
}
