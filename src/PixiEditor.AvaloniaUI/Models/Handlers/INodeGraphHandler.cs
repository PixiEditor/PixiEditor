using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface INodeGraphHandler
{
   public ObservableCollection<INodeHandler> AllNodes { get; }
   public ObservableCollection<NodeConnectionViewModel> Connections { get; }
   public INodeHandler OutputNode { get; }
   public bool TryTraverse(Func<INodeHandler, bool> func);
   public void AddNode(INodeHandler node);
   public void RemoveNode(Guid nodeId);
   public void SetConnection(NodeConnectionViewModel connection);
   public void RemoveConnection(Guid nodeId, string property);
   public void RemoveConnections(Guid nodeId);
}
