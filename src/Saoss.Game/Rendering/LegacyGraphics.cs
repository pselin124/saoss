using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saoss.Game.Rendering;

/// <summary>
/// Render states and matrices that reproduce the original OpenGL 1.x setup.
/// </summary>
public static class LegacyGraphics
{
    /// <summary>Global ambient light of the original (<c>GL_LIGHT_MODEL_AMBIENT</c>).</summary>
    public const float AmbientLight = 0.35f;

    private const float FieldOfViewDegrees = 45f;
    private const float NearPlane = 1f;
    private const float FarPlane = 1024f;

    /// <summary>
    /// <c>glBlendFunc(GL_ONE, GL_ONE_MINUS_SRC_COLOR)</c>: black source pixels leave the target unchanged.
    /// Used for the font and other black-background textures.
    /// </summary>
    public static readonly BlendState TextBlend = new()
    {
        Name = nameof(TextBlend),
        ColorSourceBlend = Blend.One,
        ColorDestinationBlend = Blend.InverseSourceColor,
        AlphaSourceBlend = Blend.One,
        AlphaDestinationBlend = Blend.InverseSourceColor,
    };

    /// <summary>Back-face culling was disabled in the original, and some models rely on that.</summary>
    public static readonly RasterizerState NoCulling = RasterizerState.CullNone;

    /// <summary>
    /// The original projection, <c>gluPerspective(45, width / height, 1, 1024)</c>. The vertical field of view
    /// is fixed, so wider windows show more of the scene horizontally.
    /// </summary>
    public static Matrix CreateProjection(Viewport viewport) =>
        Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FieldOfViewDegrees), viewport.AspectRatio, NearPlane, FarPlane);

    /// <summary>
    /// Configures per-vertex lighting with one directional light and vertex colors as the material,
    /// like the original <c>GL_COLOR_MATERIAL</c> setup.
    /// </summary>
    /// <param name="effect">Effect to configure.</param>
    /// <param name="lightDirection">Direction the light travels, in world space.</param>
    /// <param name="diffuse">Diffuse intensity of the light.</param>
    /// <param name="specularPower">Shininess; specular highlights are white.</param>
    public static void ConfigureLitVertexColors(BasicEffect effect, Vector3 lightDirection, float diffuse, float specularPower)
    {
        effect.TextureEnabled = false;
        effect.VertexColorEnabled = true;
        effect.LightingEnabled = true;
        effect.PreferPerPixelLighting = false;
        effect.DiffuseColor = Vector3.One;
        effect.AmbientLightColor = new Vector3(AmbientLight);
        effect.SpecularColor = Vector3.One;
        effect.SpecularPower = specularPower;
        effect.DirectionalLight0.Enabled = true;
        effect.DirectionalLight0.Direction = Vector3.Normalize(lightDirection);
        effect.DirectionalLight0.DiffuseColor = new Vector3(diffuse);
        effect.DirectionalLight0.SpecularColor = Vector3.One;
        effect.DirectionalLight1.Enabled = false;
        effect.DirectionalLight2.Enabled = false;
    }
}
