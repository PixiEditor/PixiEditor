using System.Collections.ObjectModel;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.SubViewModels;

internal class ToolSetViewModel : PixiObservableObject
{
    public LocalizedString SetName { get; } 
    public ObservableCollection<IToolHandler> Tools { get; } = new();
}
