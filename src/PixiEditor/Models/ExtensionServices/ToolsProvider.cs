using PixiEditor.Extensions;
using PixiEditor.Extensions.CommonApi.Tools;
using PixiEditor.Extensions.WasmRuntime;
using PixiEditor.Extensions.WasmRuntime.Api.Tools;
using PixiEditor.Extensions.WasmRuntime.Utilities;
using PixiEditor.ViewModels.Document;
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
}
