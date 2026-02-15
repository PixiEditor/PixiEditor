using System.Text.Json;
using System.Text.Json.Serialization;

namespace PixiEditor.Helpers.Converters.JsonConverters;

internal class DefaultUnknownEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    private readonly T defaultEnumValue = (T)Enum.ToObject(typeof(T), -1);

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var enumText = reader.GetString();
            if (Enum.TryParse(enumText, out T result))
            {
                return result;
            }
        }
        else if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out var value) && Enum.IsDefined(typeof(T), value))
            {
                return (T)Enum.ToObject(typeof(T), value);
            }
        }

        return defaultEnumValue;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        // Standard string enum writing
        writer.WriteStringValue(value.ToString());
    }
}
