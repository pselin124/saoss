using Saoss.Core.Ships;
using Saoss.Core.Simulation;
using Saoss.Game.Audio;

namespace Saoss.Tests.Audio;

/// <summary>
/// Tests the ship sound table against the original's alota() sound assignments and setSoundVolumes().
/// </summary>
[TestClass]
public sealed class ShipSoundTableTests
{
    [TestMethod]
    [DataRow(ShipType.Magneto, ShipSound.Fire, "magneto-fire.wav", 0.5f)]
    [DataRow(ShipType.Magneto, ShipSound.SpecialFire, "magneto-special-fire.wav", 0.75f)]
    [DataRow(ShipType.Tringu, ShipSound.Fire, "tringu-fire.wav", 1.0f)]
    [DataRow(ShipType.Wasp, ShipSound.SpecialFire, "wasp-special-fire.wav", 0.5f)]
    [DataRow(ShipType.Ufo, ShipSound.SpecialFire, "ufo-special-fire.wav", 0.125f)]
    [DataRow(ShipType.Cubix, ShipSound.SpecialHit, "cubix-special-hit.wav", 1.0f)]
    public void OwnSounds_UseTheShipsFileAndVolume(ShipType ship, ShipSound sound, string file, float volume) =>
        Assert.AreEqual(new ShipSoundSpec(file, volume), ShipSoundTable.Get(ship, sound));

    [TestMethod]
    [DataRow(ShipType.Magneto, ShipSound.Hit, "magneto-fire.wav", 0.5f)]
    [DataRow(ShipType.Tringu, ShipSound.SpecialHit, "magneto-fire.wav", 0.5f)]
    [DataRow(ShipType.Twinblade, ShipSound.SpecialFire, "tringu-fire.wav", 0.5f)]
    [DataRow(ShipType.ScatterRay, ShipSound.SpecialFire, "trek-fire.wav", 0.75f)]
    [DataRow(ShipType.Shapper, ShipSound.Hit, "vampire-fire.wav", 0.5f)]
    [DataRow(ShipType.Moth, ShipSound.SpecialHit, "ufo-special-fire.wav", 0.75f)]
    [DataRow(ShipType.Electra, ShipSound.Hit, "cubix-hit.wav", 0f)]
    public void BorrowedSounds_UseTheOwnersFileWithTheirOwnVolume(ShipType ship, ShipSound sound, string file, float volume) =>
        Assert.AreEqual(new ShipSoundSpec(file, volume), ShipSoundTable.Get(ship, sound));

    [TestMethod]
    [DataRow(ShipType.Magneto, ShipSound.SpecialHit)]
    [DataRow(ShipType.Wasp, ShipSound.SpecialHit)]
    [DataRow(ShipType.Ufo, ShipSound.Hit)]
    [DataRow(ShipType.Ufo, ShipSound.SpecialHit)]
    [DataRow(ShipType.Thunderbolt, ShipSound.SpecialHit)]
    public void NullSounds_HaveNoEntry(ShipType ship, ShipSound sound) =>
        Assert.IsNull(ShipSoundTable.Get(ship, sound));

    [TestMethod]
    public void PeaceCannonFire_IsTheOnlyLoopingSample()
    {
        var looping = Enum.GetValues<ShipType>()
            .SelectMany(ship => Enum.GetValues<ShipSound>().Select(sound => (ship, sound)))
            .Where(key => ShipSoundTable.Get(key.ship, key.sound)?.Loops == true)
            .ToArray();

        CollectionAssert.AreEqual(new[] { (ShipType.PeaceCannon, ShipSound.Fire) }, looping);
    }

    [TestMethod]
    [DataRow(ShipType.StickInsect)]
    [DataRow(ShipType.StarWorm)]
    [DataRow(ShipType.Popper)]
    public void LastThreeShips_AreMuted(ShipType ship)
    {
        foreach (var sound in Enum.GetValues<ShipSound>())
        {
            Assert.AreEqual(0f, ShipSoundTable.Get(ship, sound)!.Volume);
        }
    }

    [TestMethod]
    public void EveryFile_IsInTheContentFolder()
    {
        var folder = Path.Combine(AppContext.BaseDirectory, "Content", "Sounds");
        Assert.HasCount(37, ShipSoundTable.Files);
        foreach (var file in ShipSoundTable.Files)
        {
            Assert.IsTrue(File.Exists(Path.Combine(folder, file)), file);
        }
    }
}
