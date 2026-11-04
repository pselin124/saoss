namespace Saoss.Core.Simulation.Snapshots;

/// <summary>
/// Reads snapshot values and rejects anything out of range with a <see cref="SnapshotFormatException"/>.
/// </summary>
internal sealed class SnapshotReader(BinaryReader reader)
{
    /// <summary>Reads a float; any value is accepted.</summary>
    public float ReadFloat() => reader.ReadSingle();

    /// <summary>Reads an int; any value is accepted.</summary>
    public int ReadInt() => reader.ReadInt32();

    /// <summary>Reads an int in [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public int ReadInt(int min, int max, string what) => Check(reader.ReadInt32(), min, max, what);

    /// <summary>Reads a long in [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public long ReadLong(long min, long max, string what)
    {
        var value = reader.ReadInt64();
        return value >= min && value <= max ? value : throw Invalid(what, value);
    }

    /// <summary>Reads a byte that must not exceed <paramref name="max"/>.</summary>
    public int ReadByte(int max, string what) => Check(reader.ReadByte(), 0, max, what);

    /// <summary>Reads a ushort that must not exceed <paramref name="max"/>.</summary>
    public int ReadUShort(int max, string what) => Check(reader.ReadUInt16(), 0, max, what);

    /// <summary>Reads a bool stored as 0 or 1.</summary>
    public bool ReadBool(string what) => ReadByte(1, what) == 1;

    /// <summary>Reads an enum stored as a byte and checks that the value is defined.</summary>
    public T ReadEnum<T>(string what)
        where T : struct, Enum
    {
        var raw = reader.ReadByte();
        var value = (T)Enum.ToObject(typeof(T), raw);
        return Enum.IsDefined(value) ? value : throw Invalid(what, raw);
    }

    /// <summary>Creates the exception for a value out of range.</summary>
    public static SnapshotFormatException Invalid(string what, long value) => new($"Invalid {what}: {value}.");

    private static int Check(int value, int min, int max, string what) =>
        value >= min && value <= max ? value : throw Invalid(what, value);
}
