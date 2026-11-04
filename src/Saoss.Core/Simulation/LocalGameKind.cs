namespace Saoss.Core.Simulation;

/// <summary>
/// The new-game entries of the original local game menu. Values are the original menu positions;
/// position 1 ("Resume Game") does not start a new game and has no entry.
/// </summary>
public enum LocalGameKind
{
    /// <summary>"Start With Random Ships": random-ships deathmatch, a new random ship on every respawn, no end.</summary>
    RandomShips = 0,

    /// <summary>"Start With &lt;credits&gt; Credits": credits game where players buy each ship from the purchase menu.</summary>
    CreditsChooseShips = 2,

    /// <summary>"Start With Credits &amp; Random Ships": credits game where each ship is a random affordable one.</summary>
    CreditsRandomShips = 3,

    /// <summary>"Flower Domination".</summary>
    FlowerDomination = 4,

    /// <summary>"Capture The Flower".</summary>
    CaptureTheFlower = 5,

    /// <summary>"Eat The Mushroom".</summary>
    EatTheMushroom = 6,
}
