using System.Collections.ObjectModel;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IOutputPropertyHandler : INodePropertyHandler
{
    bool INodePropertyHandler.IsInput => false;
    public ObservableCollection<IInputPropertyHandler> Connections { get; }
}
