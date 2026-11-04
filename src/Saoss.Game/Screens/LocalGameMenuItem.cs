namespace Saoss.Game.Screens;

/// <summary>
/// The entries of the local game menu (localgame.cpp onePlayerGameMenu), in menu order.
/// </summary>
public enum LocalGameMenuItem
{
    /// <summary>"Start With Random Ships".</summary>
    StartWithRandomShips,

    /// <summary>"Resume Game": continues the previous match where it was left.</summary>
    ResumeGame,

    /// <summary>"Start With &lt;credits&gt; Credits".</summary>
    StartWithCredits,

    /// <summary>"Start With Credits &amp; Random Ships".</summary>
    StartWithCreditsAndRandomShips,

    /// <summary>"Flower Domination".</summary>
    FlowerDomination,

    /// <summary>"Capture The Flower".</summary>
    CaptureTheFlower,

    /// <summary>"Eat The Mushroom".</summary>
    EatTheMushroom,

    /// <summary>"Settings".</summary>
    Settings,

    /// <summary>"Exit Menu": back to the main menu.</summary>
    ExitMenu,
}
