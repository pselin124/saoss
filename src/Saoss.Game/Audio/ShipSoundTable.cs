using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Game.Audio;

/// <summary>
/// The sound set of every ship type, a port of the original's <c>alota()</c> sound assignments and
/// <c>setSoundVolumes()</c> (Alustuksia.cpp). Files are named after the ship whose original file they are
/// (a_N_xxx.wav belongs to ship N), so a ship that borrowed another ship's sound points at that ship's file.
/// A missing entry is a sound the original left NULL. The original's per-load setVolume(128)/setVolume(32) calls
/// are not ported: setSoundVolumes ran right after them and overwrote every one.
/// </summary>
public static class ShipSoundTable
{
    private const string MagnetoFire = "magneto-fire.wav";
    private const string MagnetoSpecialFire = "magneto-special-fire.wav";
    private const string TringuFire = "tringu-fire.wav";
    private const string TringuHit = "tringu-hit.wav";
    private const string WaspSpecialFire = "wasp-special-fire.wav";
    private const string SubniFire = "subni-fire.wav";
    private const string SubniSpecialHit = "subni-special-hit.wav";
    private const string RolluFire = "rollu-fire.wav";
    private const string RolluSpecialFire = "rollu-special-fire.wav";
    private const string RolluSpecialHit = "rollu-special-hit.wav";
    private const string TrekFire = "trek-fire.wav";
    private const string TrekHit = "trek-hit.wav";
    private const string UfoSpecialFire = "ufo-special-fire.wav";
    private const string BombaFire = "bomba-fire.wav";
    private const string PeaceCannonFire = "peace-cannon-fire.wav";
    private const string BlodduFire = "bloddu-fire.wav";
    private const string BlodduSpecialFire = "bloddu-special-fire.wav";
    private const string ShapperiFire = "shapperi-fire.wav";
    private const string ShapperiSpecialHit = "shapperi-special-hit.wav";
    private const string PowerVacuumFire = "power-vacuum-fire.wav";
    private const string PowerVacuumHit = "power-vacuum-hit.wav";
    private const string PowerVacuumSpecialFire = "power-vacuum-special-fire.wav";
    private const string BulldogFire = "bulldog-fire.wav";
    private const string BulldogHit = "bulldog-hit.wav";
    private const string BulldogSpecialFire = "bulldog-special-fire.wav";
    private const string WiiiiFire = "wiiii-fire.wav";
    private const string WiiiiSpecialFire = "wiiii-special-fire.wav";
    private const string ElephantFire = "elephant-fire.wav";
    private const string ElephantHit = "elephant-hit.wav";
    private const string ElephantSpecialFire = "elephant-special-fire.wav";
    private const string CubixFire = "cubix-fire.wav";
    private const string CubixHit = "cubix-hit.wav";
    private const string CubixSpecialFire = "cubix-special-fire.wav";
    private const string CubixSpecialHit = "cubix-special-hit.wav";
    private const string ElectraFire = "electra-fire.wav";
    private const string ElectraSpecialFire = "electra-special-fire.wav";
    private const string SpaceTurtleFire = "space-turtle-fire.wav";

    // Indexed by ShipType, then by ShipSound (Fire, Hit, SpecialFire, SpecialHit).
    private static readonly ShipSoundSpec?[][] Rows =
    [
        /* Magneto */ [S(MagnetoFire, 0.5f), S(MagnetoFire, 0.5f), S(MagnetoSpecialFire, 0.75f), null],
        /* Tringu */ [S(TringuFire, 1.0f), S(TringuHit, 1.0f), S(MagnetoFire, 0.5f), S(MagnetoFire, 0.5f)],
        /* Wasp */ [S(MagnetoFire, 0.5f), S(MagnetoFire, 0.5f), S(WaspSpecialFire, 0.5f), null],
        /* Subni */ [S(SubniFire, 0.75f), S(TringuHit, 0.5f), S(TringuFire, 0.5f), S(SubniSpecialHit, 0.75f)],
        /* Rollu */ [S(RolluFire, 0.9f), S(MagnetoFire, 0.5f), S(RolluSpecialFire, 0.75f), S(RolluSpecialHit, 0.75f)],
        /* Trek */ [S(TrekFire, 0.75f), S(TrekHit, 0.75f), S(MagnetoFire, 0.5f), S(MagnetoFire, 0.5f)],
        /* Ufo */ [S(MagnetoFire, 0.5f), null, S(UfoSpecialFire, 0.125f), null],
        /* Bomba */ [S(BombaFire, 0.5f), S(TringuHit, 1.0f), S(MagnetoFire, 0.5f), null],
        /* Rayska */ [S(MagnetoFire, 0.75f), S(MagnetoFire, 0.5f), S(TrekFire, 0.75f), S(TrekHit, 0.75f)],
        /* PeaceCannon */ [new(PeaceCannonFire, 0.75f, Loops: true), S(MagnetoFire, 0.5f), S(TrekFire, 0.75f), S(TrekHit, 0.75f)],
        /* Bloddu */ [S(BlodduFire, 0.5f), S(BlodduFire, 0.5f), S(BlodduSpecialFire, 0.8f), S(MagnetoFire, 0.3f)],
        /* Shapperi */ [S(ShapperiFire, 0.5f), S(BlodduFire, 0.5f), S(MagnetoFire, 0.3f), S(ShapperiSpecialHit, 0.25f)],
        /* Moth */ [S(MagnetoFire, 0.34f), S(MagnetoFire, 0.34f), S(MagnetoSpecialFire, 0.5f), S(UfoSpecialFire, 0.75f)],
        /* PowerVacuum */ [S(PowerVacuumFire, 0.6f), S(PowerVacuumHit, 0.8f), S(PowerVacuumSpecialFire, 0.6f), S(MagnetoFire, 0f)],
        /* Bulldog */ [S(BulldogFire, 0.99f), S(BulldogHit, 0.6f), S(BulldogSpecialFire, 0.99f), S(MagnetoFire, 0f)],
        /* Wiiii */ [S(WiiiiFire, 1.0f), S(MagnetoFire, 0f), S(WiiiiSpecialFire, 1.0f), S(MagnetoFire, 0f)],
        /* Elephant */ [S(ElephantFire, 0.8f), S(ElephantHit, 0.7f), S(ElephantSpecialFire, 0.8f), S(MagnetoFire, 0.5f)],
        /* Cubix */ [S(CubixFire, 0.6f), S(CubixHit, 1.0f), S(CubixSpecialFire, 0.6f), S(CubixSpecialHit, 1.0f)],
        /* Electra */ [S(ElectraFire, 0.8f), S(CubixHit, 0f), S(ElectraSpecialFire, 0.6f), S(CubixSpecialHit, 0f)],
        /* Tongu */ [S(MagnetoFire, 0.5f), S(MagnetoFire, 0.5f), S(CubixFire, 0.6f), S(TringuHit, 1.0f)],
        /* SpaceTurtle */ [S(SpaceTurtleFire, 0.7f), S(TringuHit, 0.9f), S(MagnetoFire, 0.5f), S(MagnetoFire, 0.5f)],
        /* StickInsect */ [S(MagnetoFire, 0f), S(MagnetoFire, 0f), S(MagnetoFire, 0f), S(MagnetoFire, 0f)],
        /* StarWorm */ [S(MagnetoFire, 0f), S(MagnetoFire, 0f), S(MagnetoFire, 0f), S(MagnetoFire, 0f)],
        /* Popper */ [S(MagnetoFire, 0f), S(MagnetoFire, 0f), S(MagnetoFire, 0f), S(MagnetoFire, 0f)],
    ];

    /// <summary>Every distinct sound file the table uses.</summary>
    public static IReadOnlyList<string> Files { get; } =
        [.. Rows.SelectMany(row => row).OfType<ShipSoundSpec>().Select(spec => spec.File).Distinct()];

    /// <summary>The sound <paramref name="sound"/> of <paramref name="ship"/>, or null when the original had none.</summary>
    public static ShipSoundSpec? Get(ShipType ship, ShipSound sound) => Rows[(int)ship][(int)sound];

    private static ShipSoundSpec S(string file, float volume) => new(file, volume);
}
