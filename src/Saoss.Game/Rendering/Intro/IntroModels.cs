using Microsoft.Xna.Framework.Graphics;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Intro;

/// <summary>
/// The intro scene's display lists (<c>luoDisplayListat</c> and <c>drawGround</c>/<c>drawPole</c> in lippu.cpp):
/// the ground disk, the metallic flag pole, a mushroom, four flowers, flat stand-ins of the plants for their shadows,
/// and the unused wind arrow. Slice counts grow with the detail level like the original's.
/// </summary>
public sealed class IntroModels
{
    private const float GroundRadius = 30f;
    private const float PoleRadius = 0.17f;
    private const float PoleKnobRadius = 0.32f;
    private const int DaisyPetals = 6;
    private const int LeafCount = 3;

    /// <summary>Records every list at the given detail level (0-3); <paramref name="groundTexture"/> textures the ground and pole.</summary>
    public IntroModels(LegacyGl gl, Texture2D groundTexture, int detail)
    {
        Ground = gl.NewList();
        RecordGround(gl, groundTexture, detail);
        gl.EndList();

        Pole = gl.NewList();
        RecordPole(gl, detail);
        gl.EndList();

        Mushroom = gl.NewList();
        RecordMushroom(gl, detail);
        gl.EndList();

        BlueDaisy = RecordDaisy(gl, detail, (0.1f, 0.1f, 0.8f), 6 + detail, 5 + detail, (0.7f, 0.3f, 0.3f), 6 + detail, 5 + detail);
        RedDaisy = RecordDaisy(gl, detail, (0.8f, 0.2f, 0.2f), 5 + detail, 5 + detail, (0.3f, 0.3f, 0.7f), 6 + (2 * detail), 4 + (2 * detail));

        StarFlower = gl.NewList();
        RecordStarFlower(gl, detail);
        gl.EndList();

        Sunflower = gl.NewList();
        RecordSunflower(gl, detail);
        gl.EndList();

        MushroomShadow = gl.NewList();
        RecordMushroomShadow(gl, detail);
        gl.EndList();

        DaisyShadow = gl.NewList();
        RecordDaisyShadow(gl, detail);
        gl.EndList();

        StarFlowerShadow = gl.NewList();
        RecordStarFlowerShadow(gl, detail);
        gl.EndList();

        SunflowerShadow = gl.NewList();
        RecordSunflowerShadow(gl, detail);
        gl.EndList();

        WindArrow = gl.NewList();
        RecordWindArrow(gl);
        gl.EndList();
    }

    /// <summary>Green textured ground disk of radius 30, drawn with GL_BLEND texturing [MAA].</summary>
    public GlDisplayList Ground { get; }

    /// <summary>Silver flag pole of height <see cref="ClothFlag.PoleHeight"/> with a golden knob [TANKO].</summary>
    public GlDisplayList Pole { get; }

    /// <summary>Red mushroom with white dots [SIENI2].</summary>
    public GlDisplayList Mushroom { get; }

    /// <summary>Flower with a blue center and red petals [KUKKA5].</summary>
    public GlDisplayList BlueDaisy { get; }

    /// <summary>Flower with a red center and blue petals [KUKKA22].</summary>
    public GlDisplayList RedDaisy { get; }

    /// <summary>Purple flower with a blue star and three leaves [KUKKA33].</summary>
    public GlDisplayList StarFlower { get; }

    /// <summary>Flat olive flower with an orange center and three leaves [KUKKA44].</summary>
    public GlDisplayList Sunflower { get; }

    /// <summary>Shadow shape of <see cref="Mushroom"/>: stem and a disk [VARJOSIENI2].</summary>
    public GlDisplayList MushroomShadow { get; }

    /// <summary>Shadow shape of both daisies: stem and six petal disks [VARJOKUKKA5, VARJOKUKKA22].</summary>
    public GlDisplayList DaisyShadow { get; }

    /// <summary>Shadow shape of <see cref="StarFlower"/>, without its stem [VARJOKUKKA33].</summary>
    public GlDisplayList StarFlowerShadow { get; }

    /// <summary>Shadow shape of <see cref="Sunflower"/>, without its stem [VARJOKUKKA44].</summary>
    public GlDisplayList SunflowerShadow { get; }

    /// <summary>Black arrow above the pole showing the wind direction [NUOLI2]; the original's call to it is commented out.</summary>
    public GlDisplayList WindArrow { get; }

    private static void RecordGround(LegacyGl gl, Texture2D texture, int detail)
    {
        gl.TexEnv(GlTexEnv.Blend);
        gl.BindTexture(texture);
        gl.PushMatrix();
        gl.Color(0.1f, 0.5f, 0.0f, 0.5f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Disk(0, GroundRadius, 30 + (5 * detail), 1, textured: true);
        gl.PopMatrix();
    }

    private static void RecordPole(LegacyGl gl, int detail)
    {
        gl.Disable(GlCapability.Texture2D);
        gl.MaterialSpecular(0.75f, 0.75f, 0.75f, 1);
        gl.MaterialShininess(72);
        gl.Color(0.88f, 0.91f, 0.94f, 1);
        gl.PushMatrix();
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(PoleRadius, PoleRadius, ClothFlag.PoleHeight, 15 + (3 * detail), 1, textured: true);
        gl.PopMatrix();
        gl.Color(1, 0.8f, 0.1f, 1);
        gl.PushMatrix();
        gl.Translate(0, ClothFlag.PoleHeight, 0);
        gl.Sphere(PoleKnobRadius, 13 + (6 * detail), 10 + (6 * detail), textured: true);
        gl.PopMatrix();
        gl.MaterialSpecular(0, 0, 0, 1);
        gl.MaterialShininess(0);
        gl.Enable(GlCapability.Texture2D);
    }

    private static void RecordMushroom(LegacyGl gl, int detail)
    {
        gl.PushMatrix();
        gl.PushMatrix();
        gl.Color(0.8f, 0.8f, 0.8f, 0.5f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(0.1f, 0.1f, 1, 5 + detail, 1);
        gl.PopMatrix();
        gl.Translate(0, 1, 0);
        gl.PushMatrix();
        gl.Color(0.8f, 0.1f, 0.1f, 0.5f);
        gl.Scale(1, 0.25f, 1);
        gl.Sphere(0.5f, 7 + (2 * detail), 5 + detail);
        gl.PopMatrix();
        gl.PushMatrix();
        gl.Color(0.8f, 0.9f, 0.9f, 0.5f);
        var dotSteps = new (float X, float Y, float Z)[] { (0.25f, 0.09f, -0.1f), (-0.1f, 0, 0.35f), (-0.2f, 0, -0.1f), (0.05f, 0, -0.15f), (-0.15f, 0, -0.1f) };
        foreach (var (x, y, z) in dotSteps)
        {
            gl.Translate(x, y, z);
            gl.Sphere(0.075f, 4 + detail, 3 + detail);
        }

        gl.PopMatrix();
        gl.PopMatrix();
    }

    private static GlDisplayList RecordDaisy(LegacyGl gl, int detail, (float R, float G, float B) center, int centerSlices, int centerStacks, (float R, float G, float B) petals, int petalSlices, int petalStacks)
    {
        var list = gl.NewList();
        gl.PushMatrix();
        RecordStem(gl, 0.075f, 5 + detail);
        gl.Translate(0, 1, 0);
        gl.PushMatrix();
        gl.Color(center.R, center.G, center.B, 0.5f);
        gl.Scale(1, 0.75f, 1);
        gl.Sphere(0.25f, centerSlices, centerStacks);
        gl.PopMatrix();
        gl.Color(petals.R, petals.G, petals.B, 0.5f);
        for (var i = 0; i < DaisyPetals; i++)
        {
            gl.PushMatrix();
            gl.Rotate(i * 60, 0, 1, 0);
            gl.Translate(0, 0.05f, -0.25f);
            gl.Rotate(25, 1, 0, 0);
            gl.Scale(0.5f, 0.25f, 1);
            gl.Sphere(0.4f, petalSlices, petalStacks);
            gl.PopMatrix();
        }

        gl.PopMatrix();
        gl.EndList();
        return list;
    }

    private static void RecordStarFlower(LegacyGl gl, int detail)
    {
        gl.PushMatrix();
        RecordStem(gl, 0.05f, 5 + detail);
        RecordLeaves(gl, detail);
        gl.Translate(0, 1, 0);
        gl.PushMatrix();
        gl.Color(0.8f, 0.2f, 0.8f, 0.5f);
        gl.Sphere(0.25f, 5 + detail, 4 + detail);
        gl.Color(0.2f, 0.2f, 0.8f, 0.5f);
        gl.Scale(0.2f, 0.2f, 1);
        gl.Sphere(0.45f, 4, 3 + detail);
        gl.Scale(5, 1, 0.2f);
        gl.Sphere(0.45f, 4, 3 + detail);
        gl.Scale(0.2f, 5, 0.2f);
        gl.Sphere(0.45f, 4, 3 + detail);
        gl.PopMatrix();
        gl.PopMatrix();
    }

    private static void RecordSunflower(LegacyGl gl, int detail)
    {
        gl.PushMatrix();
        RecordStem(gl, 0.05f, 5 + detail);
        RecordLeaves(gl, detail);
        gl.Translate(0, 1, 0);
        gl.PushMatrix();
        gl.Scale(1, 0.2f, 1);
        gl.Color(0.4f, 0.4f, 0.10f, 0.5f);
        gl.Sphere(0.5f, 8 + (3 * detail), 5 + (2 * detail));
        gl.Scale(1, 5, 1);
        gl.Color(1.0f, 0.75f, 0.4f, 0.5f);
        gl.Sphere(0.2f, 5 + detail, 4 + detail);
        gl.PopMatrix();
        gl.PopMatrix();
    }

    private static void RecordStem(LegacyGl gl, float radius, int slices)
    {
        gl.PushMatrix();
        gl.Color(0.1f, 0.8f, 0.1f, 0.5f);
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(radius, radius, 1, slices, 1);
        gl.PopMatrix();
    }

    private static void RecordLeaves(LegacyGl gl, int detail)
    {
        gl.Color(0.1f, 0.6f, 0.1f, 0.5f);
        for (var i = 0; i < LeafCount; i++)
        {
            gl.PushMatrix();
            gl.Rotate((i * 120) + 30, 0, 1, 0);
            gl.Translate(0, 0.05f, -0.25f);
            gl.Rotate(45, 1, 0, 0);
            gl.Scale(0.5f, 0.15f, 1);
            gl.Sphere(0.6f, 5 + (2 * detail), 3 + (2 * detail));
            gl.PopMatrix();
        }
    }

    private static void RecordMushroomShadow(LegacyGl gl, int detail)
    {
        gl.PushMatrix();
        RecordShadowStem(gl, 0.1f);
        gl.Translate(0, 1, 0);
        gl.PushMatrix();
        gl.Rotate(-90, 1, 0, 0);
        gl.Disk(0, 0.5f, 10 + (2 * detail), 1);
        gl.PopMatrix();
        gl.PopMatrix();
    }

    private static void RecordDaisyShadow(LegacyGl gl, int detail)
    {
        gl.PushMatrix();
        RecordShadowStem(gl, 0.075f);
        gl.Translate(0, 1, 0);
        for (var i = 0; i < DaisyPetals; i++)
        {
            gl.PushMatrix();
            gl.Rotate(i * 60, 0, 1, 0);
            gl.Translate(0, 0.05f, -0.25f);
            gl.Rotate(25, 1, 0, 0);
            gl.Rotate(-90, 1, 0, 0);
            gl.Scale(0.5f, 1, 1);
            gl.Disk(0, 0.5f, 8 + (2 * detail), 1);
            gl.PopMatrix();
        }

        gl.PopMatrix();
    }

    private static void RecordStarFlowerShadow(LegacyGl gl, int detail)
    {
        gl.PushMatrix();
        RecordLeafShadows(gl);
        gl.Translate(0, 1, 0);
        gl.PushMatrix();
        gl.Rotate(-90, 1, 0, 0);
        gl.Disk(0, 0.25f, 6 + (2 * detail), 1);
        gl.Scale(1, 0.2f, 1.0f);
        gl.Disk(0, 0.45f, 4, 1);
        gl.Rotate(-90, 0, 1, 0);
        gl.Scale(0.2f, 5, 1);
        gl.Disk(0, 0.45f, 4, 1);
        gl.PopMatrix();
        gl.PopMatrix();
    }

    private static void RecordSunflowerShadow(LegacyGl gl, int detail)
    {
        gl.PushMatrix();
        RecordLeafShadows(gl);
        gl.Translate(0, 1, 0);
        gl.PushMatrix();
        gl.Scale(1, 0.2f, 1);
        gl.Rotate(-90, 1, 0, 0);
        gl.Disk(0, 0.5f, 10 + (2 * detail), 1);
        gl.PopMatrix();
        gl.PopMatrix();
    }

    private static void RecordShadowStem(LegacyGl gl, float radius)
    {
        gl.PushMatrix();
        gl.Rotate(-90, 1, 0, 0);
        gl.Cylinder(radius, radius, 1, 5, 1);
        gl.PopMatrix();
    }

    private static void RecordLeafShadows(LegacyGl gl)
    {
        for (var i = 0; i < LeafCount; i++)
        {
            gl.PushMatrix();
            gl.Rotate((i * 120) + 30, 0, 1, 0);
            gl.Translate(0, 0.05f, -0.25f);
            gl.Rotate(45, 1, 0, 0);
            gl.Rotate(-90, 1, 0, 0);
            gl.Disk(0, 0.6f, 3, 1);
            gl.PopMatrix();
        }
    }

    private static void RecordWindArrow(LegacyGl gl)
    {
        const float height = 17f;
        gl.Disable(GlCapability.Lighting);
        gl.Color(0, 0, 0);
        gl.Begin(GlPrimitive.Lines);
        gl.Vertex(0, height, 0);
        gl.Vertex(0, height, 1);
        gl.Vertex(0, height, 1);
        gl.Vertex(-0.2f, height, 0.8f);
        gl.Vertex(0, height, 1);
        gl.Vertex(0.2f, height, 0.8f);
        gl.End();
        gl.Enable(GlCapability.Lighting);
    }
}
