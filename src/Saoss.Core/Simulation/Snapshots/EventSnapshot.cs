using Saoss.Core.Ships;

namespace Saoss.Core.Simulation.Snapshots;

/// <summary>
/// Writes and reads <see cref="GameEvent"/>s, each as a kind byte followed by its fields.
/// </summary>
internal static class EventSnapshot
{
    private enum Kind : byte
    {
        ShipSoundPlayed,
        ShipLoopStopped,
        ShipExploded,
        ShipRevived,
        ShipRespawning,
        AsteroidExploded,
        Effect,
    }

    /// <summary>Writes one event.</summary>
    public static void Write(BinaryWriter writer, GameEvent gameEvent)
    {
        switch (gameEvent)
        {
            case ShipSoundPlayed e:
                writer.Write((byte)Kind.ShipSoundPlayed);
                writer.Write((byte)e.ShipIndex);
                writer.Write((byte)e.SoundOf);
                writer.Write((byte)e.Sound);
                writer.Write(e.Loop);
                break;
            case ShipLoopStopped e:
                WriteShipEvent(writer, Kind.ShipLoopStopped, e.ShipIndex);
                break;
            case ShipExploded e:
                WriteShipEvent(writer, Kind.ShipExploded, e.ShipIndex);
                PoolSnapshot.WriteFloats(writer, e.X, e.Y);
                break;
            case ShipRevived e:
                WriteShipEvent(writer, Kind.ShipRevived, e.ShipIndex);
                break;
            case ShipRespawning e:
                WriteShipEvent(writer, Kind.ShipRespawning, e.ShipIndex);
                break;
            case AsteroidExploded e:
                writer.Write((byte)Kind.AsteroidExploded);
                PoolSnapshot.WriteFloats(writer, e.X, e.Y);
                break;
            case EffectEvent e:
                writer.Write((byte)Kind.Effect);
                writer.Write((byte)e.Kind);
                PoolSnapshot.WriteFloats(writer, e.X, e.Y, e.VelocityX, e.VelocityY);
                break;
            default:
                throw new ArgumentException($"Unknown event type {gameEvent.GetType().Name}.", nameof(gameEvent));
        }
    }

    /// <summary>Reads one event written by <see cref="Write"/>.</summary>
    public static GameEvent Read(SnapshotReader reader, int shipCount)
    {
        var maxShip = shipCount - 1;
        return reader.ReadEnum<Kind>("event kind") switch
        {
            Kind.ShipSoundPlayed => new ShipSoundPlayed(
                reader.ReadByte(maxShip, "event ship"),
                reader.ReadEnum<ShipType>("event ship type"),
                reader.ReadEnum<ShipSound>("event sound"),
                reader.ReadBool("event loop")),
            Kind.ShipLoopStopped => new ShipLoopStopped(reader.ReadByte(maxShip, "event ship")),
            Kind.ShipExploded => new ShipExploded(reader.ReadByte(maxShip, "event ship"), reader.ReadFloat(), reader.ReadFloat()),
            Kind.ShipRevived => new ShipRevived(reader.ReadByte(maxShip, "event ship")),
            Kind.ShipRespawning => new ShipRespawning(reader.ReadByte(maxShip, "event ship")),
            Kind.AsteroidExploded => new AsteroidExploded(reader.ReadFloat(), reader.ReadFloat()),
            _ => new EffectEvent(
                reader.ReadEnum<EffectKind>("effect kind"),
                reader.ReadFloat(),
                reader.ReadFloat(),
                reader.ReadFloat(),
                reader.ReadFloat()),
        };
    }

    private static void WriteShipEvent(BinaryWriter writer, Kind kind, int shipIndex)
    {
        writer.Write((byte)kind);
        writer.Write((byte)shipIndex);
    }
}
