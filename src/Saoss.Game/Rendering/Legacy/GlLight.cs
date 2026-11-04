using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// One OpenGL fixed-function light. Positions are stored in eye space, transformed when set, like glLightfv.
/// </summary>
public sealed class GlLight
{
    private readonly GlLighting _owner;
    private bool _enabled;
    private Vector4 _position = new(0, 0, 1, 0);
    private Vector4 _ambient = new(0, 0, 0, 1);
    private Vector4 _diffuse;
    private Vector4 _specular;
    private float _constantAttenuation = 1f;
    private float _linearAttenuation;
    private float _quadraticAttenuation;

    /// <summary>Creates a light of <paramref name="owner"/> with OpenGL's defaults for lights 1 to 7.</summary>
    internal GlLight(GlLighting owner) => _owner = owner;

    /// <summary>Whether the light contributes (GL_LIGHTi enabled).</summary>
    public bool Enabled { get => _enabled; set => Set(ref _enabled, value); }

    /// <summary>Eye-space position; W = 0 means a directional light.</summary>
    public Vector4 Position { get => _position; set => Set(ref _position, value); }

    /// <summary>Ambient intensity.</summary>
    public Vector4 Ambient { get => _ambient; set => Set(ref _ambient, value); }

    /// <summary>Diffuse intensity.</summary>
    public Vector4 Diffuse { get => _diffuse; set => Set(ref _diffuse, value); }

    /// <summary>Specular intensity.</summary>
    public Vector4 Specular { get => _specular; set => Set(ref _specular, value); }

    /// <summary>Distance attenuation terms.</summary>
    public float ConstantAttenuation { get => _constantAttenuation; set => Set(ref _constantAttenuation, value); }

    /// <inheritdoc cref="ConstantAttenuation"/>
    public float LinearAttenuation { get => _linearAttenuation; set => Set(ref _linearAttenuation, value); }

    /// <inheritdoc cref="ConstantAttenuation"/>
    public float QuadraticAttenuation { get => _quadraticAttenuation; set => Set(ref _quadraticAttenuation, value); }

    /// <summary>Takes every setting of <paramref name="other"/>.</summary>
    internal void CopyFrom(GlLight other)
    {
        _enabled = other._enabled;
        _position = other._position;
        _ambient = other._ambient;
        _diffuse = other._diffuse;
        _specular = other._specular;
        _constantAttenuation = other._constantAttenuation;
        _linearAttenuation = other._linearAttenuation;
        _quadraticAttenuation = other._quadraticAttenuation;
    }

    private void Set<T>(ref T field, T value)
    {
        field = value;
        _owner.MarkChanged();
    }
}
