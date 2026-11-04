using System.Text;
using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Network.Lobby;

namespace Saoss.Network.Protocol;

/// <summary>Wire format of the larger message parts: strings, match settings and the lobby.</summary>
internal static class SettingsCodec
{
    private const int MaxMatchCredits = 1_000_000;
    private const int AiTargetsFlag = 1;
    private const int UsesCreditsFlag = 2;
    private const int ChooseShipsFlag = 4;
    private const int ChoicesPauseFlag = 8;
    private const int AllFlags = 15;

    /// <summary>Writes a UTF-8 string with a ushort byte length.</summary>
    public static void WriteString(BinaryWriter writer, string text, int maxChars)
    {
        if (text.Length > maxChars)
        {
            throw new ArgumentException($"Text is longer than {maxChars} characters.", nameof(text));
        }

        var bytes = Encoding.UTF8.GetBytes(text);
        writer.Write((ushort)bytes.Length);
        writer.Write(bytes);
    }

    /// <summary>Reads a nickname, which must be valid, or empty when <paramref name="allowEmpty"/> is set.</summary>
    public static string ReadNickname(WireReader reader, bool allowEmpty)
    {
        var text = reader.ReadString(Nickname.MaxLength, "nickname");
        return Nickname.IsValid(text) || (allowEmpty && text.Length == 0) ? text : throw new ProtocolException("Invalid nickname.");
    }

    /// <summary>Writes match settings.</summary>
    public static void WriteSettings(BinaryWriter writer, MatchSettings settings)
    {
        writer.Write((byte)settings.Players.Count);
        foreach (var player in settings.Players)
        {
            writer.Write((byte)player.Team);
            writer.Write(player.IsHuman);
        }

        writer.Write((byte)settings.Mode);
        writer.Write((byte)((settings.AiTargetsOnlyEnemies ? AiTargetsFlag : 0)
            | (settings.UsesCredits ? UsesCreditsFlag : 0)
            | (settings.PlayersChooseShips ? ChooseShipsFlag : 0)
            | (settings.ChoicesPauseMatch ? ChoicesPauseFlag : 0)));
        writer.Write(settings.Credits);
    }

    /// <summary>Reads match settings.</summary>
    public static MatchSettings ReadSettings(WireReader reader)
    {
        var players = new PlayerSetup[reader.ReadByte(GameConstants.MaxPlayers, "player count")];
        if (players.Length == 0)
        {
            throw WireReader.Invalid("player count", 0);
        }

        for (var i = 0; i < players.Length; i++)
        {
            players[i] = new PlayerSetup(reader.ReadByte(GameConstants.MaxPlayers - 1, "team"), reader.ReadBool("human flag"));
        }

        var mode = reader.ReadEnum<GameMode>("game mode");
        var flags = reader.ReadByte(AllFlags, "settings flags");
        return new MatchSettings
        {
            Players = players,
            Mode = mode,
            AiTargetsOnlyEnemies = (flags & AiTargetsFlag) != 0,
            UsesCredits = (flags & UsesCreditsFlag) != 0,
            PlayersChooseShips = (flags & ChooseShipsFlag) != 0,
            ChoicesPauseMatch = (flags & ChoicesPauseFlag) != 0,
            Credits = reader.ReadInt(0, MaxMatchCredits, "credits"),
        };
    }

    /// <summary>Writes the lobby's settings and slots.</summary>
    public static void WriteLobby(BinaryWriter writer, LobbyState lobby)
    {
        writer.Write((byte)lobby.MaxPlayers);
        writer.Write((byte)lobby.Mode);
        writer.Write(lobby.Credits);
        writer.Write(lobby.AiTargetsOnlyEnemies);
        foreach (var slot in lobby.Slots)
        {
            writer.Write((byte)slot.Status);
            writer.Write((byte)slot.Team);
            writer.Write((sbyte)slot.OwnerId);
            WriteString(writer, slot.Nickname, Nickname.MaxLength);
        }
    }

    /// <summary>Reads a lobby copy written by <see cref="WriteLobby"/>.</summary>
    public static LobbyState ReadLobby(WireReader reader)
    {
        var maxPlayers = reader.ReadByte(LobbyState.SlotCount, "maximum players");
        if (maxPlayers < LobbyState.MinPlayers)
        {
            throw WireReader.Invalid("maximum players", maxPlayers);
        }

        var mode = reader.ReadEnum<GameMode>("game mode");
        var credits = reader.ReadInt(LobbyState.MinCredits, LobbyState.MaxCredits, "credits");
        var aiTargets = reader.ReadBool("AI target flag");
        var slots = new LobbySlot[LobbyState.SlotCount];
        for (var i = 0; i < slots.Length; i++)
        {
            slots[i] = ReadSlot(reader);
        }

        return new LobbyState(maxPlayers, mode, credits, aiTargets, slots);
    }

    private static LobbySlot ReadSlot(WireReader reader)
    {
        var status = reader.ReadEnum<SlotStatus>("slot status");
        var team = reader.ReadByte(LobbyState.SlotCount - 1, "team");
        var owner = (sbyte)reader.ReadByte(byte.MaxValue, "slot owner");
        var human = status == SlotStatus.Human;
        if (human ? owner < LobbyState.HostId || owner >= LobbyState.SlotCount : owner != LobbyState.NoOwner)
        {
            throw WireReader.Invalid("slot owner", owner);
        }

        var nickname = ReadNickname(reader, allowEmpty: !human);
        return human || nickname.Length == 0 ? new LobbySlot(status, team, nickname, owner) : throw new ProtocolException("Nickname on a slot without a human.");
    }
}
