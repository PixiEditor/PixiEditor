using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Config;

public class ToolSetsConfig : List<ToolSet>
{
}

public class ToolsConfig
{
    [JsonConverter(typeof(ToolConverter))]
    public List<ToolConfig> CustomTools { get; set; }

    public ToolSetsConfig ToolSets { get; set; }
}

public class ToolSet
{
    public string Name { get; set; }

    public string? Icon { get; set; }

    [JsonConverter(typeof(ToolConverter))] public List<ToolConfig> Tools { get; set; }
}

public class ToolConfig
{
    public string ToolName { get; set; }
    public string? Brush { get; set; }
    public string? ToolTip { get; set; }
    public string? DefaultShortcut { get; set; }
    public Dictionary<string, object>? Settings { get; set; }
    public bool IsSimpleTool => Settings == null || Settings.Count == 0;
    public string? Icon { get; set; }
    public List<ActionDisplayConfig>? ActionDisplays { get; set; }
}

public class ActionDisplayConfig
{
    public string ActionDisplay { get; set; }
    public string? Modifiers { get; set; }
}

public class ToolConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => objectType == typeof(List<ToolConfig>);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);

        if (token.Type == JTokenType.Array)
        {
            var tools = new List<ToolConfig>();

            foreach (var item in token)
            {
                if (item.Type == JTokenType.String)
                {
                    tools.Add(new ToolConfig { ToolName = item.ToString() });
                }
                else if (item.Type == JTokenType.Object)
                {
                    tools.Add(item.ToObject<ToolConfig>(serializer));
                }
                else
                {
                    throw new JsonSerializationException("Unexpected token type in Tools array");
                }
            }

            return tools;
        }

        throw new JsonSerializationException("Expected array for Tools");
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var tools = (List<ToolConfig>)value;

        writer.WriteStartArray();

        foreach (var tool in tools)
        {
            if (tool.IsSimpleTool)
            {
                writer.WriteValue(tool.ToolName);
            }
            else
            {
                serializer.Serialize(writer, tool);
            }
        }

        writer.WriteEndArray();
    }
}
