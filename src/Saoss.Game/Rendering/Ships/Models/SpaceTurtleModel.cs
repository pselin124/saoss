using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Space Turtle (ship 20): a flat green shell with a turtle-shell texture, a gun head in front and four
/// leaf-shaped flippers [alus20, case 20].
/// </summary>
public static class SpaceTurtleModel
{
    /// <summary>Records the whole ship [DRAW_ALUS_20]. Steps: textured shell, gun head and barrel, four flippers.</summary>
    public static GlDisplayList Record(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        gl.Enable(GlCapability.Lighting);
        gl.Enable(GlCapability.Texture2D);
        gl.Enable(GlCapability.DepthTest);
        gl.Disable(GlCapability.Blend);

        gl.PushMatrix();
        gl.Color(0.3f, 0.8f, 0.3f);
        gl.Scale(1, 1, 0.5f);
        gl.Sphere(1, 10 + (6 * detail), 6 + (4 * detail), textured: true);
        gl.PopMatrix();
        gl.Disable(GlCapability.Texture2D);

        gl.PushMatrix();
        gl.Translate(0, 0.9f, 0);
        gl.Color(0.33f, 0.7f, 0.33f);
        gl.Sphere(0.45f, 6 + (6 * detail), 5 + (4 * detail));
        gl.Color(0.37f, 0.6f, 0.37f);
        gl.Translate(0, 0.1f, 0);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.35f, 0.25f, 1, 5 + (4 * detail), 1 + (2 * detail));
        gl.PopMatrix();

        // Flippers; the list ends rotated 360 degrees, back where it started.
        gl.Color(0.37f, 0.8f, 0.37f);
        gl.Rotate(45, 0, 0, 1);
        for (var i = 0; i < 4; i++)
        {
            gl.Translate(0, 1.5f, 0);
            gl.Scale(1, 2, 0.25f);
            gl.Sphere(0.5f, 12 + (8 * detail), 8 + (6 * detail));
            gl.Scale(1, 0.5f, 4);
            gl.Translate(0, -1.5f, 0);
            gl.Rotate(90, 0, 0, 1);
        }

        // Depth testing is left on, as in the original.
        gl.Enable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.Lighting);
        gl.EndList();
        return list;
    }

    /// <summary>Draws the ship with the shell texture bound.</summary>
    public static void Draw(ShipDrawContext c)
    {
        c.Gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.0f * 0.5f);
        c.Gl.BindTexture(c.Textures.SpaceTurtle);
        c.Gl.CallList(c.Models.SpaceTurtleBody);
        c.Gl.PopMatrix();
    }
}
