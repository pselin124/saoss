using Saoss.Game;

var startup = args switch
{
    _ when args.Contains("--ship-gallery") => StartupScreen.ShipGallery,
    _ when args.Contains("--victory-preview") => StartupScreen.VictoryPreview,
    _ when args.Contains("--victory-preview-credits") => StartupScreen.CreditsVictoryPreview,
    _ => StartupScreen.MainMenu,
};
using var game = new SaossGame(startup);
game.Run();
