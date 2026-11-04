using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation;

[TestClass]
public sealed class GameRandomTests
{
    private const int Draws = 20_000;

    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(42)]
    [DataRow(-7)]
    [DataRow(161803398)]
    [DataRow(int.MaxValue)]
    [DataRow(int.MinValue)]
    public void Sequence_MatchesSystemRandom(int seed)
    {
        var expected = new Random(seed);
        var actual = new GameRandom(seed);

        for (var i = 0; i < Draws; i++)
        {
            if (i % 3 == 0)
            {
                Assert.AreEqual(expected.Next(i % 50 + 1), actual.NextInt(i % 50 + 1), $"Draw {i}.");
            }
            else
            {
                Assert.AreEqual((float)(expected.NextDouble() * 360f), actual.Next(360f), $"Draw {i}.");
            }
        }
    }
}
