using Saoss.Core.Ships;

namespace Saoss.Tests.Simulation;

/// <summary>
/// Spot checks that the generated ship catalog matches the original stat blocks.
/// </summary>
[TestClass]
public sealed class ShipCatalogTests
{
    [TestMethod]
    public void All_HasOneDefinitionPerTypeInIndexOrder()
    {
        Assert.HasCount(24, ShipCatalog.All);
        for (var i = 0; i < ShipCatalog.All.Count; i++)
        {
            Assert.AreEqual((ShipType)i, ShipCatalog.All[i].Type);
        }
    }

    [TestMethod]
    public void Magneto_MatchesOriginalStats()
    {
        var magneto = ShipCatalog.Get(ShipType.Magneto);

        Assert.AreEqual(200f, magneto.Mass);
        Assert.AreEqual(0.0015f, magneto.Thrust);
        Assert.AreEqual(2.6f, magneto.Radius);
        Assert.AreEqual(8, magneto.Primary.FireDelay);
        Assert.AreEqual(40, magneto.Primary.MaxProjectiles);
        Assert.AreEqual(125, magneto.Special.Duration);
        Assert.AreEqual(19, magneto.Cost);
    }

    [TestMethod]
    public void Popper_And_Elephant_MatchOriginalStats()
    {
        Assert.AreEqual(85f, ShipCatalog.Get(ShipType.Popper).Special.Damage);
        Assert.AreEqual(1000, ShipCatalog.Get(ShipType.Elephant).Special.MaxProjectiles);
        Assert.AreEqual(47, ShipCatalog.Get(ShipType.Elephant).Cost);
    }

    [TestMethod]
    public void InertiaFreeShips_AreUfoElectraAndStarWorm()
    {
        var inertiaFree = ShipCatalog.All.Where(d => d.IsInertiaFree).Select(d => d.Type).ToArray();

        CollectionAssert.AreEquivalent(new[] { ShipType.Ufo, ShipType.Electra, ShipType.StarWorm }, inertiaFree);
    }
}
