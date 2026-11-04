namespace Saoss.Game.Settings;

/// <summary>
/// A screen size in pixels.
/// </summary>
/// <param name="Width">Width in pixels.</param>
/// <param name="Height">Height in pixels.</param>
public readonly record struct Resolution(int Width, int Height);
