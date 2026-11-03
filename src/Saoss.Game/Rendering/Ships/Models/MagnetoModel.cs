using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Magneto (ship 0): a long sand-colored ellipsoid with a glassy blue canopy, two thin tail fins and two
/// side pods with gun barrels. Turns blue with an orange canopy while the shield is on [alus0, case 0].
/// </summary>
public static class MagnetoModel
{
    /// <summary>Records the hull [DRAW_ALUS_0_OSA_1], canopy [DRAW_ALUS_0_OSA_2] and gun pods [DRAW_ALUS_0_OSA_3].</summary>
    public static (GlDisplayList Hull, GlDisplayList Canopy, GlDisplayList Guns) Record(LegacyGl gl, int detail)
    {
        var hull = gl.NewList();
        gl.Scale(1, 2.6f, 0.6f);
        gl.Sphere(0.5f, 15 + (detail * 10), 5 + (8 * detail));
        gl.EndList();

        var canopy = gl.NewList();
        gl.Scale(1, 1f / 2.6f, 1f / 0.6f);
        gl.Translate(0, 0.5f, 0.1f);
        gl.Scale(1, 2.3f, 1);
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.One, GlBlend.OneMinusSrcAlpha);
        gl.Sphere(0.3f, 15 + (detail * 10), 5 + (8 * detail));
        gl.Disable(GlCapability.Blend);
        gl.EndList();

        var guns = gl.NewList();
        gl.Sphere(0.5f, 15 + (detail * 10), 5 + (8 * detail));
        gl.Translate(-2.6f / 0.7f, 0, 0);
        gl.Sphere(0.5f, 15 + (detail * 10), 5 + (8 * detail));
        gl.Scale(1f / 0.7f, 1, 1);
        gl.Translate(0, 0.15f, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Color(0.6f, 0.6f, 0.6f);
        gl.Cylinder(0.08f, 0.03f, 1.5f, 5 + (detail * 5), 1 + (detail * 2));
        gl.Translate(2.6f, 0, 0);
        gl.Cylinder(0.08f, 0.03f, 1.5f, 5 + (detail * 5), 1 + (detail * 2));
        gl.EndList();
        return (hull, canopy, guns);
    }

    /// <summary>Draws the ship; part colors swap between warm and cold with the shield.</summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        var shield = c.Ship.IsShieldActive;
        ShipGlState.Solid(gl);
        gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.7f);

        gl.PushMatrix();
        SetColor(gl, shield, (0.5f, 0.8f, 0.9f), (0.9f, 0.8f, 0.5f));
        gl.CallList(c.Models.MagnetoHull);
        SetColor(gl, shield, (0.8f, 0.5f, 0.2f), (0.2f, 0.5f, 0.8f), 0.5f);
        gl.CallList(c.Models.MagnetoCanopy);
        gl.PopMatrix();

        // Tail fins.
        gl.PushMatrix();
        SetColor(gl, shield, (0.4f, 0.6f, 0.75f), (0.75f, 0.6f, 0.4f));
        gl.Translate(0, -0.9f, 0.2f);
        gl.Scale(1.5f, 0.35f, 0.02f);
        gl.CallList(c.Models.Shared.Cube);
        gl.Translate(0, 0, -0.4f * 50);
        gl.CallList(c.Models.Shared.Cube);
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Translate(1.3f, -0.9f, 0);
        gl.Scale(0.7f, 1, 1);
        SetColor(gl, shield, (0.5f, 0.55f, 0.65f), (0.65f, 0.55f, 0.5f));
        gl.CallList(c.Models.MagnetoGuns);
        gl.PopMatrix();

        gl.PopMatrix();
        gl.Enable(GlCapability.Texture2D);
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.Lighting);
    }

    private static void SetColor(LegacyGl gl, bool shield, (float R, float G, float B) shielded, (float R, float G, float B) normal, float alpha = 1f)
    {
        var (r, g, b) = shield ? shielded : normal;
        gl.Color(r, g, b, alpha);
    }
}
