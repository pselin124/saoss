using Microsoft.Xna.Framework.Graphics;
using Saoss.Game.Rendering;
using Saoss.Game.Rendering.Models;

namespace Saoss.Game;

/// <summary>
/// Textures, fonts and meshes shared by all screens, loaded once at startup.
/// </summary>
public sealed class GameAssets : IDisposable
{
    private readonly Texture2D _fontTexture;

    private GameAssets(Texture2D titleBackground, Texture2D fontTexture, Mesh menuArrow)
    {
        TitleBackground = titleBackground;
        _fontTexture = fontTexture;
        Font = new BitmapFont(fontTexture);
        MenuArrow = menuArrow;
    }

    /// <summary>Picture behind the main menus.</summary>
    public Texture2D TitleBackground { get; }

    /// <summary>The game's only font.</summary>
    public BitmapFont Font { get; }

    /// <summary>Arrow that marks the selected menu item.</summary>
    public Mesh MenuArrow { get; }

    /// <summary>Loads all shared assets from the Content folder next to the executable.</summary>
    public static GameAssets Load(GraphicsDevice graphicsDevice)
    {
        var arrowBuilder = new MeshBuilder();
        MenuArrowModel.Build(arrowBuilder);
        return new GameAssets(
            LoadTexture(graphicsDevice, "title-background.png"),
            LoadTexture(graphicsDevice, "font.png"),
            arrowBuilder.Build(graphicsDevice));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        TitleBackground.Dispose();
        _fontTexture.Dispose();
        MenuArrow.Dispose();
    }

    private static Texture2D LoadTexture(GraphicsDevice graphicsDevice, string fileName) =>
        Texture2D.FromFile(graphicsDevice, Path.Combine(AppContext.BaseDirectory, "Content", "Textures", fileName));
}
