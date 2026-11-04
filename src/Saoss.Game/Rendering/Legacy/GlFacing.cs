using Microsoft.Xna.Framework;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>Which side of a triangle faces the viewer, as OpenGL decides it for two-sided lighting and culling.</summary>
public static class GlFacing
{
    /// <summary>
    /// Whether the triangle with clip-space corners <paramref name="a"/>, <paramref name="b"/>, <paramref name="c"/>
    /// is front facing: counter-clockwise on screen, or clockwise when <paramref name="frontFaceClockwise"/> (glFrontFace(GL_CW)).
    /// </summary>
    /// <remarks>
    /// Uses the determinant of the corners' (x, y, w), which has the sign of the on-screen winding when all corners
    /// are in front of the eye and stays correct for triangles that reach behind it (where dividing by w would flip).
    /// </remarks>
    public static bool IsFrontFacing(Vector4 a, Vector4 b, Vector4 c, bool frontFaceClockwise)
    {
        var determinant = (a.X * ((b.Y * c.W) - (c.Y * b.W)))
            - (b.X * ((a.Y * c.W) - (c.Y * a.W)))
            + (c.X * ((a.Y * b.W) - (b.Y * a.W)));
        return frontFaceClockwise ? determinant < 0 : determinant > 0;
    }
}
