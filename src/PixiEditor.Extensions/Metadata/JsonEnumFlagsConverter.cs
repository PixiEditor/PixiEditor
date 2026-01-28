using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PixiEditor.Extensions.Metadata;

public class JsonEnumFlagConverter : JsonConverter
{
    public override object ReadJson(JsonReader reader,  Type objectType, Object existingValue, JsonSerializer serializer)
    {
        var jArrayLoad = JArray.Load(reader);
        if (jArrayLoad == null || jArrayLoad.Count == 0)
        {
            return Enum.Parse(typeof(ExtensionPermissions), Enum.GetNames(objectType)[0]);
        }
        
        var flags = jArrayLoad
            .Select(f => f.ToString())
            .Aggregate((f1, f2) => $"{f1}, {f2}");

        return Enum.Parse(objectType, flags);
    }

    public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
    {
        var flags = value.ToString()
            .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
            .Select(f => $"\"{f}\"");

        writer.WriteRawValue($"[{string.Join(", ", flags)}]");
    }

    public override bool CanConvert(Type objectType)
    {
        return true;
    }
}
