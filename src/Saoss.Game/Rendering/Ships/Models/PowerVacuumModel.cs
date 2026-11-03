using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Power Vacuum (ship 13): a translucent green jellyfish shape: a glowing core that shifts from green to red
/// as the held shot charges, a skirt around it, a flared cannon in front and a red jet disc while
/// thrusting [alus13, case 13].
/// </summary>
public static class PowerVacuumModel
{
    /// <summary>
    /// Records the translucent outer parts [DRAW_ALUS_13]: cannon ball, cannon, skirt, cannon collar, rear
    /// disc and rear cylinder.
    /// </summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        gl.PushMatrix();
        gl.Color(0.35f, 0.65f, 0.35f, 0.87f);
        gl.Translate(0, 1, 0);
        gl.Sphere(0.35f, 10 + (5 * detail), 5 + (4 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.45f, 0.55f, 0.45f, 0.97f);
        gl.Translate(0, 1.15f, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.2f, 0.45f, 0.8f, 12 + (8 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        // Skirt.
        gl.PushMatrix();
        gl.Color(0.3f, 0.7f, 0.3f, 0.6f);
        gl.Translate(0, -1.1f, 0);
        gl.Scale(1, 1, 0.5f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(1.4f, 0.8f, 2.1f, 12 + (8 * detail), 1);
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.4f, 0.55f, 0.4f, 0.9f);
        gl.Translate(0, 1, 0);
        gl.Scale(0.8f, 0.2f, 0.5f * 0.8f);
        gl.Sphere(1, 10 + (10 * detail), 8 + (6 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.4f, 0.65f, 0.4f, 0.7f);
        gl.Translate(0, -1.1f, 0);
        gl.Scale(1.4f, 0.35f, 0.5f * 1.4f);
        gl.Sphere(1, 10 + (10 * detail), 8 + (4 * detail));
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(0.5f, 0.6f, 0.5f, 0.6f);
        gl.Translate(0, -1.1f, 0);
        gl.Scale(1, 1, 0.5f);
        gl.Rotate(90, 1, 0, 0);
        gl.Cylinder(0.5f, 0.7f, 0.7f, 10 + (10 * detail), 1);
        gl.PopMatrix();
        gl.EndList();
        return list;
    }

    /// <summary>
    /// Draws the ship with alpha blending. Steps: red inner ball, the core colored by the charge of the
    /// newest shot (primary slot 0 while it was fired in the last 3 ticks), the outer list, the jet disc.
    /// </summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        var detail = c.Detail;
        gl.Enable(GlCapability.Lighting);
        gl.Disable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.Enable(GlCapability.DepthTest);

        gl.PushMatrix();
        gl.Scale(c.Radius / 1.1f * 0.65f, c.Radius / 1.1f * 0.65f, c.Radius / 1.1f * 0.65f);
        gl.Disable(GlCapability.Texture2D);
        gl.Rotate(c.Angle - 90f, 0, 0, 1);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcAlpha);

        var shot = c.Ship.PrimaryProjectiles[0];
        var charge = shot.Lifetime >= c.Ship.Definition.Primary.Lifetime - 3 ? (0.7f * shot.Red) + 0.2f : 0.2f;

        gl.PushMatrix();
        gl.Color(1.0f, 0.0f, 0.0f, 1.0f);
        gl.Scale(0.7f, 0.5f, 1);
        gl.Sphere(0.35f, 8, 5);
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Color(charge, 1.0f - charge, 0.2f, 0.7f);
        gl.Scale(0.7f, 1, 0.5f);
        gl.Sphere(1, 12 + (8 * detail), 8 + (6 * detail));
        gl.PopMatrix();

        gl.CallList(c.Models.PowerVacuumBody);

        if (c.IsThrusting)
        {
            gl.PushMatrix();
            gl.Color(0.7f, 0.2f, 0.2f, 0.7f);
            gl.Translate(0, -1.8f, 0);
            gl.Scale(1, 1, 0.5f);
            gl.Rotate(90, 1, 0, 0);
            gl.Disk(0, 0.7f, 10 + (10 * detail), 1 + (2 * detail));
            gl.PopMatrix();
        }

        gl.Enable(GlCapability.Texture2D);
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.Lighting);
        gl.PopMatrix();
    }
}
