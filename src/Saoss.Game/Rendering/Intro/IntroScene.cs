using Microsoft.Xna.Framework.Graphics;
using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;
using Saoss.Game.Rendering.Ships;

namespace Saoss.Game.Rendering.Intro;

/// <summary>
/// Draws the intro scene, a port of lippu.cpp (<c>Init</c>, <c>Display</c>, <c>Reshape</c>, <c>draw</c>): a circling
/// camera over a round meadow with the credits written on the ground, a cloth flag waving on a pole, mushrooms and
/// flowers, a few ships, and flat blended shadows of all of them drawn once per pixel with the stencil buffer.
/// </summary>
public sealed class IntroScene : IDisposable
{
    private const float FieldOfViewDegrees = 65f;
    private const float Aspect = 1f;
    private const float NearPlane = 0.1f;
    private const float FarPlane = 100f;
    private const float CameraHeight = 7.5f;
    private const float RopeOffset = 0.2f;
    private const float PoleRopeRadius = 0.17f;
    private const float ShadowRopeRadius = 0.1f;
    private const float FlagShadowShift = 0.4f;
    private const float FlagShadowFlatten = 0.01f;
    private const float ShadowHeight = 0.02f;
    private const float ShadowFlatten = 0.01f;
    private const float ShadowLeanDegrees = 35f;

    private readonly GraphicsDevice _device;
    private readonly LegacyGl _gl;
    private readonly SceneTextures _textures;
    private readonly IntroModels _models;
    private readonly GlText _text;
    private readonly IntroShips _ships;
    private readonly int _detail;

    /// <summary>
    /// Loads the textures, records the scene's and the ships' display lists at <paramref name="detail"/> (0-3) and sets
    /// up the intro's lighting [Init].
    /// </summary>
    public IntroScene(GraphicsDevice device, int detail, Random random)
    {
        _device = device;
        _detail = detail;
        _gl = new LegacyGl(device);
        _textures = new SceneTextures(device);
        _models = new IntroModels(_gl, _textures.Ground, detail);
        _text = new GlText(_gl, _textures.Font);
        var shipModels = new ShipModels(_gl, new SharedModels(_gl, detail), detail);
        _ships = new IntroShips(_gl, new ShipModelRenderer(_gl, _textures, shipModels), random);
        InitializeState();
    }

    /// <summary>
    /// Squashes what follows onto the ground just above it, leaning away from the light: the matrix the original used
    /// to turn plants into their shadows [varjoOperaatio].
    /// </summary>
    public static void FlattenOntoGround(LegacyGl gl)
    {
        gl.Translate(0.0f, ShadowHeight, 0.0f);
        gl.Scale(1.0f, ShadowFlatten, 1.0f);
        gl.Rotate(ShadowLeanDegrees, 0, 0, 1);
    }

    /// <summary>Shows or hides the ships' exhaust parts (see <see cref="IntroShips.SetThrust"/>).</summary>
    public void SetThrust(bool thrust) => _ships.SetThrust(thrust);

    /// <summary>
    /// Draws intro frame <paramref name="frame"/> with the flag's current shape [Display, draw]. Steps: clear and set
    /// the projection, place the camera and light, draw the ground and its text, pole and flag, plants, ships, flag
    /// ropes, then all shadows, and leave the state as the original did.
    /// </summary>
    public void Draw(int frame, ClothFlag flag)
    {
        var gl = _gl;
        gl.Clear(includeStencil: true);
        SetProjection();
        gl.LoadIdentity();
        gl.MatrixMode(GlMatrixMode.ModelView);
        gl.PushMatrix();
        PlaceCamera(frame);

        gl.Enable(GlCapability.Texture2D);
        gl.PushMatrix();
        gl.CallList(_models.Ground);
        gl.PopMatrix();
        DrawGroundText();
        gl.PushMatrix();
        gl.CallList(_models.Pole);
        gl.PopMatrix();
        gl.Disable(GlCapability.Texture2D);
        DrawFlag(flag);

        gl.Enable(GlCapability.CullFace);
        DrawPlants(shadows: false);
        _ships.Draw(frame);
        RestoreAfterShips();
        DrawRopes(flag);

        DrawShadows(flag);
        RestoreAtEnd();
        gl.Flush();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _gl.Dispose();
        _textures.Dispose();
    }

    private void InitializeState()
    {
        var gl = _gl;
        gl.LightAmbient(0, 0.2f, 0.2f, 0.2f, 1.0f);
        gl.LightDiffuse(0, 0.9f, 0.9f, 0.9f, 1.0f);
        gl.LightSpecular(0, 1.0f, 1.0f, 1.0f, 1.0f);
        gl.Enable(GlCapability.DepthTest);
        gl.ClearColor(0.4f, 0.4f, 0.8f, 1.0f);
        gl.Enable(GlCapability.Lighting);
        gl.Enable(GlCapability.ColorMaterial);
        gl.LightModelLocalViewer(true);

        // The game enables GL_NORMALIZE only after the intro, so scaled models are lit with scaled normals here.
        gl.Disable(GlCapability.Normalize);
    }

    // The original's square aspect stretches the scene to the window's shape [Reshape].
    private void SetProjection()
    {
        var parameters = _device.PresentationParameters;
        _gl.Viewport(0, 0, parameters.BackBufferWidth, parameters.BackBufferHeight);
        _gl.MatrixMode(GlMatrixMode.Projection);
        _gl.LoadIdentity();
        _gl.Perspective(FieldOfViewDegrees, Aspect, NearPlane, FarPlane);
        _gl.MatrixMode(GlMatrixMode.ModelView);
        _gl.LoadIdentity();
    }

    // The camera sways around the pole and in and out on a mix of slow waves; the light is fixed in the scene.
    private void PlaceCamera(int frame)
    {
        var distance = 20 + (MathF.Cos(frame / 574.0f) * 4.0f);
        var tilt = (MathF.Sin(frame / 400.0f) * 18.0f) + (MathF.Cos(frame / 150.0f) * 8.0f) + 15.0f;
        var turn = (MathF.Cos(frame / 600.0f) * 50.0f) + (MathF.Sin(frame / 100.0f) * 10.0f);
        _gl.Translate(0.0f, -CameraHeight, -distance);
        _gl.Rotate(tilt, 1.0f, 0.0f, 0.0f);
        _gl.Rotate(turn, 0.0f, 1.0f, 0.0f);
        _gl.LightPosition(0, 7.0f, 10.0f, 0.0f, 0.0f);
        _gl.Enable(GlCapability.Light0);
    }

    // Unlit and untested, multiplying the ground by the font's inverted glyphs (the ground list left GL_BLEND texturing on).
    private void DrawGroundText()
    {
        var gl = _gl;
        gl.PushMatrix();
        gl.Disable(GlCapability.Lighting);
        gl.Enable(GlCapability.Texture2D);
        gl.Disable(GlCapability.DepthTest);
        gl.Color(1, 1, 1, 0);
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.Zero, GlBlend.SrcColor);
        gl.Rotate(-90, 1, 0, 0);
        _text.Println(-13, 22, 0, "TGO &", 1.1f);
        _text.Println(-13, 18, 0, "Cinere", 1.1f);
        _text.Println(-5, 20, 0, "/ THP Presents", 1.1f);
        _text.Println(-5, 15, 0, "Saoss", 1.6f);
        gl.Enable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Lighting);
        gl.Disable(GlCapability.Blend);
        gl.Enable(GlCapability.Texture2D);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcAlpha);
        gl.Disable(GlCapability.CullFace);
        gl.PopMatrix();
    }

    // The title picture on the cloth, lit from both sides with the averaged vertex normals.
    private void DrawFlag(ClothFlag flag)
    {
        var gl = _gl;
        gl.Color(0.9f, 1.0f, 0.9f);
        gl.Enable(GlCapability.Texture2D);
        gl.TexEnv(GlTexEnv.Modulate);
        gl.BindTexture(_textures.Title);
        gl.LightModelTwoSide(true);
        EmitFlagTriangles(flag);
        gl.LightModelTwoSide(false);
        gl.Disable(GlCapability.Texture2D);
    }

    private void EmitFlagTriangles(ClothFlag flag)
    {
        var gl = _gl;
        var corners = flag.TriangleCorners;
        gl.Begin(GlPrimitive.Triangles);
        for (var i = 0; i < corners.Count; i++)
        {
            var vertex = corners[i];
            var normal = flag.Normals[vertex];
            var texCoord = flag.TexCoords[vertex];
            var position = flag.Positions[vertex];
            gl.Normal(normal.X, normal.Y, normal.Z);
            gl.TexCoord(texCoord.X, texCoord.Y);
            gl.Vertex(position.X, position.Y, position.Z);
        }

        gl.End();
    }

    private void DrawPlants(bool shadows)
    {
        var gl = _gl;
        gl.Scale(IntroPlantLayout.Scale, IntroPlantLayout.Scale, IntroPlantLayout.Scale);
        foreach (var group in IntroPlantLayout.Groups)
        {
            if (_detail < group.MinDetail)
            {
                continue;
            }

            gl.PushMatrix();
            foreach (var (x, z, plant) in group.Steps)
            {
                gl.Translate(x, 0, z);
                if (shadows)
                {
                    gl.PushMatrix();
                    FlattenOntoGround(gl);
                    gl.CallList(ShadowOf(plant));
                    gl.PopMatrix();
                }
                else
                {
                    gl.CallList(ModelOf(plant));
                }
            }

            gl.PopMatrix();
        }

        gl.Scale(1 / IntroPlantLayout.Scale, 1 / IntroPlantLayout.Scale, 1 / IntroPlantLayout.Scale);
    }

    private GlDisplayList ModelOf(IntroPlant plant) => plant switch
    {
        IntroPlant.Mushroom => _models.Mushroom,
        IntroPlant.BlueDaisy => _models.BlueDaisy,
        IntroPlant.RedDaisy => _models.RedDaisy,
        IntroPlant.StarFlower => _models.StarFlower,
        _ => _models.Sunflower,
    };

    private GlDisplayList ShadowOf(IntroPlant plant) => plant switch
    {
        IntroPlant.Mushroom => _models.MushroomShadow,
        IntroPlant.BlueDaisy or IntroPlant.RedDaisy => _models.DaisyShadow,
        IntroPlant.StarFlower => _models.StarFlowerShadow,
        _ => _models.SunflowerShadow,
    };

    // Two-sided lighting stays on into the next frame, until the flag turns it off again.
    private void RestoreAfterShips()
    {
        var gl = _gl;
        gl.TexEnv(GlTexEnv.Modulate);
        gl.BindTexture(_textures.Title);
        gl.LightModelTwoSide(true);
        gl.Disable(GlCapability.Texture2D);
        gl.Enable(GlCapability.CullFace);
        gl.Enable(GlCapability.DepthTest);
        gl.Disable(GlCapability.Blend);
    }

    // Ropes from the pole top to the flag's left corners and down to the pole foot.
    private void DrawRopes(ClothFlag flag)
    {
        var gl = _gl;
        var top = flag.Positions[ClothFlag.TopLeft];
        var bottom = flag.Positions[ClothFlag.BottomLeft];
        var angle = MathF.Atan2(bottom.X, bottom.Z);
        gl.Disable(GlCapability.Lighting);
        gl.Begin(GlPrimitive.LineStrip);
        gl.Color(0.9f, 0.8f, 0.7f);
        gl.Vertex(MathF.Sin(angle) * PoleRopeRadius, ClothFlag.PoleHeight - RopeOffset, MathF.Cos(angle) * PoleRopeRadius);
        gl.Vertex(top.X, top.Y, top.Z);
        gl.Vertex(bottom.X, bottom.Y, bottom.Z);
        gl.Vertex(MathF.Sin(angle) * PoleRopeRadius, 0, MathF.Cos(angle) * PoleRopeRadius);
        gl.End();
    }

    /// <summary>
    /// Draws every shadow as the object squashed flat onto the ground, darkening the ground by half (the shapes' alpha)
    /// and marking the stencil buffer so overlapping shadows darken only once. Steps: pole, flag and ropes laid along -X;
    /// plants leaning away from the light; the Stick Insect.
    /// </summary>
    private void DrawShadows(ClothFlag flag)
    {
        var gl = _gl;
        gl.Enable(GlCapability.StencilTest);
        gl.StencilFunc(GlCompare.Greater, 1, 1);
        gl.StencilOp(GlStencilOp.Keep, GlStencilOp.Keep, GlStencilOp.Replace);
        gl.Enable(GlCapability.PolygonOffsetFill);
        gl.PolygonOffset(0, -1);

        gl.PushMatrix();
        gl.Disable(GlCapability.CullFace);
        gl.Rotate(90, 0, 0, 1);
        gl.Scale(0.1f, 7.0f / 10.0f, 1);
        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.Texture2D);
        gl.BlendFunc(GlBlend.Zero, GlBlend.OneMinusSrcAlpha);
        gl.CallList(_models.Pole);
        gl.Translate(FlagShadowShift, 0, 0);
        gl.Scale(FlagShadowFlatten, 1.0f, 1);
        EmitFlagTriangles(flag);
        gl.Enable(GlCapability.CullFace);
        DrawShadowRopes(flag);
        gl.PopMatrix();

        DrawPlants(shadows: true);
        if (_detail > 0)
        {
            _ships.DrawStickInsectShadow();
        }

        gl.Disable(GlCapability.PolygonOffsetFill);
        gl.Disable(GlCapability.StencilTest);
    }

    private void DrawShadowRopes(ClothFlag flag)
    {
        var gl = _gl;
        var top = flag.Positions[ClothFlag.TopLeft];
        var bottom = flag.Positions[ClothFlag.BottomLeft];
        var angle = MathF.Atan2(bottom.X, bottom.Z);
        gl.Disable(GlCapability.Lighting);
        gl.Begin(GlPrimitive.LineStrip);
        gl.Color(0.9f, 0.8f, 0.7f, 0.5f);
        gl.Vertex(0, ClothFlag.PoleHeight, 0);
        gl.Vertex(top.X, top.Y, top.Z);
        gl.Vertex(bottom.X, bottom.Y, bottom.Z);
        gl.Vertex(MathF.Sin(angle) * ShadowRopeRadius, 0, MathF.Cos(angle) * ShadowRopeRadius);
        gl.End();
    }

    private void RestoreAtEnd()
    {
        var gl = _gl;
        gl.PopMatrix();
        gl.Disable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Texture2D);
        gl.BlendFunc(GlBlend.SrcAlpha, GlBlend.OneMinusSrcAlpha);
        gl.Enable(GlCapability.DepthTest);
        gl.Disable(GlCapability.Blend);
        gl.Disable(GlCapability.Texture2D);
        gl.Disable(GlCapability.CullFace);
        gl.Enable(GlCapability.Lighting);
    }
}
