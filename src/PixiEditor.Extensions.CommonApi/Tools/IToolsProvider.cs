namespace PixiEditor.Extensions.CommonApi.Tools;

public interface IToolsProvider
{
    public void RegisterBrushTool(byte[] pixiFileBytes, ExtensionToolConfig config);
    public void AddToolToToolset(string toolName, string toolsetName, int atIndex);
}
