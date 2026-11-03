using Saoss.Game;

using var game = new SaossGame(showShipGallery: args.Contains("--ship-gallery"));
game.Run();
