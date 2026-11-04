using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// Typing one line of text (net.cpp readnick and readip): the prompt and the text on black. Enter or Escape ends
/// the entry with the typed text, like the original, which edited the text in place.
/// </summary>
public sealed class TextEntryScreen : IScreen, IDisposable
{
    private static readonly Vector3 PromptPosition = new(-10f, 10f, -60f);
    private static readonly Vector3 TextPosition = new(-38f, 6f, -60f);

    private readonly MenuScene _scene;
    private readonly string _prompt;
    private readonly TextInputBuffer _buffer;
    private readonly Action<string> _done;

    /// <summary>Starts editing <paramref name="buffer"/>'s text; <paramref name="done"/> gets the result.</summary>
    public TextEntryScreen(GraphicsDevice graphicsDevice, GameAssets assets, string prompt, TextInputBuffer buffer, Action<string> done)
    {
        _scene = new MenuScene(graphicsDevice, assets);
        _prompt = prompt;
        _buffer = buffer;
        _done = done;
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (keyboard.WasPressed(Keys.Enter) || keyboard.WasPressed(Keys.Escape))
        {
            _done(_buffer.Text);
            return;
        }

        _buffer.Type(keyboard.TypedCharacters);
    }

    /// <inheritdoc/>
    public void Draw()
    {
        _scene.Begin(withBackground: false);
        _scene.DrawText(PromptPosition, _prompt, Color.Red);
        _scene.DrawText(TextPosition, _buffer.Text, Color.Red);
    }

    /// <inheritdoc/>
    public void Dispose() => _scene.Dispose();
}
