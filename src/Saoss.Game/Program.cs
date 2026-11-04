using System.Globalization;
using Saoss.Game;
using Saoss.Game.Settings;

// Developer options:
//   --skip-intro                 open the main menu instead of the intro
//   --settings-dir <path>        read and write settings.json there, keeping test runs away from the player's own
//   --ship-gallery               the ship model gallery
//   --victory-preview            a Flower Domination match that ends at once
//   --victory-preview-credits    a credits game that ends at once
//   --host                       host a network game at once (saved nicknames and player count), skipping the menus
//   --auto-start <n>             with --host: start the first match once n human players are in the lobby
//   --join <host>                connect to a host at once with the saved nicknames and player count
const string settingsDirOption = "--settings-dir";
const string joinOption = "--join";
const string autoStartOption = "--auto-start";

string? ValueOf(string option)
{
    var index = Array.IndexOf(args, option);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

var joinHost = ValueOf(joinOption);
var screen = args switch
{
    _ when args.Contains("--ship-gallery") => StartupScreen.ShipGallery,
    _ when args.Contains("--victory-preview") => StartupScreen.VictoryPreview,
    _ when args.Contains("--victory-preview-credits") => StartupScreen.CreditsVictoryPreview,
    _ when args.Contains("--host") => StartupScreen.HostLobby,
    _ when joinHost != null && HostAddress.IsValid(joinHost) => StartupScreen.Join,
    _ when args.Contains("--skip-intro") => StartupScreen.MainMenu,
    _ => StartupScreen.Intro,
};

var autoStart = int.TryParse(ValueOf(autoStartOption), NumberStyles.None, CultureInfo.InvariantCulture, out var players) && players > 0 ? players : (int?)null;
var startup = new StartupOptions(screen) { JoinHost = joinHost, AutoStartPlayers = autoStart };
var settingsDirectory = ValueOf(settingsDirOption) ?? SettingsStore.DefaultDirectory;

using var game = new SaossGame(new SettingsStore(settingsDirectory), startup);
game.Run();
