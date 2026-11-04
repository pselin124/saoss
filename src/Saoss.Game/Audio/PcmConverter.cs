using System.Buffers.Binary;

namespace Saoss.Game.Audio;

/// <summary>Converts decoded float samples to the 16-bit PCM that MonoGame's streaming sound takes.</summary>
public static class PcmConverter
{
    /// <summary>Bytes of one 16-bit sample.</summary>
    public const int BytesPerSample = sizeof(short);

    /// <summary>
    /// Writes <paramref name="samples"/> (-1 to 1, clipped outside) as little-endian 16-bit samples to
    /// <paramref name="destination"/>, which must hold <see cref="BytesPerSample"/> bytes per sample.
    /// </summary>
    public static void ToInt16(ReadOnlySpan<float> samples, Span<byte> destination)
    {
        for (var i = 0; i < samples.Length; i++)
        {
            var value = (short)Math.Clamp(MathF.Round(samples[i] * short.MaxValue), short.MinValue, short.MaxValue);
            BinaryPrimitives.WriteInt16LittleEndian(destination[(i * BytesPerSample)..], value);
        }
    }
}
