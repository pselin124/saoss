using System.Globalization;
using Saoss.Game;
using Saoss.Game.Diagnostics;
using Saoss.Game.Settings;

// Developer options:
//   --skip-intro                 open the main menu instead of the intro
//   --settings-dir <path>        read and write settings.json there, keeping test runs away from the player's own
//   --ship-gallery               the ship model gallery
//   --controls-menu              the controls menu (settings menu > Controls); leaving it returns to the settings
//   --victory-preview            a Flower Domination match that ends at once
//   --victory-preview-credits    a credits game that ends at once
//   --winners <n>                with a victory preview: n ships on the winning team
//   --host                       host a network game at once (saved nicknames and player count), skipping the menus
//   --auto-start <n>             with --host: start the first match once n human players are in the lobby
//   --join <host>                connect to a host at once with the saved nicknames and player count
//   --perf                       frame statistics in the window title and on standard output, once a second (vsync
//                                and frame rate cap stay as in normal play)
//   --bench <seconds>            measure the startup screen uncapped (no vsync, one update per frame) at 1920x1080 after
//                                a 2 s warm-up, append the result to bench.txt in the settings folder and exit
//   --bench-match <n>            start with an all-AI random-ships match of n ships with a fixed seed (for --bench)
//   --detail <0-3>               detail level for this run
//   --capture <png>              run uncapped with one update per frame and fixed seeds, save frame
//                                --capture-frame <n> (default 300) as a PNG and exit; for pixel comparisons
// Unhandled exceptions are appended to crash.log in the settings folder.
const string settingsDirOption = "--settings-dir";
const string joinOption = "--join";
const string autoStartOption = "--auto-start";
const string winnersOption = "--winners";
const string benchOption = "--bench";
const string benchMatchOption = "--bench-match";
const string detailOption = "--detail";
const string perfOption = "--perf";
const string captureOption = "--capture";
const string captureFrameOption = "--capture-frame";
const string benchFileName = "bench.txt";

string? ValueOf(string option)
{
    var index = Array.IndexOf(args, option);
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

int? IntValueOf(string option) =>
    int.TryParse(ValueOf(option), NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : null;

var joinHost = ValueOf(joinOption);
var benchPlayers = IntValueOf(benchMatchOption);
var screen = args switch
{
    _ when benchPlayers != null => StartupScreen.BenchmarkMatch,
    _ when args.Contains("--ship-gallery") => StartupScreen.ShipGallery,
    _ when args.Contains("--controls-menu") => StartupScreen.ControlsMenu,
    _ when args.Contains("--victory-preview") => StartupScreen.VictoryPreview,
    _ when args.Contains("--victory-preview-credits") => StartupScreen.CreditsVictoryPreview,
    _ when args.Contains("--host") => StartupScreen.HostLobby,
    _ when joinHost != null && HostAddress.IsValid(joinHost) => StartupScreen.Join,
    _ when args.Contains("--skip-intro") => StartupScreen.MainMenu,
    _ => StartupScreen.Intro,
};

var autoStart = IntValueOf(autoStartOption) is > 0 and var players ? players : (int?)null;
var settingsDirectory = ValueOf(settingsDirOption) ?? SettingsStore.DefaultDirectory;
CrashReporter.Install(settingsDirectory);
var detail = IntValueOf(detailOption);
var benchSeconds = double.TryParse(ValueOf(benchOption), NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) && seconds > 0 ? seconds : (double?)null;
var shipsLabel = benchPlayers is { } n ? $" {n} ships" : string.Empty;
var detailLabel = detail is { } d ? $" detail {d}" : string.Empty;
var benchLabel = $"{screen}{shipsLabel}{detailLabel}";
var startup = new StartupOptions(screen)
{
    JoinHost = joinHost,
    AutoStartPlayers = autoStart,
    PreviewWinners = IntValueOf(winnersOption) ?? VictoryPreview.DefaultWinners,
    BenchmarkPlayers = benchPlayers ?? GameSettings.MaxPlayers,
    Detail = detail,
    ShowPerformance = args.Contains(perfOption),
    Benchmark = benchSeconds is { } s ? new BenchmarkOptions(s, benchLabel, Path.Combine(settingsDirectory, benchFileName)) : null,
    Capture = ValueOf(captureOption) is { } capturePath
        ? new FrameCapture(Path.GetFullPath(capturePath), IntValueOf(captureFrameOption) ?? FrameCapture.DefaultFrame)
        : null,
};

using var game = new SaossGame(new SettingsStore(settingsDirectory), startup);
game.Run();
