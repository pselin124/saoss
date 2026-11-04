using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Snapshots;

namespace Saoss.Tests.Simulation.Snapshots;

[TestClass]
public sealed class WorldSnapshotTests
{
    private const int Seed = 7;
    private const int Ticks = 2500;
    private const int SnapshotInterval = 500;
    private const int ExtraTicks = 400;
    private const int FuzzRounds = 300;
    private const int SizeTicks = 1000;

    [TestMethod]
    [DataRow("Deathmatch")]
    [DataRow("RandomShips")]
    [DataRow("CreditsRandom")]
    [DataRow("CreditsChoose")]
    [DataRow("Domination")]
    [DataRow("CaptureTheFlower")]
    [DataRow("Mushroom")]
    [DataRow("DominationQuickEnd")]
    public void Snapshot_RestoresEveryField_AndBothWorldsStayIdentical(string scenario)
    {
        var settings = ScriptedMatch.Settings(scenario);
        var host = new World(settings, Seed);
        var mirror = new World(settings, Seed);

        // A mirror that receives every snapshot, as a network client does.
        for (var tick = 1; tick <= Ticks; tick++)
        {
            ScriptedMatch.Drive(host, Seed);
            host.Step();
            if (tick % SnapshotInterval == 0)
            {
                WorldSnapshot.Apply(mirror, WorldSnapshot.ToBytes(host));
                DeepComparer.AssertEqual(host, mirror, $"{scenario}, mirror after tick {tick}");
            }
        }

        var fresh = new World(settings, Seed);
        WorldSnapshot.Apply(fresh, WorldSnapshot.ToBytes(host));
        DeepComparer.AssertEqual(host, fresh, $"{scenario}, fresh world");

        for (var tick = 1; tick <= ExtraTicks; tick++)
        {
            ScriptedMatch.Drive(host, Seed);
            host.Step();
            ScriptedMatch.Drive(fresh, Seed);
            fresh.Step();
        }

        DeepComparer.AssertEqual(host, fresh, $"{scenario}, after stepping both {ExtraTicks} more ticks");
    }

    [TestMethod]
    public void Snapshot_ScenarioReachesInterestingStates()
    {
        var quickEnd = new World(ScriptedMatch.Settings("DominationQuickEnd"), Seed);
        ScriptedMatch.Run(quickEnd, Ticks, Seed);
        Assert.IsNotNull(quickEnd.Outcome, "The quick domination match should end.");

        var choose = new World(ScriptedMatch.Settings("CreditsChoose"), Seed);
        var sawPending = false;
        for (var tick = 0; tick < Ticks && !sawPending; tick++)
        {
            choose.Step();
            sawPending = choose.PendingChoice != null;
            ScriptedMatch.Drive(choose, Seed);
        }

        Assert.IsTrue(sawPending, "The choose-ships match should stop for a purchase.");
    }

    [TestMethod]
    public void Apply_KeepsPendingChoiceInstanceWhileUnchanged()
    {
        var settings = ScriptedMatch.Settings("CreditsChoose");
        var host = new World(settings, Seed);
        var mirror = new World(settings, Seed);
        WorldSnapshot.Apply(mirror, WorldSnapshot.ToBytes(host));
        var choice = mirror.PendingChoice;
        Assert.IsNotNull(choice);

        WorldSnapshot.Apply(mirror, WorldSnapshot.ToBytes(host));

        Assert.AreSame(choice, mirror.PendingChoice);
    }

    [TestMethod]
    public void Apply_RejectsMismatchedWorld()
    {
        var host = new World(ScriptedMatch.Settings("Domination"), Seed);
        var bytes = WorldSnapshot.ToBytes(host);

        Assert.ThrowsExactly<SnapshotFormatException>(() => WorldSnapshot.Apply(new World(ScriptedMatch.Settings("Mushroom"), Seed), bytes));
        Assert.ThrowsExactly<SnapshotFormatException>(() => WorldSnapshot.Apply(new World(ScriptedMatch.Settings("Domination", players: 4), Seed), bytes));
        Assert.ThrowsExactly<SnapshotFormatException>(() => WorldSnapshot.Apply(new World(ScriptedMatch.Settings("Domination"), Seed), (byte[])[.. bytes, 0]));
    }

    [TestMethod]
    public void Apply_CorruptedData_ThrowsOnlySnapshotFormatException()
    {
        var settings = ScriptedMatch.Settings("CaptureTheFlower");
        var host = new World(settings, Seed);
        ScriptedMatch.Run(host, 1500, Seed);
        var valid = WorldSnapshot.ToBytes(host);
        var random = new Random(Seed);
        var target = new World(settings, Seed);

        for (var round = 0; round < FuzzRounds; round++)
        {
            var corrupted = Corrupt(valid, random);
            try
            {
                WorldSnapshot.Apply(target, corrupted);
            }
            catch (SnapshotFormatException)
            {
                // A partly updated world is discarded after this.
                target = new World(settings, Seed);
            }
        }

        for (var length = 0; length < valid.Length; length += 1 + (length / 8))
        {
            Assert.ThrowsExactly<SnapshotFormatException>(() => WorldSnapshot.Apply(new World(settings, Seed), valid.AsMemory(0, length)), $"Length {length}.");
        }
    }

    [TestMethod]
    [DataRow(4)]
    [DataRow(16)]
    public void SnapshotSize_MidBattle_IsSmallEnoughToStream(int ships)
    {
        // Every ship is an AI pilot, so all of them fight.
        var settings = new MatchSettings
        {
            Players = [.. Enumerable.Range(0, ships).Select(i => new PlayerSetup(i, IsHuman: false))],
            Mode = Saoss.Core.GameMode.RandomShipsDeathmatch,
        };
        var world = new World(settings, Seed);
        var sizes = new List<int>();
        for (var tick = 0; tick < SizeTicks; tick++)
        {
            world.Step();
            if (tick >= SnapshotInterval)
            {
                sizes.Add(WorldSnapshot.ToBytes(world).Length);
            }
        }

        TestContext.WriteLine($"{ships} ships: average {sizes.Average():F0} bytes, max {sizes.Max()} bytes per snapshot.");
        Assert.IsLessThan(64 * 1024, sizes.Max());
    }

    /// <summary>Receives the measured sizes.</summary>
    public TestContext TestContext { get; set; } = null!;

    private static byte[] Corrupt(byte[] valid, Random random)
    {
        var bytes = (byte[])valid.Clone();
        var changes = 1 + random.Next(4);
        for (var i = 0; i < changes; i++)
        {
            bytes[random.Next(bytes.Length)] = (byte)random.Next(256);
        }

        return random.Next(4) == 0 ? bytes[..random.Next(bytes.Length)] : bytes;
    }
}
