using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// Fixed-function lighting state and the per-vertex OpenGL 1.x lighting equation.
/// </summary>
public sealed class GlLighting
{
    /// <summary>Number of lights, as in OpenGL.</summary>
    public const int LightCount = 8;

    private Vector4 _globalAmbient = new(0.2f, 0.2f, 0.2f, 1f);
    private bool _localViewer;
    private bool _twoSide;
    private bool _colorMaterial;
    private Vector4 _materialAmbient = new(0.2f, 0.2f, 0.2f, 1f);
    private Vector4 _materialDiffuse = new(0.8f, 0.8f, 0.8f, 1f);
    private Vector4 _materialSpecular = new(0, 0, 0, 1);
    private Vector4 _materialEmission = new(0, 0, 0, 1);
    private float _shininess;

    /// <summary>Creates the default state: light 0 white, the others black.</summary>
    public GlLighting()
    {
        for (var i = 0; i < LightCount; i++)
        {
            var isFirst = i == 0;
            Lights[i] = new GlLight(this)
            {
                Diffuse = isFirst ? Vector4.One : new Vector4(0, 0, 0, 1),
                Specular = isFirst ? Vector4.One : new Vector4(0, 0, 0, 1),
            };
        }
    }

    /// <summary>The lights.</summary>
    public GlLight[] Lights { get; } = new GlLight[LightCount];

    /// <summary>GL_LIGHT_MODEL_AMBIENT.</summary>
    public Vector4 GlobalAmbient { get => _globalAmbient; set => Set(ref _globalAmbient, value); }

    /// <summary>GL_LIGHT_MODEL_LOCAL_VIEWER.</summary>
    public bool LocalViewer { get => _localViewer; set => Set(ref _localViewer, value); }

    /// <summary>GL_LIGHT_MODEL_TWO_SIDE: back-facing polygons are lit with the reversed normal.</summary>
    public bool TwoSide { get => _twoSide; set => Set(ref _twoSide, value); }

    /// <summary>GL_COLOR_MATERIAL: the current color replaces the ambient and diffuse material.</summary>
    public bool ColorMaterial { get => _colorMaterial; set => Set(ref _colorMaterial, value); }

    /// <summary>Front material ambient.</summary>
    public Vector4 MaterialAmbient { get => _materialAmbient; set => Set(ref _materialAmbient, value); }

    /// <summary>Front material diffuse.</summary>
    public Vector4 MaterialDiffuse { get => _materialDiffuse; set => Set(ref _materialDiffuse, value); }

    /// <summary>Front material specular.</summary>
    public Vector4 MaterialSpecular { get => _materialSpecular; set => Set(ref _materialSpecular, value); }

    /// <summary>Front material emission.</summary>
    public Vector4 MaterialEmission { get => _materialEmission; set => Set(ref _materialEmission, value); }

    /// <summary>Front material shininess (specular exponent).</summary>
    public float Shininess { get => _shininess; set => Set(ref _shininess, value); }

    /// <summary>Grows with every change to this state or its lights, so copies are made only after changes.</summary>
    internal int Version { get; private set; }

    /// <summary>Records a change made to one of the lights.</summary>
    internal void MarkChanged() => Version++;

    /// <summary>Takes every setting of <paramref name="other"/>, including its lights.</summary>
    internal void CopyFrom(GlLighting other)
    {
        for (var i = 0; i < LightCount; i++)
        {
            Lights[i].CopyFrom(other.Lights[i]);
        }

        _globalAmbient = other._globalAmbient;
        _localViewer = other._localViewer;
        _twoSide = other._twoSide;
        _colorMaterial = other._colorMaterial;
        _materialAmbient = other._materialAmbient;
        _materialDiffuse = other._materialDiffuse;
        _materialSpecular = other._materialSpecular;
        _materialEmission = other._materialEmission;
        _shininess = other._shininess;
        Version++;
    }

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

    private void Set<T>(ref T field, T value)
    {
        field = value;
        Version++;
    }
}
