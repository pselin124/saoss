using Saoss.Game.Audio;

namespace Saoss.Tests.Audio;

/// <summary>
/// Decodes the start of the shipped music files, to check the streaming decoders produce sound.
/// </summary>
[TestClass]
public sealed class MusicDecoderTests
{
    private const int ExpectedSampleRate = 44100;
    private const int ExpectedChannels = 2;

    [TestMethod]
    [DataRow("dialinthefortforyou.ogg")]
    [DataRow("eramaa.mp3")]
    [DataRow(@"Start\start.mp3")]
    public void FirstSecond_DecodesToNonSilentSamples(string file)
    {
        using var decoder = MusicDecoders.Open(Path.Combine(AppContext.BaseDirectory, "Content", "Music", file));
        var buffer = new float[ExpectedSampleRate * ExpectedChannels];

        var total = 0;
        while (total < buffer.Length)
        {
            var read = decoder.ReadSamples(buffer, total, buffer.Length - total);
            if (read == 0)
            {
                break;
            }

            total += read;
        }

        Assert.AreEqual(ExpectedSampleRate, decoder.SampleRate);
        Assert.AreEqual(ExpectedChannels, decoder.Channels);
        Assert.AreEqual(buffer.Length, total);
        Assert.IsTrue(buffer.Any(sample => Math.Abs(sample) > 0.01f), "The first second is silent.");
        Assert.IsTrue(buffer.All(sample => Math.Abs(sample) <= 1.5f), "Samples are out of range.");
    }

    [TestMethod]
    public void Open_OfAnUnsupportedFile_Throws() =>
        Assert.ThrowsExactly<NotSupportedException>(() => MusicDecoders.Open("song.xm"));
}
