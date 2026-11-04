using Microsoft.Xna.Framework;
using XnaColor = Microsoft.Xna.Framework.Color;

namespace Saoss.Game.Rendering.Legacy;

/// <summary>
/// Collects the lit vertices of a frame and lights them all at once when the frame is drawn, on several threads
/// when there are many. Each vertex keeps a copy of the lighting state it was emitted under, so the colors are the
/// same as lighting every vertex at once.
/// </summary>
internal sealed class GlDeferredShading
{
    private const int ChunkSize = 2048;
    private const int ParallelThreshold = 2 * ChunkSize;

    private readonly List<GlLighting> _lightingCopies = [];
    private readonly Action<int> _shadeChunk;
    private GlShadeJob[] _jobs = new GlShadeJob[4096];
    private XnaColor[] _colors = new XnaColor[4096];
    private int _count;
    private int _lightingCopyCount;
    private int _copiedVersion;

    /// <summary>Creates an empty set.</summary>
    public GlDeferredShading() => _shadeChunk = ShadeChunk;

    /// <summary>Number of vertices waiting.</summary>
    public int Count => _count;

    /// <summary>Adds a vertex lit by the current state of <paramref name="lighting"/>; returns its job index.</summary>
    public int Add(Vector3 position, Vector3 normal, Vector4 material, GlLighting lighting)
    {
        if (_count == _jobs.Length)
        {
            Array.Resize(ref _jobs, _jobs.Length * 2);
            Array.Resize(ref _colors, _colors.Length * 2);
        }

        _jobs[_count] = new GlShadeJob(position, normal, material, CopyOf(lighting));
        return _count++;
    }

    /// <summary>Lights every waiting vertex.</summary>
    public void ShadeAll()
    {
        if (_count < ParallelThreshold)
        {
            ShadeRange(0, _count);
            return;
        }

        Parallel.For(0, (_count + ChunkSize - 1) / ChunkSize, _shadeChunk);
    }

    /// <summary>The color of job <paramref name="job"/> after <see cref="ShadeAll"/>.</summary>
    public XnaColor ColorOf(int job) => _colors[job];

    /// <summary>Forgets all vertices and lighting copies.</summary>
    public void Clear()
    {
        _count = 0;
        _lightingCopyCount = 0;
    }

    // Copies are pooled and made only when the state changed since the last lit vertex.
    private int CopyOf(GlLighting lighting)
    {
        var version = lighting.Version;
        if (_lightingCopyCount > 0 && version == _copiedVersion)
        {
            return _lightingCopyCount - 1;
        }

        if (_lightingCopyCount == _lightingCopies.Count)
        {
            _lightingCopies.Add(new GlLighting());
        }

        _lightingCopies[_lightingCopyCount].CopyFrom(lighting);
        _copiedVersion = version;
        return _lightingCopyCount++;
    }

    private void ShadeChunk(int chunk) => ShadeRange(chunk * ChunkSize, Math.Min(_count, (chunk + 1) * ChunkSize));

    private void ShadeRange(int start, int end)
    {
        for (var i = start; i < end; i++)
        {
            ref readonly var job = ref _jobs[i];
            var color = _lightingCopies[job.Lighting].Shade(job.Position, job.Normal, job.Material);
            _colors[i] = new XnaColor(Vector4.Clamp(color, Vector4.Zero, Vector4.One));
        }
    }
}
