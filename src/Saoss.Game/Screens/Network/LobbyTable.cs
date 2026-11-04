using Microsoft.Xna.Framework;
using Saoss.Network.Lobby;

namespace Saoss.Game.Screens.Network;

/// <summary>
/// Draws the lobby's slot table and game mode, shared by the host's and the clients' lobby screens
/// (net.cpp drawHostMPMenu and drawJoinWait): one row per slot in use with name, status and team, colored by
/// status, and the game mode at the bottom.
/// </summary>
public static class LobbyTable
{
    /// <summary>Depth of the lobby text.</summary>
    public const float TextZ = -60f;

    /// <summary>Distance of the lobby picture from the camera.</summary>
    public const float BackgroundDistance = 2f;

    /// <summary>Height of the first row.</summary>
    public const float TopY = 28f;

    /// <summary>Distance between rows.</summary>
    public const float RowSpacing = 3f;

    /// <summary>Height of the arrow at the first row.</summary>
    public const float ArrowTopY = 5.62f;

    /// <summary>Arrow movement per row.</summary>
    public const float ArrowRowStep = 0.6f;

    private const float NameX = -14f;
    private const float StatusX = 8f;
    private const float TeamX = 23f;
    private const float ModeY = -25f;
    private const float ModeLabelX = -37f;
    private const float ModeLabelSize = 1.2f;
    private const float ModeX = -13f;

    private static readonly Color OpenColor = new(0.2f, 1f, 0.2f);
    private static readonly Color ClosedColor = new(1f, 0.2f, 0.2f);
    private static readonly Color AiColor = new(1f, 0.2f, 1f);
    private static readonly Color HumanColor = new(0.2f, 0.2f, 1f);

    /// <summary>Height of row <paramref name="row"/>.</summary>
    public static float RowY(int row) => TopY - (row * RowSpacing);

    /// <summary>Draws the table and the game mode.</summary>
    public static void Draw(MenuScene scene, LobbyState lobby)
    {
        var names = LobbyTexts.SlotNames(lobby);
        for (var i = 0; i < lobby.MaxPlayers; i++)
        {
            var slot = lobby.Slots[i];
            var color = slot.Status switch
            {
                SlotStatus.Open => OpenColor,
                SlotStatus.Closed => ClosedColor,
                SlotStatus.Ai => AiColor,
                _ => HumanColor,
            };
            var y = RowY(i);
            scene.DrawText(new Vector3(StatusX, y, TextZ), LobbyTexts.Status(slot.Status), color);
            scene.DrawText(new Vector3(NameX, y, TextZ), names[i], color);
            scene.DrawText(new Vector3(TeamX, y, TextZ), LobbyTexts.Team(slot), color);
        }

        scene.DrawText(new Vector3(ModeLabelX, ModeY, TextZ), "game mode:", Color.Red, ModeLabelSize);
        scene.DrawText(new Vector3(ModeX, ModeY, TextZ), LobbyTexts.Mode(lobby.Mode), Color.Blue);
    }
}
