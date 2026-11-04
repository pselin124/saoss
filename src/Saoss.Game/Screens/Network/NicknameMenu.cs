using Microsoft.Xna.Framework;
using Saoss.Game.Input;
using Saoss.Game.Multiplayer;
using Saoss.Network.Protocol;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// The nickname menu (net.cpp nikinVaihtoMenu and drawNikinVaihtoMenu): "nickname 1:" to "nickname 4:" with
/// the current nicknames, then "exit". Choosing a nickname opens the "enter nickname:" entry; a valid result is
/// saved at once.
/// </summary>
public static class NicknameMenu
{
    private const float LabelX = -35f;
    private const float ValueX = -15f;
    private const float TopY = 8f;
    private const float LineSpacing = 3.5f;
    private const float TextZ = -60f;
    private const float ArrowX = -7.5f;
    private const float ArrowTopY = 1.6f;
    private const float ArrowStep = 0.7f;

    /// <summary>Builds the menu; <paramref name="close"/> runs on "exit".</summary>
    public static OptionMenuScreen Create(IGameShell shell, Action close)
    {
        OptionMenuScreen? menu = null;
        var entries = Enumerable.Range(0, shell.Settings.Nicknames.Count)
            .Select(i => new OptionMenuEntry($"nickname {i + 1}:", _ => menu!.Child.Open(CreateEntry(shell, i, menu.Child.Close)))
            {
                Value = () => shell.Settings.Nicknames[i],
                ValueX = ValueX,
            })
            .Append(new OptionMenuEntry("exit", _ => close()))
            .ToList();
        var layout = new OptionMenuLayout
        {
            LabelPosition = i => new Vector3(LabelX, TopY - (LineSpacing * i), TextZ),
            ArrowX = ArrowX,
            ArrowY = i => ArrowTopY - (ArrowStep * i),
            Background = false,
        };
        menu = new OptionMenuScreen(shell.GraphicsDevice, shell.Assets, layout, entries);
        return menu;
    }

    private static TextEntryScreen CreateEntry(IGameShell shell, int index, Action close)
    {
        var buffer = new TextInputBuffer(Nickname.MaxLength, c => Nickname.IsAllowed(c) ? c : null);
        buffer.SetText(shell.Settings.Nicknames[index]);
        return new TextEntryScreen(shell.GraphicsDevice, shell.Assets, "enter nickname:", buffer, text =>
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var nicknames = shell.Settings.Nicknames.ToArray();
                nicknames[index] = text;
                shell.ApplySettings(shell.Settings with { Nicknames = nicknames });
            }

            close();
        });
    }
}
