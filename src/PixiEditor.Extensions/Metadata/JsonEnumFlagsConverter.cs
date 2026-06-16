using System.Text.Json;
using System.Text.Json.Serialization;

namespace PixiEditor.Extensions.Metadata;

public class JsonEnumFlagConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return Enum.TryParse<T>(reader.GetString(), out var result) ? result : default;
            }
            throw new JsonException("Expected array of strings for Enum flags.");
        }

        long combinedValue = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return (T)Enum.ToObject(typeof(T), combinedValue);
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string? flagName = reader.GetString();
                if (Enum.TryParse<T>(flagName, out var flagValue))
                {
                    combinedValue |= Convert.ToInt64(flagValue);
                }
            }
        }

        throw new JsonException("Unexpected end of JSON while reading Enum flags.");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        string[] flags = value.ToString().Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var flag in flags)
        {
            writer.WriteStringValue(flag);
        }

        writer.WriteEndArray();
    }
}
