using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Game.Rendering;

namespace Saoss.Game.Screens;

/// <summary>
/// The first menu: title picture, menu items in the bitmap font and rotating 3D arrows around the selection.
/// Layout values come from the original <c>drawmenu</c>.
/// </summary>
public sealed class MainMenuScreen : IScreen, IDisposable
{
    private const int QuitIndex = 4;
    private const float ArrowSpinDegreesPerTick = 2.5f;

    // Camera and layer depths of the original menu scene.
    private const float CameraZ = 15f;
    private const float BackgroundZ = -1.8f;
    private const float BackgroundHalfWidth = 1f;
    private const float BackgroundHalfHeight = 0.75f;
    private const float TextX = -20f;
    private const float TextTopY = 4f;
    private const float TextLineSpacing = 2f;
    private const float TextZ = -40f;
    private const float ArrowLeftX = -6.5f;
    private const float ArrowTopY = 1f;
    private const float ArrowLineSpacing = 0.55f;
    private const float ArrowGapAfterText = 0.4f;

    private static readonly string[] Items = ["Local Game", "Host Multiplayer", "Join Multiplayer", "Settings", "Quit"];
    private static readonly Vector3 SceneOffset = new(0.1f, 0.1f, 0.01f);
    private static readonly Vector3 ArrowScale = new(0.22f, 0.3f, 0.3f);
    private static readonly Color TextColor = Color.Red;
    private static readonly Color PanelColor = new(0f, 0f, 0f, 0.6f);

    private readonly GraphicsDevice _graphicsDevice;
    private readonly GameAssets _assets;
    private readonly Action _quit;
    private readonly BasicEffect _effect;
    private readonly VertexPositionColorTexture[] _quad = new VertexPositionColorTexture[6];
    private int _selection;
    private float _arrowSpinDegrees;

    /// <summary>Creates the menu. <paramref name="quit"/> is called when the player chooses Quit.</summary>
    public MainMenuScreen(GraphicsDevice graphicsDevice, GameAssets assets, Action quit)
    {
        _graphicsDevice = graphicsDevice;
        _assets = assets;
        _quit = quit;
        _effect = new BasicEffect(graphicsDevice);
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        _arrowSpinDegrees += ArrowSpinDegreesPerTick;
        if (_arrowSpinDegrees > 180f)
        {
            _arrowSpinDegrees -= 360f;
        }

        if (keyboard.WasPressed(Keys.Up))
        {
            _selection = _selection > 0 ? _selection - 1 : Items.Length - 1;
        }
        else if (keyboard.WasPressed(Keys.Down))
        {
            _selection = _selection < Items.Length - 1 ? _selection + 1 : 0;
        }
        else if (keyboard.WasPressed(Keys.Escape))
        {
            _selection = QuitIndex;
            _quit();
        }
        else if (keyboard.WasPressed(Keys.Enter) && _selection == QuitIndex)
        {
            _quit();
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        var device = _graphicsDevice;
        device.Clear(Color.Black);
        device.RasterizerState = LegacyGraphics.NoCulling;
        device.SamplerStates[0] = SamplerState.LinearClamp;
        _effect.Projection = LegacyGraphics.CreateProjection(device.Viewport);

        DrawBackground();

        _effect.View = Matrix.CreateLookAt(new Vector3(0, 0, CameraZ), Vector3.Zero, Vector3.Up);
        DrawPanel();
        DrawItems();
        DrawArrows();
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

    private void DrawPanel()
    {
        _effect.World = Matrix.Identity;
        _effect.LightingEnabled = false;
        _effect.TextureEnabled = false;
        _effect.VertexColorEnabled = true;
        _graphicsDevice.BlendState = BlendState.AlphaBlend;
        DrawQuad(new Vector3(-2f, -0.5f, 10f), new Vector3(-0.1f, 0.5f, 10f), PanelColor);
    }

    private void DrawItems()
    {
        _graphicsDevice.BlendState = LegacyGraphics.TextBlend;
        for (var i = 0; i < Items.Length; i++)
        {
            var world = Matrix.CreateTranslation(new Vector3(TextX, TextTopY - (i * TextLineSpacing), TextZ) + SceneOffset);
            _assets.Font.Draw(_effect, world, Items[i], TextColor);
        }
    }

    private void DrawArrows()
    {
        _graphicsDevice.BlendState = BlendState.Opaque;
        _graphicsDevice.DepthStencilState = DepthStencilState.Default;
        LegacyGraphics.ConfigureLitVertexColors(_effect, lightDirection: -Vector3.UnitZ, diffuse: 0.5f, specularPower: 120f);

        var y = ArrowTopY - (ArrowLineSpacing * _selection);
        var spin = Matrix.CreateRotationX(MathHelper.ToRadians(_arrowSpinDegrees));
        var scale = Matrix.CreateScale(ArrowScale);
        var offset = Matrix.CreateTranslation(SceneOffset);

        _effect.World = scale * spin * Matrix.CreateTranslation(ArrowLeftX, y, 0f) * offset;
        _assets.MenuArrow.Draw(_effect);

        // Place the right arrow just after the text by projecting its end from text depth to arrow depth.
        var textDistance = CameraZ - TextZ;
        var textEnd = TextX + GlyphMetrics.MeasureWidth(Items[_selection]);
        var rightX = (textEnd * CameraZ / textDistance) + ArrowGapAfterText;
        _effect.World = scale * spin * Matrix.CreateRotationZ(MathHelper.Pi) * Matrix.CreateTranslation(rightX, y, 0f) * offset;
        _assets.MenuArrow.Draw(_effect);
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
