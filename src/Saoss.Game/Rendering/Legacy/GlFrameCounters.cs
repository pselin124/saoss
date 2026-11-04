namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// Developer counters of the work all <see cref="LegacyGl"/> instances did since the last <see cref="Reset"/>,
/// read once per frame by the performance monitor.
/// </summary>
public static class GlFrameCounters
{
    /// <summary>Device draw calls issued by batch flushes.</summary>
    public static int DrawCalls { get; private set; }

    /// <summary>Vertices transformed (and lit) on the CPU by glVertex.</summary>
    public static int Vertices { get; private set; }

    /// <summary>Starts counting a new frame.</summary>
    public static void Reset()
    {
        DrawCalls = 0;
        Vertices = 0;
    }

    internal static void CountVertex() => Vertices++;

    internal static void CountDraw() => DrawCalls++;
}
