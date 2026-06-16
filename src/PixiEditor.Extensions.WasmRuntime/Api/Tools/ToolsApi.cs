using PixiEditor.Extensions.CommonApi.Tools;
using PixiEditor.Extensions.CommonApi.Utilities;
using ProtoBuf;

namespace PixiEditor.Extensions.WasmRuntime.Api.Tools;

internal class ToolsApi : ApiGroupHandler
{
    [ApiFunction("register_brush_tool")]
    public void RegisterBrushTool(Span<byte> pixiFileBytes, Span<byte> toolConfigBytes)
    {
        CustomToolConfig config = Serializer.Deserialize<CustomToolConfig>(toolConfigBytes);

        PixiEditorExtensionToolConfig pec = new PixiEditorExtensionToolConfig(config, Extension);

        Api.Tools.RegisterBrushTool(pixiFileBytes.ToArray(), pec);
    }

    [ApiFunction("add_tool_to_toolset")]
    public void AddToolToToolset(string toolName, string toolsetName, int atIndex)
    {
        string prefixedName = PrefixedNameUtility.ToPixiEditorRelativePreferenceName(Extension.Metadata.UniqueName, toolName);
        string prefixedToolsetName = PrefixedNameUtility.ToPixiEditorRelativePreferenceName(Extension.Metadata.UniqueName, toolsetName);
        Api.Tools.AddToolToToolset(prefixedName, prefixedToolsetName, atIndex);
    }

    [ApiFunction("add_tool_to_toolset_with_config")]
    public void AddToolToToolsetWithConfig(string toolName, string toolsetName, int atIndex, string configJson)
    {
        string prefixedName = PrefixedNameUtility.ToPixiEditorRelativePreferenceName(Extension.Metadata.UniqueName, toolName);
        string prefixedToolsetName = PrefixedNameUtility.ToPixiEditorRelativePreferenceName(Extension.Metadata.UniqueName, toolsetName);
        Api.Tools.AddToolToToolset(prefixedName, prefixedToolsetName, atIndex, configJson);
    }
}
