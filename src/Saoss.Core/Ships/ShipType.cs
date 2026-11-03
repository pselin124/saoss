namespace Saoss.Core.Ships;

/// <summary>
/// The 24 ship types. The numeric value is the original ship index, which also selected the
/// primary and special weapon behavior in the original code.
/// </summary>
public enum ShipType
{
    /// <summary>Twin magnetic guns, shield special.</summary>
    Magneto = 0,

    /// <summary>Heavy single shot with recoil.</summary>
    Tringu = 1,

    /// <summary>"Amppari": small and fast with a rapid-fire gun.</summary>
    Wasp = 2,

    /// <summary>Alternating side guns.</summary>
    Subni = 3,

    /// <summary>Heavy ship with a propeller and a long-lived shot.</summary>
    Rollu = 4,

    /// <summary>Two big slow shots.</summary>
    Trek = 5,

    /// <summary>"Uhvo": inertia-free saucer with a short-range laser.</summary>
    Ufo = 6,

    /// <summary>Very slow, long-lived shots.</summary>
    Bomba = 7,

    /// <summary>Rapid stream of small shots.</summary>
    Rayska = 8,

    /// <summary>"Rauhantykki": short-range area weapon without projectiles.</summary>
    PeaceCannon = 9,

    /// <summary>Medium gun with a large-area special.</summary>
    Bloddu = 10,

    /// <summary>Gun and a shielded slow mode.</summary>
    Shapperi = 11,

    /// <summary>"Perho": may come back to life after being destroyed.</summary>
    Moth = 12,

    /// <summary>"VoimaImuri": shot that is released when fire is let go.</summary>
    PowerVacuum = 13,

    /// <summary>Steerable slow shots, ramming special.</summary>
    Bulldog = 14,

    /// <summary>Boost mode that burns fuel continuously.</summary>
    Wiiii = 15,

    /// <summary>"Fantti": the heaviest ship, with a propeller.</summary>
    Elephant = 16,

    /// <summary>The lightest ship.</summary>
    Cubix = 17,

    /// <summary>Inertia-free ship with an electric weapon.</summary>
    Electra = 18,

    /// <summary>Special that damages ships it touches.</summary>
    Tongu = 19,

    /// <summary>"Avaruuskilppari": fires a ring of shots.</summary>
    SpaceTurtle = 20,

    /// <summary>"Sauvasirkka".</summary>
    StickInsect = 21,

    /// <summary>"Tahtimato": always moving, segmented tail.</summary>
    StarWorm = 22,

    /// <summary>"Poksu".</summary>
    Popper = 23,
}
