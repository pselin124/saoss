using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// All textures used by the game scene, loaded from Content/Textures or generated like the original.
/// Each property notes the original texture slot (texture[n] in glapi.cpp) and file. Original slots
/// that were never loaded (0, 1, 3, 7, 9-12, 24, 27, 32, 35-37) mean "no texture" and map to null. The title
/// picture (slot 31) is only the menus' background here (GameAssets); the intro flag has its own FlagTexture.
/// </summary>
public sealed class SceneTextures : IDisposable
{
    private const int FlareSize = 64;
    private const int SquareSize = 16;
    private const float FlareFalloff = 0.005f;

    private readonly List<Texture2D> _owned = [];

    /// <summary>Loads and generates all textures.</summary>
    public SceneTextures(GraphicsDevice device)
    {
        Texture2D Load(string file)
        {
            var texture = Texture2D.FromFile(device, Path.Combine(AppContext.BaseDirectory, "Content", "Textures", file));
            _owned.Add(texture);
            return texture;
        }

        Texture2D Generate(int size, Func<int, int, float> luminance)
        {
            var texture = new Texture2D(device, size, size);
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var value = MathHelper.Clamp(luminance(x, y), 0f, 1f);
                    pixels[(y * size) + x] = new Color(value, value, value, 1f);
                }
            }

            texture.SetData(pixels);
            _owned.Add(texture);
            return texture;
        }

        Shot22 = Load("shot-22.png");
        Shot0 = Load("shot-0.png");
        Shot1 = Load("shot-1.png");
        Trail = Load("trail.png");
        Tottero = Load("tottero.png");
        TrekDetail = Load("trek-detail.png");
        Sucker = Load("sucker.png");
        WhiteSquare = Generate(SquareSize, (_, _) => 1f);
        LittleMan = Load("little-man.png");
        Shot6 = Load("shot-6.png");
        Flare = Generate(FlareSize, FlareLuminance);
        Shot20 = Load("shot-20.png");
        Missile = Load("missile.png");
        Shot8 = Load("shot-8.png");
        Egg = Load("egg.png");
        Galaxy = Load("galaxy.png");
        BlackHole = Load("black-hole.png");
        Galaxy2 = Load("galaxy-2.png");
        Font = Load("font.png");
        Background2 = Load("background-2.png");
        Background3 = Load("background-3.png");
        Shot15 = Load("shot-15.png");
        SpaceTurtle = Load("space-turtle.png");
        Special18 = Load("special-18.png");
        Meteor = Load("meteor.png");
        Electric = Load("electric.png");
        Special20 = Load("special-20.png");
        TeamGlow = Generate(FlareSize, TeamGlowLuminance);
        Ground = Load("ground.png");
    }

    /// <summary>[texture[2]] pati22.png.</summary>
    public Texture2D Shot22 { get; }

    /// <summary>[texture[4]] pati0.png.</summary>
    public Texture2D Shot0 { get; }

    /// <summary>[texture[5]] pati1.png.</summary>
    public Texture2D Shot1 { get; }

    /// <summary>[texture[6]] jalki.png, exhaust trail.</summary>
    public Texture2D Trail { get; }

    /// <summary>[texture[8]] tottero1.png.</summary>
    public Texture2D Tottero { get; }

    /// <summary>[texture[13]] alus5_2.png.</summary>
    public Texture2D TrekDetail { get; }

    /// <summary>[texture[14]] sucker.png.</summary>
    public Texture2D Sucker { get; }

    /// <summary>[texture[15]] generated 16x16 white square.</summary>
    public Texture2D WhiteSquare { get; }

    /// <summary>[texture[16]] pikku.png, the health pickup figure.</summary>
    public Texture2D LittleMan { get; }

    /// <summary>[texture[17]] pati6.png.</summary>
    public Texture2D Shot6 { get; }

    /// <summary>[texture[18]] generated round light flare, used for stars, particles and many shots.</summary>
    public Texture2D Flare { get; }

    /// <summary>[texture[19]] pati20.png.</summary>
    public Texture2D Shot20 { get; }

    /// <summary>[texture[20]] Ohjus.png (missile).</summary>
    public Texture2D Missile { get; }

    /// <summary>[texture[21]] pati8.png.</summary>
    public Texture2D Shot8 { get; }

    /// <summary>[texture[22]] muna.png (egg).</summary>
    public Texture2D Egg { get; }

    /// <summary>[texture[23]] galax.png.</summary>
    public Texture2D Galaxy { get; }

    /// <summary>[texture[25]] aukko.png (black hole).</summary>
    public Texture2D BlackHole { get; }

    /// <summary>[texture[26]] galax2.png.</summary>
    public Texture2D Galaxy2 { get; }

    /// <summary>[texture[28]] teksti.png, the bitmap font.</summary>
    public Texture2D Font { get; }

    /// <summary>[texture[29]] tausta2.png.</summary>
    public Texture2D Background2 { get; }

    /// <summary>[texture[30]] tausta3.png.</summary>
    public Texture2D Background3 { get; }

    /// <summary>[texture[33]] pati15.png.</summary>
    public Texture2D Shot15 { get; }

    /// <summary>[texture[34]] alus20.png (Space Turtle shell).</summary>
    public Texture2D SpaceTurtle { get; }

    /// <summary>[texture[38]] epati18.png.</summary>
    public Texture2D Special18 { get; }

    /// <summary>[texture[39]] meteor2.png, asteroid surface.</summary>
    public Texture2D Meteor { get; }

    /// <summary>[texture[40]] electric.png.</summary>
    public Texture2D Electric { get; }

    /// <summary>[texture[41]] epati20.png.</summary>
    public Texture2D Special20 { get; }

    /// <summary>[texture[42]] generated ring glow around ships, tinted by team color.</summary>
    public Texture2D TeamGlow { get; }

    /// <summary>ground.png, used by the intro scene.</summary>
    public Texture2D Ground { get; }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var texture in _owned)
        {
            texture.Dispose();
        }
    }

    // generateLightFlare: Gaussian blob.
    private static float FlareLuminance(int x, int y)
    {
        var dx = x - (FlareSize / 2);
        var dy = y - (FlareSize / 2);
        return MathF.Exp(-((dx * dx) + (dy * dy)) * FlareFalloff) - 0.01f;
    }

    // generateTeamJuttu: brighter Gaussian with the middle hollowed out.
    private static float TeamGlowLuminance(int x, int y)
    {
        const float innerRadius = 16f;
        var dx = x - (FlareSize / 2);
        var dy = y - (FlareSize / 2);
        var value = (MathF.Exp(-((dx * dx) + (dy * dy)) * FlareFalloff) - 0.01f) * 1.5f;
        var r = MathF.Sqrt((dx * dx) + (dy * dy));
        if (r < innerRadius)
        {
            value -= MathF.Pow(1f - (r / innerRadius), 0.5f) + 1f - (r / innerRadius);
        }

        return MathF.Max(value, 0f);
    }
}
