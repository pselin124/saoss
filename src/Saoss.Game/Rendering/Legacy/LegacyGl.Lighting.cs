using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// Light and material setters (glLightfv, glLightModel, glMaterial).
/// </summary>
public sealed partial class LegacyGl
{
    /// <summary>glLightfv(GL_POSITION): the position is transformed by the current modelview matrix.</summary>
    public void LightPosition(int light, float x, float y, float z, float w)
    {
        if (TryRecord((light, x, y, z, w), static (g, a) => g.LightPosition(a.light, a.x, a.y, a.z, a.w)))
        {
            return;
        }

        var eye = Vector4.Transform(new Vector4(x, y, z, w), _modelView);
        Lighting.Lights[light].Position = eye;
    }

    /// <summary>glLightfv(GL_DIFFUSE).</summary>
    public void LightDiffuse(int light, float r, float g, float b, float a)
    {
        if (TryRecord((light, r, g, b, a), static (gl, c) => gl.LightDiffuse(c.light, c.r, c.g, c.b, c.a)))
        {
            return;
        }

        Lighting.Lights[light].Diffuse = new Vector4(r, g, b, a);
    }

    /// <summary>glLightfv(GL_SPECULAR).</summary>
    public void LightSpecular(int light, float r, float g, float b, float a)
    {
        if (TryRecord((light, r, g, b, a), static (gl, c) => gl.LightSpecular(c.light, c.r, c.g, c.b, c.a)))
        {
            return;
        }

        Lighting.Lights[light].Specular = new Vector4(r, g, b, a);
    }

    /// <summary>glLightfv(GL_AMBIENT).</summary>
    public void LightAmbient(int light, float r, float g, float b, float a)
    {
        if (TryRecord((light, r, g, b, a), static (gl, c) => gl.LightAmbient(c.light, c.r, c.g, c.b, c.a)))
        {
            return;
        }

        Lighting.Lights[light].Ambient = new Vector4(r, g, b, a);
    }

    /// <summary>glLightModelfv(GL_LIGHT_MODEL_AMBIENT).</summary>
    public void LightModelAmbient(float r, float g, float b, float a)
    {
        if (TryRecord((r, g, b, a), static (gl, c) => gl.LightModelAmbient(c.r, c.g, c.b, c.a)))
        {
            return;
        }

        Lighting.GlobalAmbient = new Vector4(r, g, b, a);
    }

    /// <summary>glLightModeli(GL_LIGHT_MODEL_LOCAL_VIEWER).</summary>
    public void LightModelLocalViewer(bool enabled)
    {
        if (TryRecord(enabled, static (g, e) => g.LightModelLocalViewer(e)))
        {
            return;
        }

        Lighting.LocalViewer = enabled;
    }

    /// <summary>glLightModeli(GL_LIGHT_MODEL_TWO_SIDE).</summary>
    public void LightModelTwoSide(bool enabled)
    {
        if (TryRecord(enabled, static (g, e) => g.LightModelTwoSide(e)))
        {
            return;
        }

        Lighting.TwoSide = enabled;
    }

    /// <summary>glMaterialfv(GL_FRONT, GL_SPECULAR).</summary>
    public void MaterialSpecular(float r, float g, float b, float a)
    {
        if (TryRecord((r, g, b, a), static (gl, c) => gl.MaterialSpecular(c.r, c.g, c.b, c.a)))
        {
            return;
        }

        Lighting.MaterialSpecular = new Vector4(r, g, b, a);
    }

    /// <summary>glMaterialfv(GL_FRONT, GL_EMISSION).</summary>
    public void MaterialEmission(float r, float g, float b, float a)
    {
        if (TryRecord((r, g, b, a), static (gl, c) => gl.MaterialEmission(c.r, c.g, c.b, c.a)))
        {
            return;
        }

        Lighting.MaterialEmission = new Vector4(r, g, b, a);
    }

    /// <summary>glMaterialf(GL_FRONT, GL_SHININESS).</summary>
    public void MaterialShininess(float shininess)
    {
        if (TryRecord(shininess, static (g, s) => g.MaterialShininess(s)))
        {
            return;
        }

        Lighting.Shininess = shininess;
    }
}
