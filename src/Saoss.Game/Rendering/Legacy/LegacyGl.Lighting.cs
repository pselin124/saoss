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
        if (TryRecord(g => g.LightPosition(light, x, y, z, w)))
        {
            return;
        }

        var eye = Vector4.Transform(new Vector4(x, y, z, w), _modelView);
        Lighting.Lights[light].Position = eye;
    }

    /// <summary>glLightfv(GL_DIFFUSE).</summary>
    public void LightDiffuse(int light, float r, float g, float b, float a)
    {
        if (TryRecord(gl => gl.LightDiffuse(light, r, g, b, a)))
        {
            return;
        }

        Lighting.Lights[light].Diffuse = new Vector4(r, g, b, a);
    }

    /// <summary>glLightfv(GL_SPECULAR).</summary>
    public void LightSpecular(int light, float r, float g, float b, float a)
    {
        if (TryRecord(gl => gl.LightSpecular(light, r, g, b, a)))
        {
            return;
        }

        Lighting.Lights[light].Specular = new Vector4(r, g, b, a);
    }

    /// <summary>glLightfv(GL_AMBIENT).</summary>
    public void LightAmbient(int light, float r, float g, float b, float a)
    {
        if (TryRecord(gl => gl.LightAmbient(light, r, g, b, a)))
        {
            return;
        }

        Lighting.Lights[light].Ambient = new Vector4(r, g, b, a);
    }

    /// <summary>glLightModelfv(GL_LIGHT_MODEL_AMBIENT).</summary>
    public void LightModelAmbient(float r, float g, float b, float a)
    {
        if (TryRecord(gl => gl.LightModelAmbient(r, g, b, a)))
        {
            return;
        }

        Lighting.GlobalAmbient = new Vector4(r, g, b, a);
    }

    /// <summary>glLightModeli(GL_LIGHT_MODEL_LOCAL_VIEWER).</summary>
    public void LightModelLocalViewer(bool enabled)
    {
        if (TryRecord(g => g.LightModelLocalViewer(enabled)))
        {
            return;
        }

        Lighting.LocalViewer = enabled;
    }

    /// <summary>glMaterialfv(GL_FRONT, GL_SPECULAR).</summary>
    public void MaterialSpecular(float r, float g, float b, float a)
    {
        if (TryRecord(gl => gl.MaterialSpecular(r, g, b, a)))
        {
            return;
        }

        Lighting.MaterialSpecular = new Vector4(r, g, b, a);
    }

    /// <summary>glMaterialfv(GL_FRONT, GL_EMISSION).</summary>
    public void MaterialEmission(float r, float g, float b, float a)
    {
        if (TryRecord(gl => gl.MaterialEmission(r, g, b, a)))
        {
            return;
        }

        Lighting.MaterialEmission = new Vector4(r, g, b, a);
    }

    /// <summary>glMaterialf(GL_FRONT, GL_SHININESS).</summary>
    public void MaterialShininess(float shininess)
    {
        if (TryRecord(g => g.MaterialShininess(shininess)))
        {
            return;
        }

        Lighting.Shininess = shininess;
    }
}
