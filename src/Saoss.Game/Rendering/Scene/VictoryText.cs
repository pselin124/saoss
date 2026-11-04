namespace Saoss.Game.Rendering.Scene;

/// <summary>A piece of text of the end-of-match screen, at a <c>gl_println</c> position.</summary>
/// <param name="X">Left edge of the text.</param>
/// <param name="Y">Height of the text.</param>
/// <param name="Text">The text.</param>
public readonly record struct VictoryText(float X, float Y, string Text);
