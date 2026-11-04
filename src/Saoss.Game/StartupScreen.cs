namespace Saoss.Game;

/// <summary>
/// The screen the game opens with: normally the main menu, or a developer view chosen on the command line.
/// </summary>
public enum StartupScreen
{
    /// <summary>The main menu.</summary>
    MainMenu,

    /// <summary>The ship model gallery (<c>--ship-gallery</c>).</summary>
    ShipGallery,

    /// <summary>A Flower Domination match that ends at once (<c>--victory-preview</c>).</summary>
    VictoryPreview,

    /// <summary>A credits game that ends at once (<c>--victory-preview-credits</c>).</summary>
    CreditsVictoryPreview,
}
