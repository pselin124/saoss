using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Ships.Models;

/// <summary>
/// Electra (ship 18): four glowing red balls on a grey rod, tumbling slowly, each wrapped in a crackling
/// electric spark billboard [alus18, case 18].
/// </summary>
public static class ElectraModel
{
    private const float SparkSize = 0.7f;
    private const float SmallBallX = 0.45f;

    /// <summary>Records the balls and rod [DRAW_ALUS_18] and one spark quad [DRAW_ALUS_18_SALAMA].</summary>
    public static (GlDisplayList Body, GlDisplayList Spark) Record(LegacyGl gl, int detail)
    {
        var body = gl.NewList();
        gl.Enable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Lighting);
        gl.Disable(GlCapability.Texture2D);
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.SrcColor, GlBlend.One);
        RecordBall(gl, detail, 0, -1.3f, 0.5f);
        RecordBall(gl, detail, 0, 1.3f, 0.5f);
        RecordBall(gl, detail, SmallBallX, 0, 0.3f);
        RecordBall(gl, detail, -SmallBallX, 0, 0.3f);

        gl.PushMatrix();
        gl.Translate(0, -1, 0);
        gl.Color(0.7f, 0.7f, 0.7f, 0.7f);
        gl.Rotate(-90.0f, 1.0f, 0.0f, 0.0f);
        gl.Cylinder(0.1f, 0.1f, 2, 5 + (5 * detail), 1);
        gl.PopMatrix();
        gl.EndList();

        var spark = gl.NewList();
        gl.Begin(GlPrimitive.Quads);
        gl.Normal(0, 0, 1.0f);
        gl.TexCoord(0.0f, 1.0f);
        gl.Vertex(-1.0f * SparkSize, -1.0f * SparkSize, 0.0f);
        gl.TexCoord(1.0f, 1.0f);
        gl.Vertex(1.0f * SparkSize, -1.0f * SparkSize, 0.0f);
        gl.TexCoord(1.0f, 0.0f);
        gl.Vertex(1.0f * SparkSize, 1.0f * SparkSize, 0.0f);
        gl.TexCoord(0.0f, 0.0f);
        gl.Vertex(-1.0f * SparkSize, 1.0f * SparkSize, 0.0f);
        gl.End();
        gl.EndList();
        return (body, spark);
    }

    /// <summary>
    /// Draws the ship. Steps: tumble the whole ship by <c>PropellerAngle</c>, draw the balls with the electric
    /// texture bound, then put a spark on each ball with the tumble undone so the sparks stay flat.
    /// Leaves texturing on, like the original.
    /// </summary>
    public static void Draw(ShipDrawContext c)
    {
        var gl = c.Gl;
        var spin = c.Ship.PropellerAngle;
        gl.PushMatrix();
        c.ScaleAndTurn(c.Radius / 1.0f * 0.7f);
        gl.Rotate(spin / 5.0f, 1.0f, 0.0f, 0.0f);
        gl.Rotate(spin / 4.0f, 0.0f, 1.0f, 0.0f);
        gl.Rotate(spin / 3.0f, 0.0f, 0.0f, 1.0f);
        gl.BindTexture(c.Textures.Electric);
        gl.CallList(c.Models.ElectraBody);

        gl.PushMatrix();
        gl.Translate(0, -1.3f, 0);
        gl.Color(2, 2, 2, 1.5f);
        gl.Enable(GlCapability.Texture2D);
        gl.BlendFunc(GlBlend.SrcColor, GlBlend.One);
        DrawSpark(c, spin);
        gl.PopMatrix();

        gl.PushMatrix();
        gl.Translate(0, 1.3f, 0);
        DrawSpark(c, spin);
        gl.PopMatrix();

        foreach (var x in new[] { SmallBallX, -SmallBallX })
        {
            gl.PushMatrix();
            gl.Translate(x, 0, 0);
            gl.Scale(SmallBallX / SparkSize, SmallBallX / SparkSize, SmallBallX / SparkSize);
            DrawSpark(c, spin);
            gl.PopMatrix();
        }

        gl.PopMatrix();
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.Lighting);
        gl.Disable(GlCapability.DepthTest);
    }

    private static void DrawSpark(ShipDrawContext c, float spin)
    {
        c.Gl.Rotate(-1 * spin / 3.0f, 0.0f, 0.0f, 1.0f);
        c.Gl.Rotate(-1 * spin / 4.0f, 0.0f, 1.0f, 0.0f);
        c.Gl.Rotate(-1 * spin / 5.0f, 1.0f, 0.0f, 0.0f);
        c.Gl.CallList(c.Models.ElectraSpark);
    }

    private static void RecordBall(LegacyGl gl, int detail, float x, float y, float radius)
    {
        gl.PushMatrix();
        gl.Translate(x, y, 0);
        gl.Color(0.8f, 0.1f, 0.1f);
        gl.Sphere(radius, 10 + (10 * detail), 8 + (10 * detail));
        gl.PopMatrix();
    }
}
