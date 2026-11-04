using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Small drawing helpers that the original repeated inline many times.
/// </summary>
public static class SceneDrawing
{
    /// <summary>
    /// The original's standard textured quad from (-size, -size) to (size, size) at z = 0, with the texture
    /// upright (t = 1 at the bottom).
    /// </summary>
    public static void Quad(LegacyGl gl, float size, float z = 0f)
    {
        gl.Begin(GlPrimitive.Quads);
        gl.Normal(0, 0, 1);
        gl.TexCoord(0, 1);
        gl.Vertex(-size, -size, z);
        gl.TexCoord(1, 1);
        gl.Vertex(size, -size, z);
        gl.TexCoord(1, 0);
        gl.Vertex(size, size, z);
        gl.TexCoord(0, 0);
        gl.Vertex(-size, size, z);
        gl.End();
    }

    /// <summary>Turns billboards toward the tilted camera (glRotatef(alfa, 0,0,1); glRotatef(beta, 0,1,0)).</summary>
    public static void FaceCamera(LegacyGl gl, GameCamera camera)
    {
        gl.Rotate(camera.Alpha, 0, 0, 1);
        gl.Rotate(camera.Beta, 0, 1, 0);
    }

    /// <summary>Team color of ship glows and HUD [valitseVari]; alpha above 1 is clamped like GL.</summary>
    public static void TeamColor(LegacyGl gl, int team)
    {
        var (r, g, b) = TeamRgb(team);
        gl.Color(r, g, b, 1f);
    }

    /// <summary>Red, green and blue of a team's color [valitseVari].</summary>
    public static (float R, float G, float B) TeamRgb(int team) => team switch
    {
        0 => (1f, 0.25f, 0.25f),
        1 => (0.25f, 0.25f, 1f),
        2 => (0.25f, 1f, 0.25f),
        3 => (0f, 1f, 1f),
        4 => (1f, 1f, 1f),
        5 => (1f, 0f, 1f),
        6 => (1f, 1f, 0f),
        7 => (1f, 0.6f, 0.6f),
        8 => (0.6f, 1f, 0.6f),
        9 => (0.6f, 0.6f, 1f),
        10 => (1f, 0.6f, 1f),
        11 => (1f, 1f, 0.6f),
        12 => (0.6f, 1f, 1f),
        13 => (1f, 0f, 0f),
        14 => (0f, 1f, 0f),
        15 => (0f, 0f, 1f),
        _ => (1f, 0f, 0f),
    };
}
