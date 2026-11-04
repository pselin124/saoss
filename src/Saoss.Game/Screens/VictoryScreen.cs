using Microsoft.Xna.Framework.Input;
using Saoss.Core.Simulation.Modes;
using Saoss.Game.Input;
using Saoss.Game.Rendering.Scene;

namespace Saoss.Game.Screens;

/// <summary>
/// The end-of-match screen shown after a match ends (the drawVictory loops of localgame.cpp), until Enter or
/// Escape is pressed. It draws with the finished match's resources and leaves the match itself untouched.
/// </summary>
public sealed class VictoryScreen : IScreen
{
    private readonly GameScreen _match;
    private readonly VictoryLayout _layout;
    private readonly Action _done;

    /// <summary>Shows how <paramref name="match"/> ended; <paramref name="done"/> runs when the player dismisses it.</summary>
    public VictoryScreen(GameScreen match, MatchOutcome outcome, Action done)
    {
        _match = match;
        _layout = VictoryLayout.Create(match.World, outcome);
        _done = done;
    }

    /// <inheritdoc/>
    public void Update(KeyboardTracker keyboard)
    {
        if (keyboard.WasPressed(Keys.Enter) || keyboard.WasPressed(Keys.Escape))
        {
            _done();
        }
    }

    /// <inheritdoc/>
    public void Draw() => _match.DrawVictory(_layout);
}
