namespace Saoss.Game.Screens.Network;

/// <summary>Corners of a menu's translucent panel on the panel plane (see <see cref="MenuScene.DrawPanel"/>).</summary>
/// <param name="Left">Left edge.</param>
/// <param name="Bottom">Bottom edge.</param>
/// <param name="Right">Right edge.</param>
/// <param name="Top">Top edge.</param>
public sealed record MenuPanel(float Left, float Bottom, float Right, float Top);
