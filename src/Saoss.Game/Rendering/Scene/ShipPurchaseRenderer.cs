using System.Globalization;
using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Draws the full-screen ship purchase menu of a credits game (drawAlusValinta): the highlighted ship turning
/// on the left (or "random ship"), the player's remaining credits, the ship's number, name and price, and its stats.
/// </summary>
public sealed class ShipPurchaseRenderer(LegacyGl gl, SceneTextures textures, ShipRenderer ships)
{
    private const float FieldOfView = 45f;
    private const float NearPlane = 1f;
    private const float FarPlane = 100f;
    private const float CameraZ = 15f;
    private const float PreviewScale = 0.5f;
    private const float PreviewHeading = 90f;
    private const float StatLabelX = -5f;
    private const float StatValueX = 12f;
    private const float StatTopY = -5f;
    private const float StatLineSpacing = 3f;
    private const float InfoZ = -40f;
    private const float HeaderZ = -25f;
    private const float SpeedDisplayScale = 1000f;
    private const float InertiaFreeSpeedDivisor = 100f;
    private const string Unknown = "x";

    private readonly GlText _text = new(gl, textures.Font);
    private readonly GameRandom _previewRandom = new(0);
    private Ship? _preview;

    /// <summary>
    /// Draws the menu for <paramref name="buyer"/> with entry <paramref name="selection"/> highlighted
    /// (<see cref="GameConstants.ShipTypeCount"/> is the random entry). <paramref name="tick"/> turns the ship.
    /// </summary>
    public void Draw(Ship buyer, int selection, int tick, SceneState state)
    {
        var definition = selection < ShipCatalog.All.Count ? ShipCatalog.All[selection] : null;
        gl.MatrixMode(GlMatrixMode.Projection);
        gl.LoadIdentity();
        gl.Perspective(FieldOfView, (float)gl.ViewportWidth / gl.ViewportHeight, NearPlane, FarPlane);
        gl.MatrixMode(GlMatrixMode.ModelView);
        gl.Disable(GlCapability.Lighting);
        gl.Disable(GlCapability.Blend);
        gl.Enable(GlCapability.Texture2D);
        gl.Enable(GlCapability.DepthTest);
        gl.Clear();
        gl.LoadIdentity();
        gl.LightPosition(0, 0, 0, 0, 1);
        gl.LookAt(0, 0, CameraZ, 0, 0, 0, 0, 1, 0);
        DrawPreview(buyer, definition, tick, state);

        gl.Enable(GlCapability.Blend);
        gl.Disable(GlCapability.DepthTest);
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        gl.Color(1, 0, 0, 1);
        DrawHeader(buyer, selection, definition);
        DrawStats(definition);
        gl.Flush();
    }

    // The ship turns on a tilted plane on the left; the random entry shows slanted text instead.
    private void DrawPreview(Ship buyer, ShipDefinition? definition, int tick, SceneState state)
    {
        gl.PushMatrix();
        if (definition == null)
        {
            // Drawn with blending still off and the color left from the previous frame's text.
            gl.Color(1, 0, 0, 1);
            gl.Rotate(-45, 0, 0, 1);
            _text.Println(-17, -22, -45, "random ship");
            gl.PopMatrix();
            return;
        }

        gl.Rotate(-60, 1, 0, 0);
        gl.Translate(-5, -2, -1.5f);
        gl.Rotate(tick, 0, 0, 1);
        gl.Scale(PreviewScale, PreviewScale, PreviewScale);
        ships.DrawPreview(PreviewShip(buyer, definition), state);
        gl.PopMatrix();
    }

    // A stand-in ship of the buyer's team with the highlighted type, at the origin facing up.
    private Ship PreviewShip(Ship buyer, ShipDefinition definition)
    {
        if (_preview is { } cached && cached.Index == buyer.Index && cached.Team == buyer.Team && cached.Definition == definition)
        {
            return cached;
        }

        var preview = StandInShips.Create(buyer, definition, 0, 0, PreviewHeading, _previewRandom);
        _preview = preview;
        return preview;
    }

    // "player N / choose your ship", remaining credits, ship number and name, and price.
    // The random entry's name slot read past the end of the original ship table; it is left empty.
    private void DrawHeader(Ship buyer, int selection, ShipDefinition? definition)
    {
        _text.Println(-5, 13, HeaderZ, "player");
        _text.Println(3, 13, HeaderZ, Number(buyer.Index + 1));
        _text.Println(-10, 8, HeaderZ, "choose your ship");
        _text.Println(-25, 6, InfoZ, "remaining credits:");
        _text.Println(-5, 6, InfoZ, Number((int)buyer.Score));
        _text.Println(-25, 3.5f, InfoZ, "ship number:");
        _text.Println(-10, 3.5f, InfoZ, definition == null ? Unknown : Number(selection + 1));
        if (definition != null)
        {
            _text.Println(-5, 3.5f, InfoZ, definition.Name);
        }

        _text.Println(-25, 1, InfoZ, "ship cost:");
        _text.Println(-13, 1, InfoZ, definition == null ? Unknown : Number(definition.Cost));
    }

    private void DrawStats(ShipDefinition? definition)
    {
        var speed = definition == null ? 0f : definition.Thrust * SpeedDisplayScale;
        if (definition?.IsInertiaFree == true)
        {
            speed /= InertiaFreeSpeedDivisor;
        }

        (string Label, string Value)[] rows =
        [
            ("energy:", definition == null ? Unknown : Number((int)definition.MaxEnergy)),
            ("damage:", Decimal(definition?.Primary.Damage)),
            ("special damage:", Decimal(definition?.Special.Damage)),
            ("speed:", Decimal(definition == null ? null : speed)),
            ("turning speed:", Decimal(definition?.TurnRate)),
            ("weight:", Decimal(definition?.Mass)),
        ];
        for (var i = 0; i < rows.Length; i++)
        {
            var y = StatTopY - (i * StatLineSpacing);
            _text.Println(StatLabelX, y, InfoZ, rows[i].Label);
            _text.Println(StatValueX, y, InfoZ, rows[i].Value);
        }
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Decimal(float? value) => value is { } v ? v.ToString("0.00", CultureInfo.InvariantCulture) : Unknown;
}
