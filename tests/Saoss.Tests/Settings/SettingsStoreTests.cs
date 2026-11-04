using Saoss.Game.Rendering.Scene;
using Saoss.Game.Settings;

namespace Saoss.Tests.Settings;

/// <summary>
/// Tests loading and saving the settings file.
/// </summary>
[TestClass]
public sealed class SettingsStoreTests
{
    private string _directory = null!;

    [TestInitialize]
    public void CreateDirectory() => _directory = Path.Combine(Path.GetTempPath(), "saoss-tests-" + Guid.NewGuid().ToString("N"));

    [TestCleanup]
    public void DeleteDirectory()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [TestMethod]
    public void SaveThenLoad_KeepsEveryValue()
    {
        var store = new SettingsStore(_directory);
        var players = new GameSettings().Players.Select((p, i) => p with { Team = (i * 3) % 16, IsHuman = i is 1 or 3 }).ToArray();
        var saved = new GameSettings
        {
            Width = 1600,
            Height = 900,
            Fullscreen = true,
            PlayerCount = 7,
            MusicEnabled = false,
            SoundEffectsEnabled = false,
            Credits = 725,
            TicksPerSecond = 140,
            MusicVolume = 33,
            SoundVolume = 0,
            Players = players,
            AiTargetsOnlyEnemies = true,
            CameraMode = CameraMode.TopDown,
            Detail = 3,
            Nicknames = ["tgo", "cinere", "joku", "neljas"],
            LastHost = "192.168.0.4",
            NetworkPlayerCount = 3,
        };

        store.Save(saved);
        var loaded = store.Load();

        Assert.AreEqual(saved with { Players = loaded.Players, Nicknames = loaded.Nicknames }, loaded);
        CollectionAssert.AreEqual(players, loaded.Players.ToArray());
        CollectionAssert.AreEqual(saved.Nicknames.ToArray(), loaded.Nicknames.ToArray());
    }

    [TestMethod]
    public void Load_WithoutFile_GivesDefaults()
    {
        var loaded = new SettingsStore(_directory).Load();

        AssertDefaults(loaded);
    }

    [TestMethod]
    public void Load_CorruptFile_GivesDefaults()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, SettingsStore.FileName), "{ \"Width\": 1600, \"Players\": [ oops");

        AssertDefaults(new SettingsStore(_directory).Load());
    }

    [TestMethod]
    public void Load_WrongValueType_GivesDefaults()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, SettingsStore.FileName), "{ \"Width\": \"wide\" }");

        AssertDefaults(new SettingsStore(_directory).Load());
    }

    [TestMethod]
    public void Load_MissingValues_UseDefaults()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, SettingsStore.FileName), "{ \"Credits\": 300, \"Players\": null }");

        var loaded = new SettingsStore(_directory).Load();

        Assert.AreEqual(300, loaded.Credits);
        Assert.AreEqual(100, loaded.TicksPerSecond);
        CollectionAssert.AreEqual(new GameSettings().Players.ToArray(), loaded.Players.ToArray());
    }

    [TestMethod]
    public void Load_OutOfRangeValues_AreClamped()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, SettingsStore.FileName), """
            {
              "Width": 10, "Height": 99999, "PlayerCount": 40, "Credits": 5, "TicksPerSecond": 1000,
              "MusicVolume": -3, "SoundVolume": 250, "CameraMode": 7, "Detail": 9,
              "Players": [ { "IsHuman": true, "Team": 99 }, null, { "IsHuman": true, "Team": -2 },
                           { "IsHuman": true, "Team": 3 }, { "IsHuman": true, "Team": 4 } ],
              "Nicknames": [ "averyveryverylongname", "" ]
            }
            """);

        var loaded = new SettingsStore(_directory).Load();

        Assert.AreEqual(320, loaded.Width);
        Assert.AreEqual(16384, loaded.Height);
        Assert.AreEqual(GameSettings.MaxPlayers, loaded.PlayerCount);
        Assert.AreEqual(GameSettings.MinCredits, loaded.Credits);
        Assert.AreEqual(GameSettings.MaxTicksPerSecond, loaded.TicksPerSecond);
        Assert.AreEqual(0, loaded.MusicVolume);
        Assert.AreEqual(GameSettings.MaxVolume, loaded.SoundVolume);
        Assert.AreEqual(CameraMode.Tilted, loaded.CameraMode);
        Assert.AreEqual(GameSettings.MaxDetail, loaded.Detail);
        Assert.HasCount(GameSettings.MaxPlayers, loaded.Players);
        Assert.AreEqual(new PlayerSlotSettings(true, GameSettings.MaxTeam), loaded.Players[0]);
        Assert.AreEqual(new GameSettings().Players[1], loaded.Players[1]);
        Assert.AreEqual(new PlayerSlotSettings(true, 0), loaded.Players[2]);
        Assert.AreEqual(new PlayerSlotSettings(false, 4), loaded.Players[4], "Only players 1-4 can be human.");
        CollectionAssert.AreEqual(new[] { "averyveryv", "Player 2", "Player 3", "Player 4" }, loaded.Nicknames.ToArray());
    }

    [TestMethod]
    public void Load_InvalidNetworkValues_AreRepaired()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, SettingsStore.FileName), """
            { "Nicknames": [ "<b>bold</b>", "???", "ok name" ], "LastHost": "Bad Host!", "NetworkPlayerCount": 9 }
            """);

        var loaded = new SettingsStore(_directory).Load();

        CollectionAssert.AreEqual(new[] { "bboldb", "Player 2", "ok name", "Player 4" }, loaded.Nicknames.ToArray());
        Assert.AreEqual("localhost", loaded.LastHost);
        Assert.AreEqual(GameSettings.MaxNetworkPlayers, loaded.NetworkPlayerCount);
    }

    [TestMethod]
    public void Save_CreatesTheFolder()
    {
        var store = new SettingsStore(_directory);

        store.Save(new GameSettings());

        Assert.IsTrue(File.Exists(store.FilePath));
    }

    private static void AssertDefaults(GameSettings loaded)
    {
        var defaults = new GameSettings();
        Assert.AreEqual(defaults with { Players = loaded.Players, Nicknames = loaded.Nicknames }, loaded);
        CollectionAssert.AreEqual(defaults.Players.ToArray(), loaded.Players.ToArray());
        CollectionAssert.AreEqual(defaults.Nicknames.ToArray(), loaded.Nicknames.ToArray());
    }
}
