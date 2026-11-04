namespace Saoss.Game.Settings;

/// <summary>
/// The resolutions the settings menu offers: the display's modes (the original's SDL_ListModes list), the desktop
/// size and common widescreen sizes, limited to what fits the desktop and sorted largest first like SDL's list.
/// Right moves toward larger sizes, Left toward smaller, wrapping at both ends.
/// </summary>
public static class ResolutionOptions
{
    private static readonly Resolution[] CommonSizes = [new(1280, 720), new(1600, 900), new(1920, 1080), new(2560, 1440)];

    /// <summary>Builds the list from the display's modes and the desktop size.</summary>
    public static IReadOnlyList<Resolution> Build(IEnumerable<Resolution> displayModes, Resolution desktop) =>
    [
        .. displayModes
            .Append(desktop)
            .Concat(CommonSizes)
            .Where(r => r.Width <= desktop.Width && r.Height <= desktop.Height)
            .Distinct()
            .OrderByDescending(r => r.Width)
            .ThenByDescending(r => r.Height),
    ];

    /// <summary>
    /// Position of <paramref name="current"/> in the list, or of the first size no wider than it like the original's
    /// start-up search; the smallest size if every size is wider.
    /// </summary>
    public static int IndexOf(IReadOnlyList<Resolution> options, Resolution current)
    {
        for (var i = 0; i < options.Count; i++)
        {
            if (options[i] == current)
            {
                return i;
            }
        }

        for (var i = 0; i < options.Count; i++)
        {
            if (options[i].Width <= current.Width)
            {
                return i;
            }
        }

        return options.Count - 1;
    }

    /// <summary>The next position: one larger size when <paramref name="larger"/> is set, otherwise one smaller.</summary>
    public static int Step(int index, int count, bool larger) => larger
        ? (index <= 0 ? count - 1 : index - 1)
        : (index >= count - 1 ? 0 : index + 1);
}
