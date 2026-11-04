namespace Saoss.Game;

/// <summary>
/// The screen the game opens with: normally the intro, or the main menu or a developer view chosen on the command line.
/// </summary>
public enum StartupScreen
{
    /// <summary>The intro scene, followed by the main menu.</summary>
    Intro,

    /// <summary>The main menu, skipping the intro (<c>--skip-intro</c>).</summary>
    MainMenu,

    /// <summary>The controls menu inside the settings menu (<c>--controls-menu</c>).</summary>
    ControlsMenu,

    /// <summary>The ship model gallery (<c>--ship-gallery</c>).</summary>
    ShipGallery,

    /// <summary>A Flower Domination match that ends at once (<c>--victory-preview</c>).</summary>
    VictoryPreview,

    /// <summary>A credits game that ends at once (<c>--victory-preview-credits</c>).</summary>
    CreditsVictoryPreview,

    /// <summary>The host lobby with the saved network settings (<c>--host</c>).</summary>
    HostLobby,

    /// <summary>Connecting to <see cref="StartupOptions.JoinHost"/> (<c>--join &lt;host&gt;</c>).</summary>
    Join,

    /// <summary>An all-AI random-ships match for benchmarks (<c>--bench-match &lt;players&gt;</c>).</summary>
    BenchmarkMatch,
}
