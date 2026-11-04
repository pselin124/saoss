using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Ai;
using Saoss.Tests.Simulation.Snapshots;

namespace Saoss.Tests.Simulation;

/// <summary>
/// Tests the AI levels: every level plays every mode, the original level stays untouched, and the levels are
/// measurably ordered against a target dummy.
/// </summary>
[TestClass]
public sealed class AiDifficultyTests
{
    private const int SanityTicks = 3000;
    private const int DummyTicks = 6000;
    private const int DummySeeds = 16;
    private const float DummyX = 80f;
    private const float DummyY = 60f;
    private const double OrderMargin = 1.25;

    /// <summary>Receives the measured damage.</summary>
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void Defaults_AreTheOriginalPilot()
    {
        Assert.AreEqual(AiDifficulty.GalacticTerror, new MatchSettings { Players = [] }.AiDifficulty);
        Assert.IsTrue(AiSkill.For(AiDifficulty.GalacticTerror).IsFlawless);
        Assert.IsFalse(AiSkill.For(AiDifficulty.RocketAce).IsFlawless);
        Assert.IsFalse(AiSkill.For(AiDifficulty.SpaceCadet).IsFlawless);
    }

    [TestMethod]
    public void EveryLevel_PlaysEveryMode()
    {
        foreach (var difficulty in Enum.GetValues<AiDifficulty>())
        {
            foreach (var scenario in ScriptedMatch.Scenarios)
            {
                var world = new World(ScriptedMatch.Settings(scenario) with { AiDifficulty = difficulty }, 3);
                ScriptedMatch.Run(world, SanityTicks, 3);
                Assert.IsTrue(world.Tick == SanityTicks || world.Outcome != null, $"{difficulty} {scenario}");
            }
        }
    }

    [TestMethod]
    public void GalacticTerror_NeverTouchesTheHandicapState()
    {
        var world = AllAi(AiDifficulty.GalacticTerror);
        ScriptedMatch.Run(world, SanityTicks, 1);

        foreach (var ship in world.Ships)
        {
            DeepComparer.AssertEqual(new AiState(), ship.Ai, $"ship {ship.Index}");
        }
    }

    [TestMethod]
    [DataRow(AiDifficulty.SpaceCadet)]
    [DataRow(AiDifficulty.RocketAce)]
    public void EasierLevels_RollTheirHandicaps(AiDifficulty difficulty)
    {
        var skill = AiSkill.For(difficulty);
        var world = AllAi(difficulty);
        var scales = new HashSet<float>();
        var errors = new HashSet<float>();
        var holding = 0;
        for (var tick = 0; tick < SanityTicks; tick++)
        {
            world.Step();
            var ai = world.Ships[0].Ai;
            scales.Add(ai.DistanceScale);
            errors.Add(ai.AimErrorDegrees);
            holding += ai.IsHoldingBack ? 1 : 0;
            Assert.IsLessThanOrEqualTo(skill.DistanceJitter + 1e-6, Math.Abs(ai.DistanceScale - 1));
            Assert.IsLessThanOrEqualTo(skill.MaxAimErrorDegrees, Math.Abs(ai.AimErrorDegrees));
        }

        Assert.IsGreaterThan(3, scales.Count, "The distance scale is re-rolled.");
        Assert.IsGreaterThan(10, errors.Count, "The aim error is re-rolled.");
        Assert.IsTrue(holding > 0 && holding < SanityTicks, $"Holding back {holding} of {SanityTicks} ticks.");
    }

    [TestMethod]
    public void HarderLevels_DealMoreDamageToADummy()
    {
        var cadet = TotalDummyDamage(AiDifficulty.SpaceCadet);
        var ace = TotalDummyDamage(AiDifficulty.RocketAce);
        var terror = TotalDummyDamage(AiDifficulty.GalacticTerror);
        TestContext.WriteLine($"Damage to the dummy over {DummySeeds} x {DummyTicks} ticks: Space Cadet {cadet:F0}, Rocket Ace {ace:F0}, Galactic Terror {terror:F0}.");

        Assert.IsGreaterThan(ace * OrderMargin, terror, "Galactic Terror beats Rocket Ace.");
        Assert.IsGreaterThan(cadet * OrderMargin, ace, "Rocket Ace beats Space Cadet.");
    }

    private static World AllAi(AiDifficulty difficulty) => new(
        new MatchSettings
        {
            Players = [.. Enumerable.Range(0, 8).Select(i => new PlayerSetup(i, IsHuman: false))],
            Mode = GameMode.RandomShipsDeathmatch,
            AiDifficulty = difficulty,
        },
        1);

    // One AI against a human that never touches the controls and is held at a fixed spot, over many seeds.
    private static double TotalDummyDamage(AiDifficulty difficulty)
    {
        var total = 0.0;
        for (var seed = 1; seed <= DummySeeds; seed++)
        {
            var world = TestWorld.Create(
                new MatchSettings { Players = [new PlayerSetup(0, IsHuman: false), new PlayerSetup(1, IsHuman: true)], AiDifficulty = difficulty },
                seed);
            total += DummyDamage(world, world.Ships[1]);
        }

        return total;
    }

    private static double DummyDamage(World world, Ship dummy)
    {
        var damage = 0.0;
        for (var tick = 0; tick < DummyTicks; tick++)
        {
            var wasAlive = dummy.IsAlive;
            if (wasAlive)
            {
                dummy.X = DummyX;
                dummy.Y = DummyY;
                dummy.VelocityX = 0;
                dummy.VelocityY = 0;
            }

            var energy = dummy.Energy;
            world.Step();
            if (wasAlive)
            {
                damage += dummy.IsAlive ? Math.Max(0, energy - dummy.Energy) : Math.Max(0, energy);
            }
        }

        return damage;
    }
}
