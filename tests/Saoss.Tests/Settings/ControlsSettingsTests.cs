using Microsoft.Xna.Framework.Input;
using Saoss.Game.Input;
using Saoss.Game.Settings;

namespace Saoss.Tests.Settings;

[TestClass]
public sealed class ControlsSettingsTests
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
    public void Defaults_AreTheOriginalLayoutsOnTheKeyboard()
    {
        var controls = new GameSettings().LocalControls();

        Assert.HasCount(GameSettings.MaxLocalHumans, controls);
        for (var i = 0; i < controls.Count; i++)
        {
            Assert.AreEqual(new PlayerControls(PlayerKeyBindings.Defaults[i], InputDevice.Keyboard), controls[i]);
        }
    }

    [TestMethod]
    public void SaveThenLoad_KeepsDevicesAndKeys_ByName()
    {
        var store = new SettingsStore(_directory);
        var controls = new[]
        {
            new PlayerControlSettings(InputDevice.Gamepad2, PlayerKeyBindings.Defaults[0] with { Fire = Keys.Space }),
            PlayerControlSettings.Default(1),
            new PlayerControlSettings(InputDevice.Gamepad1, PlayerKeyBindings.Defaults[2]),
            PlayerControlSettings.Default(3) with { Keys = PlayerKeyBindings.Defaults[3] with { Down = Keys.F5 } },
        };

        store.Save(new GameSettings { Controls = controls });
        var loaded = store.Load();

        CollectionAssert.AreEqual(controls, loaded.Controls.ToArray());
        var json = File.ReadAllText(store.FilePath);
        StringAssert.Contains(json, "\"Device\": \"Gamepad2\"");
        StringAssert.Contains(json, "\"Fire\": \"Space\"");
    }

    [TestMethod]
    public void Load_WithoutControls_GivesDefaults()
    {
        WriteSettings("{ \"Credits\": 300 }");

        var loaded = new SettingsStore(_directory).Load();

        CollectionAssert.AreEqual(new GameSettings().Controls.ToArray(), loaded.Controls.ToArray());
    }

    [TestMethod]
    public void Load_InvalidKeysAndDevices_FallBackOneByOne()
    {
        WriteSettings("""
            {
              "Credits": 300,
              "Controls": [
                { "Device": "Joystick7", "Keys": { "Thrust": "Q", "Left": "NoSuchKey", "Right": "Escape", "Fire": 12345,
                                                   "Special": true, "Down": "Enter" } },
                null,
                { "Device": "Gamepad3", "Keys": { "Fire": "Space" } },
                { "Device": "gamepad4" },
                { "Device": "Gamepad1" }
              ]
            }
            """);

        var loaded = new SettingsStore(_directory).Load();

        Assert.AreEqual(300, loaded.Credits, "One bad key does not throw the whole file away.");
        Assert.HasCount(GameSettings.MaxLocalHumans, loaded.Controls);
        var first = loaded.Controls[0];
        Assert.AreEqual(InputDevice.Keyboard, first.Device);
        Assert.AreEqual(PlayerKeyBindings.Defaults[0] with { Thrust = Keys.Q }, first.Keys);
        Assert.AreEqual(PlayerControlSettings.Default(1), loaded.Controls[1]);
        Assert.AreEqual(new PlayerControlSettings(InputDevice.Gamepad3, PlayerKeyBindings.Defaults[2] with { Fire = Keys.Space }), loaded.Controls[2]);
        Assert.AreEqual(new PlayerControlSettings(InputDevice.Gamepad4, PlayerKeyBindings.Defaults[3]), loaded.Controls[3]);
    }

    private void WriteSettings(string json)
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(Path.Combine(_directory, SettingsStore.FileName), json);
    }
}
