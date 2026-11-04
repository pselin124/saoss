using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Display lists shared by the scene (setLists in Alustuksia.cpp and drawCube/drawTri in glapi.cpp):
/// asteroid, cube, wedge, flowers, mushroom and the menu arrow.
/// </summary>
public sealed class SharedModels
{
    private const int FlowerPetals = 6;

    /// <summary>Records all shared lists at the given detail level (0-3).</summary>
    public SharedModels(LegacyGl gl, int detail)
    {
        Asteroid = gl.NewList();
        gl.Sphere(1, 8 + (detail * 4), 7 + (detail * 4), textured: true);
        gl.EndList();

        Cube = gl.NewList();
        DrawCube(gl);
        gl.EndList();

        Wedge = gl.NewList();
        DrawWedge(gl);
        gl.EndList();

        Flower1 = RecordFlower(gl, detail, center: (0.1f, 0.1f, 0.8f), petals: (0.7f, 0.3f, 0.3f), centerStacksPerDetail: 5, petalStacksPerDetail: 5);
        Flower2 = RecordFlower(gl, detail, center: (0.8f, 0.1f, 0.1f), petals: (0.3f, 0.3f, 0.7f), centerStacksPerDetail: 5, petalStacksPerDetail: 5);
        Flower3 = RecordFlower(gl, detail, center: (0.8f, 0.1f, 0.8f), petals: (0.3f, 0.7f, 0.3f), centerStacksPerDetail: 4, petalStacksPerDetail: 5);
        Flower4 = RecordFlower(gl, detail, center: null, petals: null, centerStacksPerDetail: 5, petalStacksPerDetail: 4);
        Mushroom = RecordMushroom(gl, detail);
        Arrow = RecordArrow(gl);
    }

    /// <summary>Textured unit sphere [list_asteroid].</summary>
    public GlDisplayList Asteroid { get; }

    /// <summary>Unit cube from -1 to 1 [DRAW_CUBE].</summary>
    public GlDisplayList Cube { get; }

    /// <summary>Triangular prism [DRAW_TRI].</summary>
    public GlDisplayList Wedge { get; }

    /// <summary>Flower with a blue center and red petals [KUKKA1].</summary>
    public GlDisplayList Flower1 { get; }

    /// <summary>Flower with a red center and blue petals [KUKKA2].</summary>
    public GlDisplayList Flower2 { get; }

    /// <summary>Flower with a purple center and green petals [KUKKA3].</summary>
    public GlDisplayList Flower3 { get; }

    /// <summary>Flower that takes the current color [KUKKA4].</summary>
    public GlDisplayList Flower4 { get; }

    /// <summary>Red mushroom with white dots [SIENI].</summary>
    public GlDisplayList Mushroom { get; }

    /// <summary>Silver arrow pointing along +X [NUOLI].</summary>
    public GlDisplayList Arrow { get; }

    private static GlDisplayList RecordFlower(LegacyGl gl, int detail, (float R, float G, float B)? center, (float R, float G, float B)? petals, int centerStacksPerDetail, int petalStacksPerDetail)
    {
        var list = gl.NewList();
        gl.PushMatrix();
        gl.PushMatrix();
        if (center != null)
        {
            gl.Color(0.1f, 0.8f, 0.1f, 0.5f);
        }

        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.075f, 0.075f, 1, 5 + (4 * detail), 1 + detail);
        gl.PopMatrix();
        gl.Translate(0, 1, 0);
        gl.PushMatrix();
        if (center is { } c)
        {
            gl.Color(c.R, c.G, c.B, 0.5f);
        }

        gl.Scale(1, 0.75f, 1);
        gl.Sphere(0.25f, 8 + (6 * detail), 5 + (centerStacksPerDetail * detail));
        gl.PopMatrix();
        if (petals is { } p)
        {
            gl.Color(p.R, p.G, p.B, 0.5f);
        }

        for (var i = 0; i < FlowerPetals; i++)
        {
            gl.PushMatrix();
            gl.Rotate(i * 60, 0, 1, 0);
            gl.Translate(0, 0.05f, -0.25f);
            gl.Rotate(25, 1, 0, 0);
            gl.Scale(0.5f, 0.25f, 1);
            gl.Sphere(0.4f, 8 + (6 * detail), 5 + (petalStacksPerDetail * detail));
            gl.PopMatrix();
        }

        gl.PopMatrix();
        gl.EndList();
        return list;
    }

    private static GlDisplayList RecordMushroom(LegacyGl gl, int detail)
    {
        var list = gl.NewList();
        gl.PushMatrix();
        gl.PushMatrix();
        gl.Color(0.8f, 0.8f, 0.8f, 0.5f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.1f, 0.1f, 1, 7 + (5 * detail), 1 + detail);
        gl.PopMatrix();
        gl.Translate(0, 1, 0);
        gl.PushMatrix();
        gl.Color(0.8f, 0.1f, 0.1f, 0.5f);
        gl.Scale(1, 0.25f, 1);
        gl.Sphere(0.5f, 8 + (6 * detail), 5 + (5 * detail));
        gl.PopMatrix();
        gl.PushMatrix();
        gl.Color(0.8f, 0.9f, 0.9f, 0.5f);
        var dots = new (float X, float Z, float Radius)[]
        {
            (0.25f, -0.1f, 0.08f), (-0.1f, 0.35f, 0.075f), (-0.2f, -0.1f, 0.06f), (0.05f, -0.15f, 0.07f),
            (-0.15f, -0.1f, 0.075f), (0.1f, 0.45f, 0.075f), (0f, -0.7f, 0.075f),
        };
        var first = true;
        foreach (var (x, z, radius) in dots)
        {
            gl.Translate(x, first ? 0.09f : 0f, z);
            first = false;
            gl.Sphere(radius, 4 + (2 * detail), 3 + (2 * detail));
        }

        gl.PopMatrix();
        gl.PopMatrix();
        gl.EndList();
        return list;
    }

    private static GlDisplayList RecordArrow(LegacyGl gl)
    {
        var list = gl.NewList();
        gl.PushMatrix();
        gl.Color(0.8f, 0.8f, 0.8f, 0.5f);
        gl.Scale(2, 0.4f, 0.2f);
        gl.Sphere(1, 15, 10);
        gl.Scale(1 / 2f, 2.5f, 1.5f);
        gl.Translate(2, 0, 0);
        gl.Rotate(30, 0, 0, 1);
        gl.Sphere(1, 3, 15);
        gl.PopMatrix();
        gl.EndList();
        return list;
    }

    private static void DrawCube(LegacyGl gl)
    {
        gl.Begin(GlPrimitive.Quads);
        Face(gl, (0, 0, 1), (-1, -1, 1), (1, -1, 1), (1, 1, 1), (-1, 1, 1));
        Face(gl, (0, 0, -1), (-1, -1, -1), (-1, 1, -1), (1, 1, -1), (1, -1, -1));
        Face(gl, (0, 1, 0), (-1, 1, -1), (-1, 1, 1), (1, 1, 1), (1, 1, -1));
        Face(gl, (0, -1, 0), (-1, -1, -1), (1, -1, -1), (1, -1, 1), (-1, -1, 1));
        Face(gl, (1, 0, 0), (1, -1, -1), (1, 1, -1), (1, 1, 1), (1, -1, 1));
        Face(gl, (-1, 0, 0), (-1, -1, -1), (-1, -1, 1), (-1, 1, 1), (-1, 1, -1));
        gl.End();
    }

    private static void DrawWedge(LegacyGl gl)
    {
        gl.Begin(GlPrimitive.Triangles);
        gl.Normal(0, 0, 1);
        gl.Vertex(-1, -1, 1);
        gl.Vertex(1, -1, 1);
        gl.Vertex(0, 1, 1);
        gl.Normal(0, 0, -1);
        gl.Vertex(-1, -1, -1);
        gl.Vertex(1, -1, -1);
        gl.Vertex(0, 1, -1);
        gl.End();
        gl.Begin(GlPrimitive.Quads);
        Face(gl, (-1, 1, 0), (-1, -1, -1), (-1, -1, 1), (0, 1, 1), (0, 1, -1));
        Face(gl, (0, -1, 0), (-1, -1, -1), (1, -1, -1), (1, -1, 1), (-1, -1, 1));
        Face(gl, (1, 1, 0), (1, -1, -1), (0, 1, -1), (0, 1, 1), (1, -1, 1));
        gl.End();
    }

    private static void Face(LegacyGl gl, (float X, float Y, float Z) normal, params (float X, float Y, float Z)[] corners)
    {
        gl.Normal(normal.X, normal.Y, normal.Z);
        foreach (var (x, y, z) in corners)
        {
            gl.Vertex(x, y, z);
        }
    }
}
