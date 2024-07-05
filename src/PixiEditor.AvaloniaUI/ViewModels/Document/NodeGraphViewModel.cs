using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class NodeGraphViewModel : ViewModelBase, INodeGraphHandler
{
    public DocumentViewModel DocumentViewModel { get; }
    public ObservableCollection<INodeHandler> AllNodes { get; } = new();
    public ObservableCollection<NodeConnectionViewModel> Connections { get; } = new();
    public INodeHandler? OutputNode { get; }

    public NodeGraphViewModel(DocumentViewModel documentViewModel)
    {
        DocumentViewModel = documentViewModel;
    }

    public bool TryTraverse(Func<INodeHandler, bool> func)
    {
        if (OutputNode == null) return false;

        var queue = CalculateExecutionQueue(OutputNode);

        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            func(node);
        }

        return true;
    }
    
    private Queue<INodeHandler> CalculateExecutionQueue(INodeHandler outputNode)
       {
           // backwards breadth-first search
           var visited = new HashSet<INodeHandler>();
           var queueNodes = new Queue<INodeHandler>();
           List<INodeHandler> finalQueue = new();
           queueNodes.Enqueue(outputNode);
   
           while (queueNodes.Count > 0)
           {
               var node = queueNodes.Dequeue();
               if (!visited.Add(node))
               {
                   continue;
               }
   
               finalQueue.Add(node);
   
               foreach (var input in node.Inputs)
               {
                   if (input.Connection == null)
                   {
                       continue;
                   }
   
                   queueNodes.Enqueue(input.Connection.Node);
               }
           }
   
           finalQueue.Reverse();
           return new Queue<INodeHandler>(finalQueue);
       } 
}
