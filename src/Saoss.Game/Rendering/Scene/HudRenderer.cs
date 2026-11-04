using System.Globalization;
using Saoss.Core.Simulation;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// A player's HUD block: name and team in the team color with shield and fuel bars, placed in a corner
/// or along the screen edges (Player::piirraGraffat). Expects an identity view moved 30 units forward.
/// </summary>
public sealed class HudRenderer(LegacyGl gl, SceneTextures textures)
{
    private const float LeftX = -14.5f;
    private const float RightX = 12.5f;
    private const float TextSize = 0.3f;
    private const float CreditsTextSize = 0.25f;
    private const float CreditsY = -0.6f;
    private const float NameX = -1f;
    private const float TeamX = 2f;
    private const float LabelY = 1f;
    private const float ReferenceScreenWidth = 512f;
    private const float BarColorExponent = 1.7f;
    private const float BarAlpha = 0.8f;

    private readonly GlText _text = new(gl, textures.Font);

    /// <summary>
    /// Draws the HUD block of <paramref name="ship"/>; in a credits game it includes the credits line. A network
    /// game shows <paramref name="nickname"/> instead of "player N".
    /// </summary>
    public void Draw(Ship ship, World world, string? nickname = null)
    {
        gl.PushMatrix();
        gl.Enable(GlCapability.Blend);

        // glDisable(GL_DEPTH) followed here: GL_DEPTH is not a capability, so the call failed and had no effect.
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        gl.Disable(GlCapability.Lighting);
        if (Position(ship.Index, world.Ships.Count) is var (x, y))
        {
            gl.Translate(x, y, 0);
        }

        // Smaller on wider windows, as in the original.
        var scale = MathF.Sqrt(1f / (gl.ViewportWidth / ReferenceScreenWidth));
        gl.Scale(scale, scale, scale);
        DrawLabels(ship, nickname);
        if (world.Settings.UsesCredits)
        {
            DrawCredits(ship, world.Settings.Credits);
        }
        gl.Translate(2f, 0, 0);
        gl.Disable(GlCapability.Texture2D);
        DrawBars(ship);
        gl.Color(0.2f, 0.5f, 0.2f, 0.5f);
        gl.Enable(GlCapability.Texture2D);
        gl.PopMatrix();
    }

    // Players 1-4 take the corners and 5-10 the left and right edges; with 11 or more players the blocks
    // are stacked down both sides. Slots 10+ in a game of fewer than 11 players had no position.
    private static (float X, float Y)? Position(int index, int playerCount)
    {
        if (playerCount >= 11)
        {
            return index < 8 ? (LeftX, 10f - (index * 2.85f)) : (RightX, 10f - ((index - 8) * 2.85f));
        }

        return index switch
        {
            0 => (LeftX, 10f),
            1 => (RightX, 10f),
            2 => (LeftX, -10f),
            3 => (RightX, -10f),
            4 => (LeftX, 6.5f),
            5 => (LeftX, 3f),
            6 => (LeftX, -2.5f),
            7 => (LeftX, -6f),
            8 => (RightX, 6.5f),
            9 => (RightX, -6f),
            _ => null,
        };
    }

    // Network games showed the nickname instead of "player N"; "team N" moves right after a long name.
    private void DrawLabels(Ship ship, string? nickname)
    {
        SceneDrawing.TeamColor(gl, ship.Team);
        var name = nickname ?? $"player {Number(ship.Index + 1)}";
        _text.Println(NameX, LabelY, 0, name, TextSize);
        var teamX = MathF.Max(TeamX, NameX + GlyphMetrics.MeasureWidth(name + " ", TextSize));
        _text.Println(teamX, LabelY, 0, $"team {Number(ship.Team + 1)}", TextSize);
        _text.Println(-1f, 0.33f, 0, "shield", TextSize);
        _text.Println(-0.4f, -0.125f, 0, "fuel", TextSize);
    }

    // "credits 391 / 400", the remaining credits truncated to a whole number.
    private void DrawCredits(Ship ship, int startingCredits) =>
        _text.Println(-1.5f, CreditsY, 0, $"credits {Number((int)ship.Score)} / {Number(startingCredits)}", CreditsTextSize);

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    // Dim frames behind the bars, then bars that fade from green to red as they empty.
    private void DrawBars(Ship ship)
    {
        var definition = ship.Definition;

        // The original clamped the ship's energy to zero here; only the drawn value is clamped now.
        var shield = MathF.Max(ship.Energy, 0f) / definition.MaxEnergy;
        var fuel = ship.Fuel / definition.MaxFuel;

        gl.Color(0.25f, 0.25f, 0f, 0.5f);
        gl.Begin(GlPrimitive.Quads);
        BarQuad(-1.05f, 1.05f, 0.20f, 0.46f);
        BarQuad(-1.05f, 1.05f, -0.25f, 0.01f);
        gl.End();

        gl.Begin(GlPrimitive.Quads);
        gl.Color(1f - MathF.Pow(shield, BarColorExponent), shield, 0f, BarAlpha);
        BarQuad(-1f, -1f + (shield * 2f), 0.23f, 0.43f);
        gl.Color(1f - MathF.Pow(fuel, BarColorExponent), fuel, 0f, BarAlpha);
        BarQuad(-1f, -1f + (fuel * 2f), -0.22f, -0.02f);
        gl.End();
    }

    private void BarQuad(float left, float right, float bottom, float top)
    {
        gl.Vertex(left, bottom, 0);
        gl.Vertex(right, bottom, 0);
        gl.Vertex(right, top, 0);
        gl.Vertex(left, top, 0);
    }
}
