using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// A message on black with a hint below it (net.cpp dispCustomError and dispErrorFullScreen), shown until Enter or
/// Escape. Also used while connecting, with a hint to cancel.
/// </summary>
public sealed class MessageScreen : IScreen, IDisposable
{
    /// <summary>The original's hint under an error.</summary>
    public const string ContinueHint = "press enter to continue..";

    private static readonly Vector3 MessagePosition = new(-30f, 12f, -60f);
    private static readonly Vector3 HintPosition = new(-30f, 8f, -60f);

    private readonly MenuScene _scene;
    private readonly string _message;
    private readonly string _hint;
    private readonly Action _dismiss;

    /// <summary>Shows <paramref name="message"/>; <paramref name="dismiss"/> runs on Enter or Escape.</summary>
    public MessageScreen(GraphicsDevice graphicsDevice, GameAssets assets, string message, Action dismiss, string hint = ContinueHint)
    {
        _scene = new MenuScene(graphicsDevice, assets);
        _message = message;
        _hint = hint;
        _dismiss = dismiss;
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (keyboard.WasPressed(Keys.Enter) || keyboard.WasPressed(Keys.Escape))
        {
            _dismiss();
        }
    }

    /// <inheritdoc/>
    public void Draw()
    {
        _scene.Begin(withBackground: false);
        _scene.DrawText(MessagePosition, _message, Color.Red);
        _scene.DrawText(HintPosition, _hint, Color.Red);
    }

    /// <inheritdoc/>
    public void Dispose() => _scene.Dispose();
}
