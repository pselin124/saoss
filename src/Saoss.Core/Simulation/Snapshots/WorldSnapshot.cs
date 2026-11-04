using Saoss.Core.Simulation.Modes;

namespace Saoss.Core.Simulation.Snapshots;

/// <summary>
/// Binary snapshot of the complete simulation state of a <see cref="World"/>, so a network client can mirror the
/// host's match: tick, random generator state, ships with their pools, asteroids, pickups, mode state, ship
/// choices, outcome and the current events. <see cref="World.Settings"/> is not included; the target world must
/// have been created with the same settings and seed.
/// </summary>
public static class WorldSnapshot
{
    /// <summary>Format version written first; bumped whenever the layout changes.</summary>
    public const byte FormatVersion = 1;

    /// <summary>Most events a snapshot may carry.</summary>
    public const int MaxEvents = 8192;

    private const int NoShip = -1;

    /// <summary>Writes the world's state.</summary>
    public static void Write(World world, BinaryWriter writer)
    {
        writer.Write(FormatVersion);
        writer.Write((byte)world.Settings.Mode);
        writer.Write(world.Tick);
        WriteRandom(writer, world.Random);
        writer.Write((byte)world.Ships.Count);
        foreach (var ship in world.Ships)
        {
            ShipSnapshot.Write(writer, ship);
        }

        WriteAsteroids(writer, world.Asteroids.Asteroids);
        PoolSnapshot.WritePickups(writer, world.Pickups);
        ModeSnapshot.Write(writer, world.Rules);
        WriteChoices(writer, world);
        WriteOutcome(writer, world.Outcome);
        WriteEvents(writer, world);
    }

    /// <summary>Returns the world's state as bytes.</summary>
    public static byte[] ToBytes(World world)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream))
        {
            Write(world, writer);
        }

        return stream.ToArray();
    }

    /// <summary>
    /// Overwrites <paramref name="target"/> with a snapshot written by <see cref="Write"/>. Throws
    /// <see cref="SnapshotFormatException"/> on bad data, after which the target may be partly updated and should
    /// be discarded.
    /// </summary>
    public static void Read(World target, BinaryReader reader)
    {
        try
        {
            ReadValidated(target, new SnapshotReader(reader));
        }
        catch (EndOfStreamException exception)
        {
            throw new SnapshotFormatException("The snapshot is truncated.", exception);
        }
    }

    /// <summary>Applies a whole snapshot from bytes; trailing data is an error.</summary>
    public static void Apply(World target, ReadOnlyMemory<byte> snapshot)
    {
        using var stream = new MemoryStream(snapshot.ToArray(), writable: false);
        using var reader = new BinaryReader(stream);
        Read(target, reader);
        if (stream.Position != stream.Length)
        {
            throw new SnapshotFormatException("The snapshot has trailing data.");
        }
    }

    private static void ReadValidated(World target, SnapshotReader reader)
    {
        RequireEqual(reader.ReadByte(byte.MaxValue, "format version"), FormatVersion, "format version");
        RequireEqual(reader.ReadByte(byte.MaxValue, "game mode"), (int)target.Settings.Mode, "game mode");
        var tick = reader.ReadInt(0, int.MaxValue, "tick");
        ReadRandom(reader, target.Random);
        var ships = target.Ships;
        RequireEqual(reader.ReadByte(byte.MaxValue, "ship count"), ships.Count, "ship count");
        foreach (var ship in ships)
        {
            ShipSnapshot.Read(reader, ship);
        }

        ReadAsteroids(reader, target.Asteroids.Asteroids);
        PoolSnapshot.ReadPickups(reader, target.Pickups);
        ModeSnapshot.Read(reader, target.Rules, ships.Count);
        var (queue, pending) = ReadChoices(reader, ships.Count);
        var outcome = ReadOutcome(reader, ships.Count);
        var (events, stepEventCount) = ReadEvents(reader, ships.Count);
        target.RestoreState(new WorldState(tick, events, stepEventCount, queue, pending, outcome));
    }

    private static void RequireEqual(int actual, int expected, string what)
    {
        if (actual != expected)
        {
            throw new SnapshotFormatException($"Snapshot {what} {actual} does not match the world's {expected}.");
        }
    }

    private static void WriteRandom(BinaryWriter writer, GameRandom random)
    {
        foreach (var value in random.State[1..])
        {
            writer.Write(value);
        }

        writer.Write((byte)random.NextIndex);
        writer.Write((byte)random.NextPIndex);
    }

    private static void ReadRandom(SnapshotReader reader, GameRandom random)
    {
        Span<int> state = stackalloc int[GameRandom.StateLength];
        for (var i = 1; i < state.Length; i++)
        {
            state[i] = reader.ReadInt();
        }

        var next = reader.ReadByte(GameRandom.StateLength - 1, "random index");
        var nextP = reader.ReadByte(GameRandom.StateLength - 1, "random index");
        random.Restore(state, next, nextP);
    }

    private static void WriteAsteroids(BinaryWriter writer, IReadOnlyList<Asteroid> asteroids)
    {
        writer.Write((byte)asteroids.Count);
        foreach (var a in asteroids)
        {
            PoolSnapshot.WriteFloats(writer, a.X, a.Y, a.VelocityX, a.VelocityY, a.Size, a.SpinPhase, a.SpinAxisX, a.SpinAxisY, a.SpinAxisZ, a.Energy);
        }
    }

    private static void ReadAsteroids(SnapshotReader reader, IReadOnlyList<Asteroid> asteroids)
    {
        RequireEqual(reader.ReadByte(byte.MaxValue, "asteroid count"), asteroids.Count, "asteroid count");
        foreach (var a in asteroids)
        {
            a.X = reader.ReadFloat();
            a.Y = reader.ReadFloat();
            a.VelocityX = reader.ReadFloat();
            a.VelocityY = reader.ReadFloat();
            a.Size = reader.ReadFloat();
            a.SpinPhase = reader.ReadFloat();
            a.SpinAxisX = reader.ReadFloat();
            a.SpinAxisY = reader.ReadFloat();
            a.SpinAxisZ = reader.ReadFloat();
            a.Energy = reader.ReadFloat();
        }
    }

    private static void WriteChoices(BinaryWriter writer, World world)
    {
        writer.Write((byte)world.ChoiceQueue.Count);
        foreach (var index in world.ChoiceQueue)
        {
            writer.Write((byte)index);
        }

        writer.Write((sbyte)(world.PendingChoice?.ShipIndex ?? NoShip));
    }

    private static (int[] Queue, int? Pending) ReadChoices(SnapshotReader reader, int shipCount)
    {
        var queue = new int[reader.ReadByte(shipCount, "choice queue length")];
        for (var i = 0; i < queue.Length; i++)
        {
            queue[i] = reader.ReadByte(shipCount - 1, "choice queue entry");
        }

        var pending = (sbyte)reader.ReadByte(byte.MaxValue, "pending choice");
        if (pending != NoShip && (pending < 0 || queue.Length == 0 || queue[0] != pending))
        {
            throw SnapshotReader.Invalid("pending choice", pending);
        }

        return (queue, pending == NoShip ? null : pending);
    }

    private static void WriteOutcome(BinaryWriter writer, MatchOutcome? outcome)
    {
        writer.Write(outcome != null);
        if (outcome == null)
        {
            return;
        }

        writer.Write((sbyte)(outcome.WinningTeam ?? NoShip));
        PoolSnapshot.WriteFloats(writer, [.. outcome.FinalScores]);
        writer.Write(outcome.WinnerCredits);
    }

    private static MatchOutcome? ReadOutcome(SnapshotReader reader, int shipCount)
    {
        if (!reader.ReadBool("outcome flag"))
        {
            return null;
        }

        var team = (sbyte)reader.ReadByte(byte.MaxValue, "winning team");
        if (team < NoShip || team >= GameConstants.MaxPlayers)
        {
            throw SnapshotReader.Invalid("winning team", team);
        }

        var scores = new float[shipCount];
        for (var i = 0; i < scores.Length; i++)
        {
            scores[i] = reader.ReadFloat();
        }

        return new MatchOutcome(team == NoShip ? null : team, scores, reader.ReadInt());
    }

    private static void WriteEvents(BinaryWriter writer, World world)
    {
        writer.Write((ushort)world.StepEventCount);
        writer.Write((ushort)world.Events.Count);
        foreach (var gameEvent in world.Events)
        {
            EventSnapshot.Write(writer, gameEvent);
        }
    }

    private static (GameEvent[] Events, int StepEventCount) ReadEvents(SnapshotReader reader, int shipCount)
    {
        var stepEventCount = reader.ReadUShort(MaxEvents, "step event count");
        var events = new GameEvent[reader.ReadUShort(MaxEvents, "event count")];
        if (stepEventCount > events.Length)
        {
            throw SnapshotReader.Invalid("step event count", stepEventCount);
        }

        for (var i = 0; i < events.Length; i++)
        {
            events[i] = EventSnapshot.Read(reader, shipCount);
        }

        return (events, stepEventCount);
    }
}
