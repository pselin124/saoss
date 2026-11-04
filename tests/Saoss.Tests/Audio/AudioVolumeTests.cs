using Saoss.Game.Audio;

namespace Saoss.Tests.Audio;

/// <summary>
/// Tests the effect and music volume math and the float to 16-bit PCM conversion.
/// </summary>
[TestClass]
public sealed class AudioVolumeTests
{
    [TestMethod]
    [DataRow(0.5f, 100, 0.5f)]
    [DataRow(0.75f, 80, 0.6f)]
    [DataRow(0.125f, 40, 0.05f)]
    [DataRow(1.0f, 0, 0f)]
    [DataRow(0f, 100, 0f)]
    public void Effect_IsRelativeVolumeTimesSetting(float relative, int setting, float expected) =>
        Assert.AreEqual(expected, AudioVolume.Effect(relative, setting), 1e-6f);

    [TestMethod]
    [DataRow(100, 1f)]
    [DataRow(80, 0.8f)]
    [DataRow(0, 0f)]
    [DataRow(150, 1f)]
    public void Music_IsSettingOverHundred(int setting, float expected) =>
        Assert.AreEqual(expected, AudioVolume.Music(setting), 1e-6f);

    [TestMethod]
    public void ToInt16_ScalesClipsAndWritesLittleEndian()
    {
        float[] samples = [0f, 1f, -1f, 0.5f, 2f, -2f];
        var bytes = new byte[samples.Length * PcmConverter.BytesPerSample];

        PcmConverter.ToInt16(samples, bytes);

        var values = Enumerable.Range(0, samples.Length).Select(i => BitConverter.ToInt16(bytes, i * 2)).ToArray();
        CollectionAssert.AreEqual(new short[] { 0, 32767, -32767, 16384, 32767, -32768 }, values);
    }
}
