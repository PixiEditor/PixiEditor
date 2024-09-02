using System.Collections.ObjectModel;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Tools;

namespace PixiEditor.ViewModels.SubViewModels;

internal class ToolSetViewModel : PixiObservableObject, IToolSetHandler
{
    public string Name { get; }
    ICollection<IToolHandler> IToolSetHandler.Tools => Tools;
    public ObservableCollection<IToolHandler> Tools { get; } = new();
    
    public ToolSetViewModel(string setName, List<IToolHandler> tools)
    {
        Name = setName;
        foreach (var tool in tools)
        {
            Tools.Add(tool);
        }    
    }
}
