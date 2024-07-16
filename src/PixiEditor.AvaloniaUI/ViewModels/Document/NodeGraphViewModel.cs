using System.Collections.ObjectModel;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.Nodes;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class NodeGraphViewModel : ViewModelBase, INodeGraphHandler
{
    public DocumentViewModel DocumentViewModel { get; }
    public ObservableCollection<INodeHandler> AllNodes { get; } = new();
    public ObservableCollection<NodeConnectionViewModel> Connections { get; } = new();
    public ObservableCollection<NodeFrameViewModelBase> Frames { get; } = new();
    public StructureTree StructureTree { get; } = new();
    public INodeHandler? OutputNode { get; private set; }

    private DocumentInternalParts Internals { get; }

    public NodeGraphViewModel(DocumentViewModel documentViewModel, DocumentInternalParts internals)
    {
        DocumentViewModel = documentViewModel;
        Internals = internals;
    }

    public void AddNode(INodeHandler node)
    {
        if (OutputNode == null)
        {
            OutputNode = node; // TODO: this is not really correct yet, a way to check what node type is added is needed
        }

        AllNodes.Add(node);
        StructureTree.Update(this);
    }

    public void RemoveNode(Guid nodeId)
    {
        var node = AllNodes.FirstOrDefault(x => x.Id == nodeId);

        if (node != null)
        {
            AllNodes.Remove(node);
        }

        StructureTree.Update(this);
    }

    public void AddFrame(Guid frameId, IEnumerable<Guid> nodes)
    {
        var frame = new NodeFrameViewModel(frameId, AllNodes.Where(x => nodes.Contains(x.Id)));
        
        Frames.Add(frame);
    }

    public void AddZone(Guid frameId, string internalName, Guid startId, Guid endId)
    {
        var start = AllNodes.First(x => x.Id == startId);
        var end = AllNodes.First(x => x.Id == endId);
        
        var zone = new NodeZoneViewModel(frameId, internalName, start, end);
        
        Frames.Add(zone);
    }

    public void RemoveFrame(Guid guid)
    {
        var frame = Frames.FirstOrDefault(x => x.Id == guid);

        if (frame == null) return;

        Frames.Remove(frame);
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

        StructureTree.Update(this);
    }

    public void RemoveConnection(Guid nodeId, string property)
    {
        var connection = Connections.FirstOrDefault(x =>
            x.InputProperty.Node.Id == nodeId && x.InputProperty.PropertyName == property);
        if (connection != null)
        {
            connection.InputProperty.ConnectedOutput = null;
            connection.OutputProperty.ConnectedInputs.Remove(connection.InputProperty);
            Connections.Remove(connection);
        }

        StructureTree.Update(this);
    }

    public void RemoveConnections(Guid nodeId)
    {
        var connections = Connections
            .Where(x => x.InputProperty.Node.Id == nodeId || x.OutputProperty.Node.Id == nodeId).ToList();
        foreach (var connection in connections)
        {
            connection.InputProperty.ConnectedOutput = null;
            connection.OutputProperty.ConnectedInputs.Remove(connection.InputProperty);
            Connections.Remove(connection);
        }

        StructureTree.Update(this);
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

    public void UpdatePropertyValue(INodeHandler node, string property, object? value)
    {
        Internals.ActionAccumulator.AddFinishedActions(new UpdatePropertyValue_Action(node.Id, property, value));
    }
    
    public void EndChangeNodePosition()
    {
        Internals.ActionAccumulator.AddFinishedActions(new EndNodePosition_Action());
    }

    public void CreateNode(Type nodeType)
    {
        IAction change;
        
        if (nodeType == typeof(ModifyImageLeftNode) || nodeType == typeof(ModifyImageRightNode))
        {
            change = new CreateModifyImageNodePair_Action(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        }
        else
        {
            change = new CreateNode_Action(nodeType, Guid.NewGuid());
        }
        
        Internals.ActionAccumulator.AddFinishedActions(change);
    }

    // TODO: Remove this
    public void CreateNodeFrameAroundEverything()
    {
        CreateNodeFrame(AllNodes);
    }
    
    public void CreateNodeFrame(IEnumerable<INodeHandler> nodes)
    {
        Internals.ActionAccumulator.AddFinishedActions(new CreateNodeFrame_Action(Guid.NewGuid(), nodes.Select(x => x.Id)));
    }

    public void ConnectProperties(INodePropertyHandler? start, INodePropertyHandler? end)
    {
        if (start == null && end == null) return;

        INodeHandler inputNode = null, outputNode = null;
        string inputProperty = null, outputProperty = null;

        var input = start?.IsInput == true ? start : end;
        var output = start?.IsInput == false ? start : end;
        
        if (input == null && output != null)
        {
            input = output.ConnectedInputs.FirstOrDefault();
            output = null;
        }

        if (input != null)
        {
            inputNode = input.Node;
            inputProperty = input.PropertyName;
        }

        if (output != null)
        {
            outputNode = output.Node;
            outputProperty = output.PropertyName;
        }

        if (input == null) return;

        IAction action = input != null && output != null ?
            new ConnectProperties_Action(inputNode.Id, outputNode.Id, inputProperty, outputProperty) :
            new DisconnectProperty_Action(inputNode.Id, inputProperty);
        
        Internals.ActionAccumulator.AddFinishedActions(action);
    }
}
