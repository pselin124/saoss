namespace Saoss.Game.Rendering.Legacy;

/// <summary>Texture environment modes for <see cref="LegacyGl.TexEnv"/> (GL_TEXTURE_ENV_MODE values).</summary>
public enum GlTexEnv
{
    /// <summary>GL_MODULATE: fragment color times texture color (the default).</summary>
    Modulate,

    /// <summary>GL_BLEND with the default black environment color: fragment color times (1 - texture color).</summary>
    Blend,
}
