using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface INodeGraphHandler
{
   public ObservableCollection<INodeHandler> AllNodes { get; }
   public ObservableCollection<NodeConnectionViewModel> Connections { get; }
   public INodeHandler OutputNode { get; }
   bool TryTraverse(Func<INodeHandler, bool> func);
}
