using System.Collections.ObjectModel;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Tools;

namespace PixiEditor.ViewModels.SubViewModels;

internal class ToolSetViewModel : PixiObservableObject, IToolSetHandler
{
    public string Name { get; }
    public string Icon { get; }
    ICollection<IToolHandler> IToolSetHandler.Tools => Tools;
    IReadOnlyDictionary<IToolHandler, string> IToolSetHandler.IconOverwrites => IconOverwrites;

    public ObservableCollection<IToolHandler> Tools { get; } = new();
    public Dictionary<IToolHandler, string> IconOverwrites { get; set; } = new Dictionary<IToolHandler, string>();

    public ToolSetViewModel(string setName, string? icon = null)
    {
        Icon = icon ?? string.Empty;
        Name = setName;
    }

    public void AddTool(IToolHandler tool)
    {
        Tools.Add(tool);
    }

    public void ApplyToolSetSettings()
    {
        foreach (IToolHandler tool in Tools)
        {
            tool.ApplyToolSetSettings(this);
        }
    }

}
