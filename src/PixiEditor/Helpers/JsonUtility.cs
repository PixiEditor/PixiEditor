using System.Text.Json;

namespace PixiEditor.Helpers;

public static class JsonUtility
{
    public static object TryDeserialize(JsonElement json, Type type)
    {
        try
        {
            return json.Deserialize(type);
        }
        catch (JsonException)
        {
            return json.ValueKind switch
            {
                JsonValueKind.String => json.GetString(),
                JsonValueKind.Number => json.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null
            };
        }
    }
}
