namespace Saoss.Core.Simulation.Snapshots;

/// <summary>
/// Writes pooled objects compactly: only slots in use, each with its index. Reading frees every slot that is not
/// listed by setting its lifetime to zero; the other fields of free slots are never read by the simulation.
/// Beams are the exception: a free beam keeps its velocity for the next <see cref="Beam.Set"/>, so free beams
/// with a velocity are written too.
/// </summary>
internal static class PoolSnapshot
{
    private const byte FreeBeamWithVelocity = 0;
    private const byte ActiveBeam = 1;

    /// <summary>Writes the projectiles in use.</summary>
    public static void WriteProjectiles(BinaryWriter writer, Projectile[] pool)
    {
        writer.Write((ushort)pool.Count(p => p.IsActive));
        for (var i = 0; i < pool.Length; i++)
        {
            var p = pool[i];
            if (!p.IsActive)
            {
                continue;
            }

            writer.Write((ushort)i);
            WriteFloats(writer, p.X, p.Y, p.VelocityX, p.VelocityY, p.Angle, p.Energy, p.Red, p.Green, p.Blue, p.Start, p.Size);
            writer.Write(p.Lifetime);
            writer.Write(p.Delay);
        }
    }

    /// <summary>Reads projectiles written by <see cref="WriteProjectiles"/> into the pool.</summary>
    public static void ReadProjectiles(SnapshotReader reader, Projectile[] pool, string what)
    {
        var listed = ReadListed(reader, pool.Length, what, (index, r) =>
        {
            var p = pool[index];
            p.X = r.ReadFloat();
            p.Y = r.ReadFloat();
            p.VelocityX = r.ReadFloat();
            p.VelocityY = r.ReadFloat();
            p.Angle = r.ReadFloat();
            p.Energy = r.ReadFloat();
            p.Red = r.ReadFloat();
            p.Green = r.ReadFloat();
            p.Blue = r.ReadFloat();
            p.Start = r.ReadFloat();
            p.Size = r.ReadFloat();
            p.Lifetime = r.ReadInt(1, int.MaxValue, what + " lifetime");
            p.Delay = r.ReadInt();
        });
        for (var i = 0; i < pool.Length; i++)
        {
            if (!listed[i])
            {
                pool[i].Lifetime = 0;
            }
        }
    }

    /// <summary>Writes the visible beams and the free ones that still have a velocity.</summary>
    public static void WriteBeams(BinaryWriter writer, Beam[] beams)
    {
        writer.Write((ushort)beams.Count(IsWorthWriting));
        for (var i = 0; i < beams.Length; i++)
        {
            var b = beams[i];
            if (!IsWorthWriting(b))
            {
                continue;
            }

            writer.Write((ushort)i);
            writer.Write(b.IsActive ? ActiveBeam : FreeBeamWithVelocity);
            WriteFloats(writer, b.VelocityX, b.VelocityY);
            if (b.IsActive)
            {
                WriteFloats(writer, b.X1, b.Y1, b.X2, b.Y2, b.Red, b.Green, b.Blue);
                writer.Write(b.Lifetime);
            }
        }
    }

    /// <summary>Reads beams written by <see cref="WriteBeams"/>.</summary>
    public static void ReadBeams(SnapshotReader reader, Beam[] beams)
    {
        var listed = ReadListed(reader, beams.Length, "beam", (index, r) =>
        {
            var b = beams[index];
            var active = r.ReadByte(ActiveBeam, "beam kind") == ActiveBeam;
            b.VelocityX = r.ReadFloat();
            b.VelocityY = r.ReadFloat();
            b.Lifetime = 0;
            if (active)
            {
                b.X1 = r.ReadFloat();
                b.Y1 = r.ReadFloat();
                b.X2 = r.ReadFloat();
                b.Y2 = r.ReadFloat();
                b.Red = r.ReadFloat();
                b.Green = r.ReadFloat();
                b.Blue = r.ReadFloat();
                b.Lifetime = r.ReadInt(1, int.MaxValue, "beam lifetime");
            }
        });
        for (var i = 0; i < beams.Length; i++)
        {
            if (!listed[i])
            {
                beams[i].Lifetime = 0;
                beams[i].VelocityX = 0;
                beams[i].VelocityY = 0;
            }
        }
    }

    /// <summary>Writes the pickups in the arena.</summary>
    public static void WritePickups(BinaryWriter writer, IReadOnlyList<Pickup> pickups)
    {
        writer.Write((ushort)pickups.Count(p => p.IsActive));
        for (var i = 0; i < pickups.Count; i++)
        {
            var p = pickups[i];
            if (p.IsActive)
            {
                writer.Write((ushort)i);
                WriteFloats(writer, p.X, p.Y);
                writer.Write(p.Lifetime);
                writer.Write(p.Delay);
            }
        }
    }

    /// <summary>Reads pickups written by <see cref="WritePickups"/>.</summary>
    public static void ReadPickups(SnapshotReader reader, IReadOnlyList<Pickup> pickups)
    {
        var listed = ReadListed(reader, pickups.Count, "pickup", (index, r) =>
        {
            var p = pickups[index];
            p.X = r.ReadFloat();
            p.Y = r.ReadFloat();
            p.Lifetime = r.ReadInt(1, int.MaxValue, "pickup lifetime");
            p.Delay = r.ReadInt();
        });
        for (var i = 0; i < pickups.Count; i++)
        {
            if (!listed[i])
            {
                pickups[i].Lifetime = 0;
            }
        }
    }

    /// <summary>Writes floats in order.</summary>
    public static void WriteFloats(BinaryWriter writer, params ReadOnlySpan<float> values)
    {
        foreach (var value in values)
        {
            writer.Write(value);
        }
    }

    private static bool IsWorthWriting(Beam beam) => beam.IsActive || beam.VelocityX != 0 || beam.VelocityY != 0;

    // Reads a count and that many entries with strictly increasing indices; returns which slots were listed.
    private static bool[] ReadListed(SnapshotReader reader, int poolSize, string what, Action<int, SnapshotReader> readEntry)
    {
        var listed = new bool[poolSize];
        var count = reader.ReadUShort(poolSize, what + " count");
        var previous = -1;
        for (var n = 0; n < count; n++)
        {
            var index = reader.ReadUShort(poolSize - 1, what + " index");
            if (index <= previous)
            {
                throw SnapshotReader.Invalid(what + " index order", index);
            }

            previous = index;
            listed[index] = true;
            readEntry(index, reader);
        }

        return listed;
    }
}
