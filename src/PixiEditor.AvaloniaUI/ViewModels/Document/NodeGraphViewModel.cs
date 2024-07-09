using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class NodeGraphViewModel : ViewModelBase, INodeGraphHandler
{
    public DocumentViewModel DocumentViewModel { get; }
    public ObservableCollection<INodeHandler> AllNodes { get; } = new();
    public ObservableCollection<NodeConnectionViewModel> Connections { get; } = new();
    public INodeHandler? OutputNode { get; private set; }
    
    private DocumentInternalParts Internals { get; }

    public NodeGraphViewModel(DocumentViewModel documentViewModel, DocumentInternalParts internals)
    {
        DocumentViewModel = documentViewModel;
        Internals = internals;
    }

    public void AddNode(INodeHandler node)
    {
        if(OutputNode == null)
        {
            OutputNode = node; // TODO: this is not really correct yet, a way to check what node type is added is needed
        }
        
        AllNodes.Add(node);
    }
    
    public void RemoveNode(Guid nodeId)
    {
        var node = AllNodes.FirstOrDefault(x => x.Id == nodeId);
        if (node != null)
        {
            AllNodes.Remove(node);
        }
    }

    public void SetConnection(NodeConnectionViewModel connection)
    {
        var existingInputConnection = Connections.FirstOrDefault(x => x.InputProperty == connection.InputProperty);
        if (existingInputConnection != null)
        {
            Connections.Remove(existingInputConnection);
            existingInputConnection.InputProperty.ConnectedOutput = null;
            existingInputConnection.OutputProperty.ConnectedInputs.Remove(existingInputConnection.InputProperty);
        }
        
        connection.InputProperty.ConnectedOutput = connection.OutputProperty;
        connection.OutputProperty.ConnectedInputs.Add(connection.InputProperty);
        
        Connections.Add(connection);
    }

    public void RemoveConnection(Guid nodeId, string property)
    {
        var connection = Connections.FirstOrDefault(x => x.InputProperty.Node.Id == nodeId && x.InputProperty.PropertyName == property);
        if (connection != null)
        {
            connection.InputProperty.ConnectedOutput = null;
            connection.OutputProperty.ConnectedInputs.Remove(connection.InputProperty);
            Connections.Remove(connection);
        }
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
                   if (input.ConnectedOutput == null)
                   {
                       continue;
                   }
   
                   queueNodes.Enqueue(input.ConnectedOutput.Node);
               }
           }
   
           finalQueue.Reverse();
           return new Queue<INodeHandler>(finalQueue);
       }

    public void SetNodePosition(INodeHandler node, VecD newPos)
    {
        Internals.ActionAccumulator.AddActions(new NodePosition_Action(node.Id, newPos));
    }
    
    public void EndChangeNodePosition()
    {
        Internals.ActionAccumulator.AddFinishedActions(new EndNodePosition_Action());
    }

    public void CreateNode(Type nodeType)
    {
        Internals.ActionAccumulator.AddFinishedActions(new CreateNode_Action(nodeType, Guid.NewGuid()));
    }

    public void ConnectProperties(INodePropertyHandler start, INodePropertyHandler end)
    {
        INodeHandler inputNode = start.IsInput ? start.Node : end.Node;
        INodeHandler outputNode = start.IsInput ? end.Node : start.Node;
        string inputProperty = start.IsInput ? start.PropertyName : end.PropertyName;
        string outputProperty = start.IsInput ? end.PropertyName : start.PropertyName;

        Internals.ActionAccumulator.AddFinishedActions(new ConnectProperties_Action(inputNode.Id, outputNode.Id, inputProperty, outputProperty));
    }
}
