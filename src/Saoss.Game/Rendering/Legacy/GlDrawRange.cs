namespace Saoss.Game.Rendering.Legacy;

/// <summary>A run of batched indices drawn with one render state.</summary>
/// <param name="Key">How the vertices are drawn.</param>
/// <param name="Start">First index in the batch.</param>
/// <param name="Count">Number of indices.</param>
internal readonly record struct GlDrawRange(GlBatchKey Key, int Start, int Count);
