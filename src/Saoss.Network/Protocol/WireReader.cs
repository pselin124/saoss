using System.Buffers.Binary;
using System.Text;

namespace Saoss.Network.Protocol;

/// <summary>
/// Reads a message payload and rejects anything out of range or truncated with a <see cref="ProtocolException"/>.
/// </summary>
internal sealed class WireReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    private readonly byte[] _data;
    private int _position;

    /// <summary>Reads from a payload.</summary>
    public WireReader(ReadOnlySpan<byte> payload) => _data = payload.ToArray();

    /// <summary>Bytes not read yet.</summary>
    public int Remaining => _data.Length - _position;

    /// <summary>Reads a byte that must not exceed <paramref name="max"/>.</summary>
    public int ReadByte(int max, string what) => Check(Take(1)[0], 0, max, what);

    /// <summary>Reads a bool stored as 0 or 1.</summary>
    public bool ReadBool(string what) => ReadByte(1, what) == 1;

    /// <summary>Reads a little-endian ushort that must not exceed <paramref name="max"/>.</summary>
    public int ReadUShort(int max, string what) => Check(BinaryPrimitives.ReadUInt16LittleEndian(Take(sizeof(ushort))), 0, max, what);

    /// <summary>Reads a little-endian int in [<paramref name="min"/>, <paramref name="max"/>].</summary>
    public int ReadInt(int min, int max, string what) => Check(BinaryPrimitives.ReadInt32LittleEndian(Take(sizeof(int))), min, max, what);

    /// <summary>Reads an enum stored as a byte and checks that the value is defined.</summary>
    public T ReadEnum<T>(string what)
        where T : struct, Enum
    {
        var raw = Take(1)[0];
        var value = (T)Enum.ToObject(typeof(T), raw);
        return Enum.IsDefined(value) ? value : throw Invalid(what, raw);
    }

    /// <summary>Reads a UTF-8 string with a ushort byte length, at most <paramref name="maxChars"/> characters.</summary>
    public string ReadString(int maxChars, string what)
    {
        var length = ReadUShort(maxChars * 3, what + " length");
        try
        {
            var text = StrictUtf8.GetString(Take(length));
            return text.Length <= maxChars ? text : throw Invalid(what + " length", text.Length);
        }
        catch (DecoderFallbackException exception)
        {
            throw new ProtocolException($"Invalid UTF-8 in {what}.", exception);
        }
    }

    /// <summary>Reads everything that is left.</summary>
    public byte[] ReadRest() => Take(Remaining).ToArray();

    /// <summary>Fails if bytes are left over.</summary>
    public void EnsureEnd()
    {
        if (Remaining != 0)
        {
            throw new ProtocolException($"{Remaining} unexpected bytes at the end of the message.");
        }
    }

    /// <summary>Creates the exception for a value out of range.</summary>
    public static ProtocolException Invalid(string what, long value) => new($"Invalid {what}: {value}.");

    private static int Check(int value, int min, int max, string what) =>
        value >= min && value <= max ? value : throw Invalid(what, value);

    private ReadOnlySpan<byte> Take(int count)
    {
        if (count > Remaining)
        {
            throw new ProtocolException("The message is truncated.");
        }

        var span = _data.AsSpan(_position, count);
        _position += count;
        return span;
    }
}
