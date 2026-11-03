using Saoss.Core;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Star Worm (ship 22): a green ball head with four red eyes, followed by a chain of shrinking, fading
/// green balls that bends along the ship's recent headings [alus22, case 22].
/// </summary>
public static class StarWormModel
{
    /// <summary>
    /// Records the head [DRAW_ALUS_22]. The list switches on alpha blending for the fading body and leaves
    /// the head frame pushed; the draw code pops it.
    /// </summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        gl.Enable(GlCapability.Lighting);
        gl.Disable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.Enable(GlCapability.DepthTest);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcAlpha);

        gl.PushMatrix();
        gl.Color(0.3f, 0.8f, 0.3f);
        gl.Translate(0, 1.5f, 0);
        gl.Sphere(0.5f, 10 + (8 * detail), 6 + (8 * detail));

        gl.PushMatrix();
        gl.Color(0.8f, 0.1f, 0.1f);
        gl.Translate(0.25f, 0.25f, 0.25f);
        gl.Sphere(0.2f, 7 + (5 * detail), 5 + (4 * detail));
        gl.Translate(-0.25f * 2, 0, 0);
        gl.Sphere(0.2f, 7 + (5 * detail), 5 + (4 * detail));
        gl.Translate(0, 0, -2 * 0.25f);
        gl.Sphere(0.2f, 7 + (5 * detail), 5 + (4 * detail));
        gl.Translate(2 * 0.25f, 0, 0);
        gl.Sphere(0.2f, 7 + (5 * detail), 5 + (4 * detail));
        gl.PopMatrix();
        gl.EndList();
        return list;
    }

    /// <summary>
    /// Draws the ship: the head, then each body ball turned by the heading change between every second
    /// tail entry and stepped half a unit back.
    /// </summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        var tail = c.Ship.TailAngle;
        gl.PushMatrix();
        gl.Rotate(c.Angle - 90.0f, 0.0f, 0.0f, 1.0f);
        gl.Scale(c.Radius * 0.5f, c.Radius * 0.5f, c.Radius * 0.5f);
        gl.CallList(c.Models.StarWormHead);

        gl.Color(0.3f, 0.8f, 0.3f);
        for (var i = 1; i < GameConstants.TailLength / 2; i++)
        {
            gl.Rotate(tail[i * 2] - tail[(i - 1) * 2], 0, 0, 1);
            gl.Translate(0, -0.5f, 0);
            gl.Color(0.3f, 0.8f, 0.3f, 1.0f - (i * 0.1f));
            gl.Sphere(0.5f - (0.02f * i), 10 + (8 * c.Detail), 6 + (8 * c.Detail));
        }

        gl.PopMatrix();
        gl.PopMatrix();
        ShipGlState.Overlay(gl);
    }
}
