using System.Text.Json;
using PixiEditor.Extensions.CommonApi.Tools;
using PixiEditor.Extensions.WasmRuntime.Api.Tools;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.Models.ExtensionServices;

internal class ToolsProvider : IToolsProvider
{
    public ToolsViewModel ToolsViewModel { get; }

    public ToolsProvider(ToolsViewModel toolsViewModel)
    {
        ToolsViewModel = toolsViewModel;
    }

    public void RegisterBrushTool(byte[] pixiFileBytes, ExtensionToolConfig config)
    {
        ToolsViewModel.AddCustomTool(pixiFileBytes, config,
            config is PixiEditorExtensionToolConfig extConfig ? extConfig.Extension.Metadata.DisplayName : "EXTENSION");
    }

    public void AddToolToToolset(string toolName, string toolsetName, int atIndex)
    {
        var tool = ToolsViewModel.AllTools.FirstOrDefault(x => x.ToolName == toolName);
        if (tool == null)
        {
            return;
        }

        var foundToolset = ToolsViewModel.AllToolSets.FirstOrDefault(x => x.Name == toolsetName);
        if(foundToolset == null)
        {
            foundToolset = new ToolSetViewModel(toolsetName);
            ToolsViewModel.AllToolSets.Add(foundToolset);
        }

        foundToolset.Tools.Insert(atIndex, tool);
    }

    public void AddToolToToolset(string toolName, string toolsetName, int atIndex, string configJson)
    {
        var tool = ToolsViewModel.AllTools.FirstOrDefault(x => x.ToolName == toolName);
        if (tool == null)
        {
            return;
        }

        var foundToolset = ToolsViewModel.AllToolSets.FirstOrDefault(x => x.Name == toolsetName);
        if (foundToolset == null)
        {
            foundToolset = new ToolSetViewModel(toolsetName);
            ToolsViewModel.AllToolSets.Add(foundToolset);
        }

        try
        {
            Dictionary<string, object> config = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson);
            tool.SetToolSetSettings(foundToolset, config);
        }
        catch (JsonException)
        {
            Console.WriteLine($"Failed to parse config JSON for tool {toolName} in toolset {toolsetName}. Adding tool without config.");
        }

        foundToolset.Tools.Insert(atIndex, tool);
    }
}
