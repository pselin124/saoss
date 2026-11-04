namespace Saoss.Game.Diagnostics;

/// <summary>What one drawn frame cost.</summary>
/// <param name="FrameMs">Wall time since the previous frame ended.</param>
/// <param name="UpdateMs">Time spent in updates since the previous frame.</param>
/// <param name="DrawMs">Time spent drawing and presenting this frame.</param>
/// <param name="DrawCalls">Device draw calls of the emulated GL.</param>
/// <param name="Vertices">Vertices transformed and lit on the CPU.</param>
/// <param name="AllocatedBytes">Managed memory allocated on the game thread during the frame.</param>
public readonly record struct FrameSample(double FrameMs, double UpdateMs, double DrawMs, int DrawCalls, int Vertices, long AllocatedBytes);
