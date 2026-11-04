using System.Text.Json;
using System.Text.Json.Serialization;

namespace Saoss.Game.Settings;

/// <summary>
/// Reads and writes an enum by name like <see cref="JsonStringEnumConverter"/>, but an unknown name or a value of
/// the wrong type reads as an undefined value instead of failing the whole file, so validation can replace just
/// that value with its default.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class LenientEnumConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    /// <summary>The value an unreadable entry gets; no enum here defines it.</summary>
    public static TEnum Unknown { get; } = (TEnum)Enum.ToObject(typeof(TEnum), -1);

    /// <inheritdoc/>
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out var number))
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), number);
        }

        if (reader.TokenType == JsonTokenType.String && Enum.TryParse<TEnum>(reader.GetString(), ignoreCase: true, out var value)
            && Enum.IsDefined(value))
        {
            return value;
        }

        reader.Skip();
        return Unknown;
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
}
