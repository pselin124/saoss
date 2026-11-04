using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Saoss.Game.Diagnostics;

/// <summary>
/// Saves one frame as a PNG and ends the game (<c>--capture &lt;png&gt;</c>): the game runs one update per frame, so
/// frame <paramref name="frame"/> always shows the same moment, for pixel comparisons between builds.
/// </summary>
/// <param name="path">PNG file to write.</param>
/// <param name="frame">1-based number of the frame to save.</param>
public sealed class FrameCapture(string path, int frame)
{
    /// <summary>Frame saved when none is given.</summary>
    public const int DefaultFrame = 300;

    /// <summary>Seed for the scenes' visual randomness while capturing.</summary>
    public const int Seed = 1;

    private int _frames;

    /// <summary>Counts a drawn frame before it is presented; saves it and returns true when it is the wanted one.</summary>
    public bool OnFrameDrawn(GraphicsDevice device)
    {
        if (++_frames != frame)
        {
            return false;
        }

        var parameters = device.PresentationParameters;
        var pixels = new Color[parameters.BackBufferWidth * parameters.BackBufferHeight];
        device.GetBackBufferData(pixels);
        using var texture = new Texture2D(device, parameters.BackBufferWidth, parameters.BackBufferHeight);
        texture.SetData(pixels);
        using var file = File.Create(path);
        texture.SaveAsPng(file, texture.Width, texture.Height);
        return true;
    }
}
