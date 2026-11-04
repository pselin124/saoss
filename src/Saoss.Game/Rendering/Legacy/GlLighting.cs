using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// Fixed-function lighting state and the per-vertex OpenGL 1.x lighting equation.
/// </summary>
public sealed class GlLighting
{
    /// <summary>Number of lights, as in OpenGL.</summary>
    public const int LightCount = 8;

    /// <summary>Creates the default state: light 0 white, the others black.</summary>
    public GlLighting()
    {
        for (var i = 0; i < LightCount; i++)
        {
            var isFirst = i == 0;
            Lights[i] = new GlLight
            {
                Diffuse = isFirst ? Vector4.One : new Vector4(0, 0, 0, 1),
                Specular = isFirst ? Vector4.One : new Vector4(0, 0, 0, 1),
            };
        }
    }

    /// <summary>The lights.</summary>
    public GlLight[] Lights { get; } = new GlLight[LightCount];

    /// <summary>GL_LIGHT_MODEL_AMBIENT.</summary>
    public Vector4 GlobalAmbient { get; set; } = new(0.2f, 0.2f, 0.2f, 1f);

    /// <summary>GL_LIGHT_MODEL_LOCAL_VIEWER.</summary>
    public bool LocalViewer { get; set; }

    /// <summary>GL_LIGHT_MODEL_TWO_SIDE: back-facing polygons are lit with the reversed normal.</summary>
    public bool TwoSide { get; set; }

    /// <summary>GL_COLOR_MATERIAL: the current color replaces the ambient and diffuse material.</summary>
    public bool ColorMaterial { get; set; }

    /// <summary>Front material ambient.</summary>
    public Vector4 MaterialAmbient { get; set; } = new(0.2f, 0.2f, 0.2f, 1f);

    /// <summary>Front material diffuse.</summary>
    public Vector4 MaterialDiffuse { get; set; } = new(0.8f, 0.8f, 0.8f, 1f);

    /// <summary>Front material specular.</summary>
    public Vector4 MaterialSpecular { get; set; } = new(0, 0, 0, 1);

    /// <summary>Front material emission.</summary>
    public Vector4 MaterialEmission { get; set; } = new(0, 0, 0, 1);

    /// <summary>Front material shininess (specular exponent).</summary>
    public float Shininess { get; set; }

    /// <summary>
    /// Lights one vertex: emission + global ambient + per light (ambient + diffuse + specular) * attenuation,
    /// clamped to [0, 1]; alpha comes from the diffuse material. Position and normal are in eye space.
    /// </summary>
    public Vector4 Shade(Vector3 position, Vector3 normal, Vector4 currentColor)
    {
        var ambientMaterial = ColorMaterial ? currentColor : MaterialAmbient;
        var diffuseMaterial = ColorMaterial ? currentColor : MaterialDiffuse;
        var result = ToVector3(MaterialEmission) + (ToVector3(GlobalAmbient) * ToVector3(ambientMaterial));
        foreach (var light in Lights)
        {
            if (!light.Enabled)
            {
                continue;
            }

            Vector3 toLight;
            var attenuation = 1f;
            if (light.Position.W != 0)
            {
                var offset = (ToVector3(light.Position) / light.Position.W) - position;
                var distance = offset.Length();
                toLight = distance > 0 ? offset / distance : Vector3.UnitZ;
                attenuation = 1f / (light.ConstantAttenuation + (light.LinearAttenuation * distance) + (light.QuadraticAttenuation * distance * distance));
            }
            else
            {
                toLight = Vector3.Normalize(ToVector3(light.Position));
            }

            var normalDotLight = MathF.Max(Vector3.Dot(normal, toLight), 0f);
            var contribution = (ToVector3(light.Ambient) * ToVector3(ambientMaterial))
                + (normalDotLight * ToVector3(light.Diffuse) * ToVector3(diffuseMaterial));
            if (normalDotLight > 0)
            {
                var toEye = LocalViewer ? SafeNormalize(-position) : Vector3.UnitZ;
                var halfway = SafeNormalize(toLight + toEye);
                var normalDotHalf = MathF.Max(Vector3.Dot(normal, halfway), 0f);
                var specular = Shininess == 0 ? 1f : MathF.Pow(normalDotHalf, Shininess);
                contribution += specular * ToVector3(light.Specular) * ToVector3(MaterialSpecular);
            }

            result += attenuation * contribution;
        }

        return new Vector4(Vector3.Clamp(result, Vector3.Zero, Vector3.One), MathHelper.Clamp(diffuseMaterial.W, 0f, 1f));
    }

    private static Vector3 ToVector3(Vector4 value) => new(value.X, value.Y, value.Z);

    private static Vector3 SafeNormalize(Vector3 value)
    {
        var length = value.Length();
        return length > 0 ? value / length : Vector3.UnitZ;
    }
}
