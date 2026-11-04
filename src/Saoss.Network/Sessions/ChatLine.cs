namespace Saoss.Network.Sessions;

/// <summary>A chat line as shown to players.</summary>
/// <param name="Sender">Nickname of the writer.</param>
/// <param name="Text">The line.</param>
public sealed record ChatLine(string Sender, string Text);
