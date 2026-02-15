using System.Text.Json;
using System.Text.Json.Serialization;

namespace PixiEditor.Models.Config;

public class ToolSetsConfig : List<ToolSet>
{
}

public class ToolsConfig : IMergeable<ToolsConfig>
{
    [JsonConverter(typeof(ToolConverter))] public List<ToolConfig> CustomTools { get; set; }

    public ToolSetsConfig ToolSets { get; set; }

    public ToolsConfig TryMergeWith(ToolsConfig other)
    {
        if (other == null)
            return this;

        var merged = new ToolsConfig
        {
            CustomTools = new List<ToolConfig>(CustomTools), ToolSets = new ToolSetsConfig()
        };

        if (other.CustomTools != null)
        {
            foreach (var tool in other.CustomTools)
            {
                if (!merged.CustomTools.Exists(t => t.ToolName == tool.ToolName))
                {
                    merged.CustomTools.Add(tool);
                }
                else
                {
                    var existingTool = merged.CustomTools.Find(t => t.ToolName == tool.ToolName);
                    if (existingTool != null)
                    {
                        existingTool.Icon = string.IsNullOrEmpty(tool.Icon) ? existingTool.Icon : tool.Icon;
                        existingTool.ActionDisplays = tool.ActionDisplays ?? existingTool.ActionDisplays;
                        existingTool.DefaultShortcut = string.IsNullOrEmpty(tool.DefaultShortcut)
                            ? existingTool.DefaultShortcut
                            : tool.DefaultShortcut;
                        existingTool.SupportsSecondaryActionOnRightClick = tool.SupportsSecondaryActionOnRightClick;
                        existingTool.ToolTip = string.IsNullOrEmpty(tool.ToolTip) ? existingTool.ToolTip : tool.ToolTip;
                        if (existingTool.Settings != null && tool.Settings != null)
                        {
                            foreach (var kvp in tool.Settings)
                            {
                                existingTool.Settings[kvp.Key] = kvp.Value;
                            }
                        }
                        else if (existingTool.Settings == null)
                        {
                            existingTool.Settings = tool.Settings;
                        }
                    }
                }
            }
        }

        foreach (var set in ToolSets)
        {
            var otherSet = other.ToolSets.Find(s => s.Name == set.Name);
            if (otherSet != null)
            {
                var mergedSet = new ToolSet
                {
                    Name = set.Name,
                    Icon = string.IsNullOrEmpty(otherSet.Icon) ? set.Icon : otherSet.Icon,
                    Tools = new List<ToolConfig>(set.Tools)
                };

                if (otherSet.Tools != null)
                {
                    foreach (var tool in otherSet.Tools)
                    {
                        if (!mergedSet.Tools.Exists(t => t.ToolName == tool.ToolName))
                        {
                            mergedSet.Tools.Add(tool);
                        }
                        else
                        {
                            var existingTool = mergedSet.Tools.Find(t => t.ToolName == tool.ToolName);
                            if (existingTool != null)
                            {
                                existingTool.Icon = string.IsNullOrEmpty(tool.Icon) ? existingTool.Icon : tool.Icon;
                                existingTool.ActionDisplays = tool.ActionDisplays ?? existingTool.ActionDisplays;
                                existingTool.DefaultShortcut = string.IsNullOrEmpty(tool.DefaultShortcut)
                                    ? existingTool.DefaultShortcut
                                    : tool.DefaultShortcut;
                                existingTool.ToolTip =
                                    string.IsNullOrEmpty(tool.ToolTip) ? existingTool.ToolTip : tool.ToolTip;
                                existingTool.SupportsSecondaryActionOnRightClick = tool.SupportsSecondaryActionOnRightClick;

                                if (existingTool.Settings != null && tool.Settings != null)
                                {
                                    foreach (var kvp in tool.Settings)
                                    {
                                        existingTool.Settings[kvp.Key] = kvp.Value;
                                    }
                                }
                                else if (existingTool.Settings == null)
                                {
                                    existingTool.Settings = tool.Settings;
                                }
                            }
                        }
                    }
                }

                merged.ToolSets.Add(mergedSet);
            }
            else
            {
                merged.ToolSets.Add(set);
            }
        }

        if (other.ToolSets != null)
        {
            foreach (var set in other.ToolSets)
            {
                if (!merged.ToolSets.Exists(s => s.Name == set.Name))
                {
                    merged.ToolSets.Add(set);
                }
            }
        }

        return merged;
    }
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
    public bool SupportsSecondaryActionOnRightClick { get; set; }
}

public class ActionDisplayConfig
{
    public string ActionDisplay { get; set; }
    public string? Modifiers { get; set; }
}

public class ToolConverter : JsonConverter<List<ToolConfig>>
{
    public override List<ToolConfig> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected start of array for Tools.");

        var tools = new List<ToolConfig>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.EndArray:
                    return tools;
                case JsonTokenType.String:
                    tools.Add(new ToolConfig { ToolName = reader.GetString() });
                    break;
                case JsonTokenType.StartObject:
                {
                    var tool = JsonSerializer.Deserialize<ToolConfig>(ref reader, options);
                    if (tool != null)
                        tools.Add(tool);

                    break;
                }
                default:
                    throw new JsonException($"Unexpected token type {reader.TokenType} in Tools array.");
            }
        }

        throw new JsonException("Unexpected end of JSON while reading Tools array.");
    }

    public override void Write(Utf8JsonWriter writer, List<ToolConfig> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var tool in value)
        {
            if (tool.IsSimpleTool)
            {
                writer.WriteStringValue(tool.ToolName);
            }
            else
            {
                JsonSerializer.Serialize(writer, tool, options);
            }
        }

        writer.WriteEndArray();
    }
}
