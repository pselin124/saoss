using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Saoss.Game.Rendering;

namespace Saoss.Game.Screens;

/// <summary>
/// The shared look of the original 3D menus (<c>drawmenu</c>, <c>drawOnePlayerGameMenu</c>): the title picture,
/// a translucent black panel, red bitmap-font text and rotating 3D arrows next to the selection.
/// Call <see cref="Begin"/> first in every frame, then draw the panel, the text and the arrows in that order.
/// </summary>
public sealed class MenuScene : IDisposable
{
    /// <summary>Distance of the camera from the arrow plane.</summary>
    public const float CameraZ = 15f;

    /// <summary>Depth of the menu text.</summary>
    public const float TextZ = -40f;

    /// <summary>How far the arrows turn per update.</summary>
    public const float ArrowSpinDegreesPerTick = 2.5f;

    private const float BackgroundZ = -1.8f;
    private const float BackgroundHalfWidth = 1f;
    private const float BackgroundHalfHeight = 0.75f;
    private const float PanelZ = 10f;
    private const float ArrowTopY = 1f;
    private const float ArrowLineSpacing = 0.55f;

    private static readonly Vector3 SceneOffset = new(0.1f, 0.1f, 0.01f);
    private static readonly Color TextColor = Color.Red;
    private static readonly Color PanelColor = new(0f, 0f, 0f, 0.6f);

    private readonly GraphicsDevice _graphicsDevice;
    private readonly GameAssets _assets;
    private readonly BasicEffect _effect;
    private readonly VertexPositionColorTexture[] _quad = new VertexPositionColorTexture[6];

    /// <summary>Creates the drawing resources.</summary>
    public MenuScene(GraphicsDevice graphicsDevice, GameAssets assets)
    {
        _graphicsDevice = graphicsDevice;
        _assets = assets;
        _effect = new BasicEffect(graphicsDevice);
    }

    /// <summary>Height of the arrows for the menu entry at <paramref name="selection"/>.</summary>
    public static float ArrowY(int selection) => ArrowTopY - (ArrowLineSpacing * selection);

    /// <summary>Advances the arrow rotation by one update, keeping it within -180..180 degrees.</summary>
    public static float AdvanceSpin(float spinDegrees)
    {
        spinDegrees += ArrowSpinDegreesPerTick;
        return spinDegrees > 180f ? spinDegrees - 360f : spinDegrees;
    }

    /// <summary>
    /// Clears the screen, draws the title picture unless <paramref name="withBackground"/> is false, and sets up
    /// the menu camera.
    /// </summary>
    public void Begin(bool withBackground = true)
    {
        var device = _graphicsDevice;
        device.Clear(Color.Black);
        device.RasterizerState = LegacyGraphics.NoCulling;
        device.SamplerStates[0] = SamplerState.LinearClamp;
        _effect.Projection = LegacyGraphics.CreateProjection(device.Viewport);
        if (withBackground)
        {
            DrawBackground();
        }

        _effect.View = Matrix.CreateLookAt(new Vector3(0, 0, CameraZ), Vector3.Zero, Vector3.Up);
    }

    /// <summary>Draws the translucent black panel behind the text, given its corners on the panel plane.</summary>
    public void DrawPanel(float left, float bottom, float right, float top)
    {
        _effect.World = Matrix.Identity;
        _effect.LightingEnabled = false;
        _effect.TextureEnabled = false;
        _effect.VertexColorEnabled = true;
        _graphicsDevice.BlendState = BlendState.AlphaBlend;
        _graphicsDevice.DepthStencilState = DepthStencilState.None;
        DrawQuad(new Vector3(left, bottom, PanelZ), new Vector3(right, top, PanelZ), PanelColor);
    }

    /// <summary>Draws red menu text at <see cref="TextZ"/> (gl_println after the menus' small scene offset).</summary>
    public void DrawText(float x, float y, string text) => DrawText(new Vector3(x, y, TextZ) + SceneOffset, text, TextColor);

    /// <summary>Draws text at exactly <paramref name="position"/> in <paramref name="color"/> (gl_println without offset).</summary>
    public void DrawText(Vector3 position, string text, Color color)
    {
        _graphicsDevice.BlendState = LegacyGraphics.TextBlend;
        _graphicsDevice.DepthStencilState = DepthStencilState.None;
        _assets.Font.Draw(_effect, Matrix.CreateTranslation(position), text, color);
    }

    /// <summary>
    /// Draws a lit arrow turning around its length at (<paramref name="x"/>, <paramref name="y"/>) on the arrow plane.
    /// The arrow points right, or left when <paramref name="pointsLeft"/> is set. The settings menus place it
    /// without the other menus' small scene offset (<paramref name="withSceneOffset"/> false).
    /// </summary>
    public void DrawArrow(float x, float y, Vector3 scale, float spinDegrees, bool pointsLeft, bool withSceneOffset = true)
    {
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        LegacyGraphics.ConfigureLitVertexColors(_effect, lightDirection: -Vector3.UnitZ, diffuse: 0.5f, specularPower: 120f);
        var turn = pointsLeft ? Matrix.CreateRotationZ(MathHelper.Pi) : Matrix.Identity;
        var offset = withSceneOffset ? SceneOffset : Vector3.Zero;
        _effect.World = Matrix.CreateScale(scale)
            * Matrix.CreateRotationX(MathHelper.ToRadians(spinDegrees))
            * turn
            * Matrix.CreateTranslation(new Vector3(x, y, 0f) + offset);
        _assets.MenuArrow.Draw(_effect);
    }

    /// <inheritdoc/>
    public void Dispose() => _effect.Dispose();

    // The 4:3 picture sits just beyond the near plane, so it is pillarboxed on wide windows like in the original.
    private void DrawBackground()
    {
        _effect.View = Matrix.Identity;
        _effect.World = Matrix.Identity;
        _effect.LightingEnabled = false;
        _effect.VertexColorEnabled = true;
        _effect.TextureEnabled = true;
        _effect.Texture = _assets.TitleBackground;
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.None;
        DrawQuad(new Vector3(-BackgroundHalfWidth, -BackgroundHalfHeight, BackgroundZ), new Vector3(BackgroundHalfWidth, BackgroundHalfHeight, BackgroundZ), Color.White);
    }

    private void DrawQuad(Vector3 bottomLeft, Vector3 topRight, Color color)
    {
        var bottomRight = new Vector3(topRight.X, bottomLeft.Y, bottomLeft.Z);
        var topLeft = new Vector3(bottomLeft.X, topRight.Y, topRight.Z);
        _quad[0] = new VertexPositionColorTexture(bottomLeft, color, new Vector2(0, 1));
        _quad[1] = new VertexPositionColorTexture(bottomRight, color, new Vector2(1, 1));
        _quad[2] = new VertexPositionColorTexture(topRight, color, new Vector2(1, 0));
        _quad[3] = _quad[0];
        _quad[4] = _quad[2];
        _quad[5] = new VertexPositionColorTexture(topLeft, color, new Vector2(0, 0));
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, _quad, 0, 2);
        }
    }
}
