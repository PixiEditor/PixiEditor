using System.Collections.ObjectModel;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Tools;

namespace PixiEditor.ViewModels.SubViewModels;

internal class ToolSetViewModel : PixiObservableObject, IToolSetHandler
{
    public LocalizedString ToolSetName { get; }
    ICollection<IToolHandler> IToolSetHandler.Tools => Tools;
    public ObservableCollection<IToolHandler> Tools { get; } = new();
    
    public ToolSetViewModel(string setName, List<IToolHandler> tools)
    {
        ToolSetName = setName;
        foreach (var tool in tools)
        {
            Tools.Add(tool);
        }    
    }
}
