namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// A recorded sequence of GL calls, replayed by <see cref="LegacyGl.CallList"/> (an OpenGL display list).
/// </summary>
public sealed class GlDisplayList
{
    /// <summary>Recorded calls in order.</summary>
    internal List<GlListCommand> Commands { get; } = [];

    /// <summary>Records a call without arguments.</summary>
    internal void Add(Action<LegacyGl> command) => Commands.Add(new GlListCommand(GlListOp.Call, 0, 0, 0, 0, command));

    /// <summary>Records a call that needs its arguments, <paramref name="state"/>.</summary>
    internal void Add<TState>(TState state, Action<LegacyGl, TState> command) => Add(gl => command(gl, state));

    /// <summary>Records a vertex attribute call with its arguments.</summary>
    internal void Add(GlListOp op, float x, float y, float z = 0, float w = 0) => Commands.Add(new GlListCommand(op, x, y, z, w));
}
