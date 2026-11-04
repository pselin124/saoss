using System.Globalization;
using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;
using Saoss.Game.Rendering.Legacy;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Draws the game mode's objects in the arena (piirraDPisteet, piirraCPisteet, piirraSPisteet) and its team
/// scoreboard at the top of the HUD (piirraDGraffat, piirraCGraffat, piirraSGraffat).
/// </summary>
public sealed class ModeRenderer(LegacyGl gl, SceneTextures textures, SharedModels models)
{
    private const float ControlPointScale = 8f;
    private const float FlowerScale = 5f;
    private const float MushroomScale = 5f;
    private const float BaseSize = 10f;
    private const float BaseHeight = 1f;
    private const float ScoreboardX = -8.5f;
    private const float ScoreboardY = 10f;
    private const float TitleSize = 0.4f;
    private const float EntrySize = 0.25f;
    private const float EntryWidth = 3.5f;
    private const float RowHeight = 1.2f;
    private const int TeamsPerRow = 5;

    // Pen position of the slash: a three-digit score starts at the entry's left edge.
    private const float SlashX = 1.1f;

    private readonly GlText _text = new(gl, textures.Font);

    /// <summary>Draws the control points, flowers and bases, or the mushroom; nothing in deathmatch.</summary>
    public void DrawMarkers(World world)
    {
        switch (world.Rules)
        {
            case DominationMode domination:
                DrawControlPoints(domination);
                break;
            case CaptureTheFlowerMode ctf:
                DrawFlowers(ctf);
                break;
            case MushroomMode mushroom:
                DrawMushroom(mushroom);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Draws the mode's title and one "team N / score / target" entry per team, five per row, in team order of
    /// first appearance. Expects the HUD view (identity moved 30 units forward). Nothing in deathmatch.
    /// </summary>
    public void DrawScoreboard(World world)
    {
        var title = world.Rules switch
        {
            DominationMode => "domination score",
            CaptureTheFlowerMode => "capture the flower score",
            MushroomMode => "mushroom score",
            _ => null,
        };
        if (title == null)
        {
            return;
        }

        gl.PushMatrix();
        gl.Enable(GlCapability.Blend);
        gl.BlendFunc(GlBlend.One, GlBlend.One);
        gl.Disable(GlCapability.Lighting);
        gl.Translate(ScoreboardX, ScoreboardY, 0);
        gl.Color(0.2f, 0.2f, 0.8f, 0.8f);
        _text.Println(1f, 2f, 0, title, TitleSize);
        gl.Color(0.2f, 0.5f, 0.2f, 0.5f);
        var target = world.Settings.Credits.ToString(CultureInfo.InvariantCulture);
        var shown = new HashSet<int>();
        foreach (var ship in world.Ships)
        {
            if (shown.Add(ship.Team))
            {
                DrawEntry(ship, target);

                // The original wrapped after the fifth and tenth team only.
                if (shown.Count is TeamsPerRow or TeamsPerRow * 2)
                {
                    gl.Translate(-EntryWidth * TeamsPerRow, -RowHeight, 0);
                }
            }
        }

        gl.PopMatrix();
    }

    // "team N" over "score / target"; scores are right-aligned so the slash stays in one column.
    private void DrawEntry(Ship ship, string target)
    {
        var score = ((int)ship.Score).ToString(CultureInfo.InvariantCulture);
        _text.Println(0, 1f, 0, $"team {(ship.Team + 1).ToString(CultureInfo.InvariantCulture)}", EntrySize);
        _text.Println(SlashX - GlyphMetrics.MeasureWidth(score + " ", EntrySize), 0.5f, 0, $"{score} / {target}", EntrySize);
        gl.Translate(EntryWidth, 0, 0);
    }

    // White flowers until a team takes them, then in the team color.
    private void DrawControlPoints(DominationMode mode)
    {
        gl.Enable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Lighting);
        gl.Disable(GlCapability.Blend);
        gl.Disable(GlCapability.Texture2D);
        foreach (var point in mode.Points)
        {
            gl.Color(1, 1, 1, 1);
            if (point.OwnerTeam >= 0)
            {
                SceneDrawing.TeamColor(gl, point.OwnerTeam);
            }

            DrawFlowerAt(point.X, point.Y, ControlPointScale);
        }
    }

    // Each base is a glow in the team color; the flower stands on it while home or lies where it was dropped.
    // A carried flower is drawn by the ship.
    private void DrawFlowers(CaptureTheFlowerMode mode)
    {
        foreach (var flower in mode.Flowers)
        {
            if (!flower.IsEnabled)
            {
                continue;
            }

            gl.Enable(GlCapability.DepthTest);
            gl.Disable(GlCapability.Lighting);
            gl.Enable(GlCapability.Blend);
            gl.Enable(GlCapability.Texture2D);
            SceneDrawing.TeamColor(gl, flower.Team);
            gl.PushMatrix();
            gl.Translate(flower.HomeX, flower.HomeY, GameConstants.ShipPlaneZ);
            gl.BindTexture(textures.Flare);
            SceneDrawing.Quad(gl, BaseSize, BaseHeight);
            gl.PopMatrix();
            if (flower.State != FlowerState.Carried)
            {
                gl.Enable(GlCapability.Lighting);
                gl.Disable(GlCapability.Blend);
                gl.Disable(GlCapability.Texture2D);
                DrawFlowerAt(flower.X, flower.Y, FlowerScale);
            }
        }
    }

    private void DrawMushroom(MushroomMode mode)
    {
        gl.Enable(GlCapability.DepthTest);
        gl.Enable(GlCapability.Lighting);
        gl.Disable(GlCapability.Blend);
        gl.Disable(GlCapability.Texture2D);
        gl.Color(1, 1, 1, 1);
        gl.PushMatrix();
        gl.Translate(mode.X, mode.Y, GameConstants.ShipPlaneZ);
        gl.Scale(MushroomScale, MushroomScale, MushroomScale);
        gl.Rotate(90, 1, 0, 0);
        gl.CallList(models.Mushroom);
        gl.PopMatrix();
    }

    private void DrawFlowerAt(float x, float y, float scale)
    {
        gl.PushMatrix();
        gl.Translate(x, y, GameConstants.ShipPlaneZ);
        gl.Scale(scale, scale, scale);
        gl.Rotate(90, 1, 0, 0);
        gl.CallList(models.Flower4);
        gl.PopMatrix();
    }
}
