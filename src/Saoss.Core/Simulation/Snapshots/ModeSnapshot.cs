using Saoss.Core.Simulation.Modes;

namespace Saoss.Core.Simulation.Snapshots;

/// <summary>
/// Writes and reads the state of the game mode: control point owners, flowers or the mushroom.
/// Deathmatch has no state.
/// </summary>
internal static class ModeSnapshot
{
    private const int None = -1;

    /// <summary>Writes the mode's state.</summary>
    public static void Write(BinaryWriter writer, GameModeRules rules)
    {
        switch (rules)
        {
            case DominationMode domination:
                foreach (var point in domination.Points)
                {
                    writer.Write((sbyte)point.OwnerTeam);
                }

                break;
            case CaptureTheFlowerMode flowers:
                foreach (var flower in flowers.Flowers)
                {
                    writer.Write(flower.IsEnabled);
                    writer.Write((byte)flower.State);
                    writer.Write((sbyte)flower.CarrierIndex);
                    PoolSnapshot.WriteFloats(writer, flower.X, flower.Y);
                }

                break;
            case MushroomMode mushroom:
                writer.Write(mushroom.X);
                writer.Write(mushroom.Y);
                break;
        }
    }

    /// <summary>Reads the mode's state written by <see cref="Write"/>.</summary>
    public static void Read(SnapshotReader reader, GameModeRules rules, int shipCount)
    {
        switch (rules)
        {
            case DominationMode domination:
                foreach (var point in domination.Points)
                {
                    point.OwnerTeam = ReadSignedByte(reader, GameConstants.MaxPlayers - 1, "control point owner");
                }

                break;
            case CaptureTheFlowerMode flowers:
                foreach (var flower in flowers.Flowers)
                {
                    ReadFlower(reader, flower, shipCount);
                }

                break;
            case MushroomMode mushroom:
                mushroom.Restore(reader.ReadInt(), reader.ReadInt());
                break;
        }
    }

    private static void ReadFlower(SnapshotReader reader, Flower flower, int shipCount)
    {
        var enabled = reader.ReadBool("flower enabled");
        var state = reader.ReadEnum<FlowerState>("flower state");
        var carrier = ReadSignedByte(reader, shipCount - 1, "flower carrier");
        if ((state == FlowerState.Carried) != (carrier != None))
        {
            throw SnapshotReader.Invalid("flower carrier", carrier);
        }

        flower.Restore(enabled, state, carrier, reader.ReadFloat(), reader.ReadFloat());
    }

    // A value in [-1, max] stored as a signed byte.
    private static int ReadSignedByte(SnapshotReader reader, int max, string what)
    {
        var value = (sbyte)reader.ReadByte(byte.MaxValue, what);
        return value >= None && value <= max ? value : throw SnapshotReader.Invalid(what, value);
    }
}
