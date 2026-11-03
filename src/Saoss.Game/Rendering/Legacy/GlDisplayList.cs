namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// A recorded sequence of GL calls, replayed by <see cref="LegacyGl.CallList"/> (an OpenGL display list).
/// </summary>
public sealed class GlDisplayList
{
    /// <summary>Recorded calls in order.</summary>
    internal List<Action<LegacyGl>> Commands { get; } = [];
}
