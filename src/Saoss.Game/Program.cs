using Saoss.Game;
using Saoss.Game.Settings;

const string settingsDirOption = "--settings-dir";

var startup = args switch
{
    _ when args.Contains("--ship-gallery") => StartupScreen.ShipGallery,
    _ when args.Contains("--victory-preview") => StartupScreen.VictoryPreview,
    _ when args.Contains("--victory-preview-credits") => StartupScreen.CreditsVictoryPreview,
    _ => StartupScreen.MainMenu,
};

// "--settings-dir <path>" keeps test runs away from the player's own settings file.
var settingsDirIndex = Array.IndexOf(args, settingsDirOption);
var settingsDirectory = settingsDirIndex >= 0 && settingsDirIndex + 1 < args.Length
    ? args[settingsDirIndex + 1]
    : SettingsStore.DefaultDirectory;

using var game = new SaossGame(new SettingsStore(settingsDirectory), startup);
game.Run();
