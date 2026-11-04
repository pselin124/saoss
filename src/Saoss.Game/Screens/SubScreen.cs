using Saoss.Game.Input;

namespace Saoss.Game.Screens;

/// <summary>
/// A screen that temporarily replaces its owner, such as a submenu or a text entry; the owner forwards updates
/// and drawing while one is open. The original ran these as nested blocking loops.
/// </summary>
public sealed class SubScreen : IDisposable
{
    /// <summary>The open sub-screen, or null.</summary>
    public IScreen? Current { get; private set; }

    /// <summary>Opens <paramref name="screen"/>, closing any open one.</summary>
    public void Open(IScreen screen)
    {
        Close();
        Current = screen;
    }

    /// <summary>Closes and disposes the open sub-screen.</summary>
    public void Close()
    {
        (Current as IDisposable)?.Dispose();
        Current = null;
    }

    /// <summary>Updates the open sub-screen; returns false when none is open.</summary>
    public bool Update(KeyboardTracker keyboard)
    {
        if (Current == null)
        {
            return false;
        }

        Current.Update(keyboard);
        return true;
    }

    /// <summary>Draws the open sub-screen; returns false when none is open.</summary>
    public bool Draw()
    {
        if (Current == null)
        {
            return false;
        }

        Current.Draw();
        return true;
    }

    /// <inheritdoc/>
    public void Dispose() => Close();
}
