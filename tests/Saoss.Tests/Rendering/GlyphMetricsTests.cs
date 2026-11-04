using Saoss.Game.Rendering;

namespace Saoss.Tests.Rendering;

/// <summary>
/// Tests the font atlas layout and character advances taken from the original font code.
/// </summary>
[TestClass]
public sealed class GlyphMetricsTests
{
    private const float Tolerance = 1e-5f;

    [TestMethod]
    public void MeasureWidth_SumsTwiceTheLetterWidths()
    {
        // Q=0.9, u=0.55, i=0.2, t=0.55 in the original letterwidth table.
        Assert.AreEqual(2f * (0.9f + 0.55f + 0.2f + 0.55f), GlyphMetrics.MeasureWidth("Quit"), Tolerance);
    }

    [TestMethod]
    public void MeasureWidth_ScalesWithSize()
    {
        Assert.AreEqual(3f * GlyphMetrics.MeasureWidth("Saoss"), GlyphMetrics.MeasureWidth("Saoss", size: 3f), Tolerance);
    }

    [TestMethod]
    public void CenteredStart_PutsTheInkMiddleAtZero()
    {
        // "il": i=0.2, l=0.2 -> pens at x and x+0.4; ink from x-0.2 to x+0.6.
        Assert.AreEqual(-0.2f, GlyphMetrics.CenteredStart("il"), Tolerance);
        Assert.AreEqual(0f, GlyphMetrics.CenteredStart("A"), Tolerance);
        Assert.AreEqual(0f, GlyphMetrics.CenteredStart(string.Empty), Tolerance);
    }

    [TestMethod]
    public void TextureBounds_LocatesAsciiCellInSixteenBySixteenGrid()
    {
        // 'A' = 0x41 -> column 1, row 4 of 64 px cells in a 1024 px texture.
        var (topLeft, bottomRight) = GlyphMetrics.TextureBounds('A');

        Assert.AreEqual((64f + 2f) / 1024f, topLeft.X, Tolerance);
        Assert.AreEqual((256f + 2f) / 1024f, topLeft.Y, Tolerance);
        Assert.AreEqual((128f - 1f) / 1024f, bottomRight.X, Tolerance);
        Assert.AreEqual((320f - 1f) / 1024f, bottomRight.Y, Tolerance);
    }

    [TestMethod]
    [DataRow('\t')]
    [DataRow('ä')]
    [DataRow('€')]
    public void Normalize_ReplacesCharactersOutsidePrintableAscii(char c)
    {
        Assert.AreEqual(GlyphMetrics.FallbackCharacter, GlyphMetrics.Normalize(c));
    }
}
