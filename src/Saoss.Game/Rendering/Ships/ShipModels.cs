using Saoss.Game.Rendering.Legacy;
using Saoss.Game.Rendering.Scene;
using Saoss.Game.Rendering.Ships.Models;

namespace Saoss.Game.Rendering.Ships;

/// <summary>
/// The display lists of all 24 ship models, recorded once like the original <c>alusPiirto()</c>
/// (aluspiirto.cpp). Several lists only make sense in pairs because one leaves a matrix pushed for a
/// part the draw code places in between; see each model class.
/// </summary>
public sealed class ShipModels
{
    /// <summary>Records every ship list at the given detail level (0-2).</summary>
    public ShipModels(LegacyGl gl, SharedModels shared, int detail)
    {
        Shared = shared;
        (MagnetoHull, MagnetoCanopy, MagnetoGuns) = MagnetoModel.Record(gl, detail);
        TringuBody = TringuModel.Record(gl, shared, detail);
        WaspBody = WaspModel.Record(gl, detail);
        SubniBody = SubniModel.Record(gl, detail);
        (RolluMast, RolluBody) = RolluModel.Record(gl, shared, detail);
        TrekBody = TrekModel.Record(gl, detail);
        (UfoSaucer, UfoCannon) = UfoModel.Record(gl, detail);
        BombaBody = BombaModel.Record(gl, detail);
        RayskaBody = RayskaModel.Record(gl, detail);
        PeaceCannonBody = PeaceCannonModel.Record(gl, shared, detail);
        BlodduBody = BlodduModel.Record(gl, detail);
        ShapperiBody = ShapperiModel.Record(gl, detail);
        (MothSpine, MothWings) = MothModel.Record(gl, detail);
        PowerVacuumBody = PowerVacuumModel.Record(gl, detail);
        (BulldogBarrel, BulldogRear) = BulldogModel.Record(gl, detail);
        (WiiiiBase, WiiiiTower, WiiiiTop) = WiiiiModel.Record(gl, detail);
        (ElephantHead, ElephantTrunk) = ElephantModel.Record(gl, detail);
        CubixBody = CubixModel.Record(gl, detail);
        (ElectraBody, ElectraSpark) = ElectraModel.Record(gl, detail);
        TonguBody = TonguModel.Record(gl, detail);
        SpaceTurtleBody = SpaceTurtleModel.Record(gl, detail);
        (StickInsectBody, StickInsectShadow) = StickInsectModel.Record(gl, detail);
        StarWormHead = StarWormModel.Record(gl, detail);
        PopperBody = PopperModel.Record(gl, detail);
    }

    /// <summary>Shared scene lists (cube, wedge, flowers) that ship models also use.</summary>
    public SharedModels Shared { get; }

    /// <summary>[DRAW_ALUS_0_OSA_1].</summary>
    public GlDisplayList MagnetoHull { get; }

    /// <summary>[DRAW_ALUS_0_OSA_2].</summary>
    public GlDisplayList MagnetoCanopy { get; }

    /// <summary>[DRAW_ALUS_0_OSA_3].</summary>
    public GlDisplayList MagnetoGuns { get; }

    /// <summary>[DRAW_ALUS_1].</summary>
    public GlDisplayList TringuBody { get; }

    /// <summary>[DRAW_ALUS_2].</summary>
    public GlDisplayList WaspBody { get; }

    /// <summary>[DRAW_ALUS_3].</summary>
    public GlDisplayList SubniBody { get; }

    /// <summary>[DRAW_ALUS_4_OSA_1].</summary>
    public GlDisplayList RolluMast { get; }

    /// <summary>[DRAW_ALUS_4_OSA_2].</summary>
    public GlDisplayList RolluBody { get; }

    /// <summary>[DRAW_ALUS_5].</summary>
    public GlDisplayList TrekBody { get; }

    /// <summary>[DRAW_ALUS_6_OSA_1].</summary>
    public GlDisplayList UfoSaucer { get; }

    /// <summary>[DRAW_ALUS_6_OSA_2].</summary>
    public GlDisplayList UfoCannon { get; }

    /// <summary>[DRAW_ALUS_7].</summary>
    public GlDisplayList BombaBody { get; }

    /// <summary>[DRAW_ALUS_8].</summary>
    public GlDisplayList RayskaBody { get; }

    /// <summary>[DRAW_ALUS_9].</summary>
    public GlDisplayList PeaceCannonBody { get; }

    /// <summary>[DRAW_ALUS_10].</summary>
    public GlDisplayList BlodduBody { get; }

    /// <summary>[DRAW_ALUS_11].</summary>
    public GlDisplayList ShapperiBody { get; }

    /// <summary>[DRAW_ALUS_12_OSA_1].</summary>
    public GlDisplayList MothSpine { get; }

    /// <summary>[DRAW_ALUS_12_OSA_2].</summary>
    public GlDisplayList MothWings { get; }

    /// <summary>[DRAW_ALUS_13].</summary>
    public GlDisplayList PowerVacuumBody { get; }

    /// <summary>[DRAW_ALUS_14_OSA_1].</summary>
    public GlDisplayList BulldogBarrel { get; }

    /// <summary>[DRAW_ALUS_14_OSA_2].</summary>
    public GlDisplayList BulldogRear { get; }

    /// <summary>[DRAW_ALUS_15_OSA_1].</summary>
    public GlDisplayList WiiiiBase { get; }

    /// <summary>[DRAW_ALUS_15_OSA_2].</summary>
    public GlDisplayList WiiiiTower { get; }

    /// <summary>[DRAW_ALUS_15_OSA_3].</summary>
    public GlDisplayList WiiiiTop { get; }

    /// <summary>[DRAW_ALUS_16_OSA_1].</summary>
    public GlDisplayList ElephantHead { get; }

    /// <summary>[DRAW_ALUS_16_OSA_2].</summary>
    public GlDisplayList ElephantTrunk { get; }

    /// <summary>[DRAW_ALUS_17].</summary>
    public GlDisplayList CubixBody { get; }

    /// <summary>[DRAW_ALUS_18].</summary>
    public GlDisplayList ElectraBody { get; }

    /// <summary>[DRAW_ALUS_18_SALAMA].</summary>
    public GlDisplayList ElectraSpark { get; }

    /// <summary>[DRAW_ALUS_19].</summary>
    public GlDisplayList TonguBody { get; }

    /// <summary>[DRAW_ALUS_20].</summary>
    public GlDisplayList SpaceTurtleBody { get; }

    /// <summary>[DRAW_ALUS_21_NORMAL].</summary>
    public GlDisplayList StickInsectBody { get; }

    /// <summary>[DRAW_ALUS_21_SHADOW].</summary>
    public GlDisplayList StickInsectShadow { get; }

    /// <summary>[DRAW_ALUS_22].</summary>
    public GlDisplayList StarWormHead { get; }

    /// <summary>[DRAW_ALUS_23].</summary>
    public GlDisplayList PopperBody { get; }
}
