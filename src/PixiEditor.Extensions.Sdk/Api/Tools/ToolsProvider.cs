using PixiEditor.Extensions.CommonApi.Tools;
using PixiEditor.Extensions.Sdk.Bridge;

namespace PixiEditor.Extensions.Sdk.Api.Tools;

public class ToolsProvider : IToolsProvider
{
    public void RegisterBrushTool(byte[] pixiFileBytes, ExtensionToolConfig config)
    {
        Interop.RegisterBrushTool(pixiFileBytes, config.Config);
    }

    public void AddToolToToolset(string toolName, string toolsetName, int atIndex)
    {
        Native.add_tool_to_toolset(toolName, toolsetName, atIndex);
    }
}
