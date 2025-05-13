using System.Collections.ObjectModel;
using System.Reflection;
using Avalonia.Input;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document;

internal class NodeGraphViewModel : ViewModelBase, INodeGraphHandler, IDisposable
{
    public DocumentViewModel DocumentViewModel { get; }
    public ObservableCollection<INodeHandler> AllNodes { get; } = new();
    public ObservableCollection<NodeConnectionViewModel> Connections { get; } = new();
    public ObservableCollection<NodeFrameViewModelBase> Frames { get; } = new();
    public ObservableCollection<string> AvailableRenderOutputs { get; } = new();
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
        if (OutputNode == null &&
            node.InternalName == typeof(OutputNode).GetCustomAttribute<NodeInfoAttribute>().UniqueName)
        {
            OutputNode = node;
        }

        AllNodes.Add(node);
        StructureTree.Update(this);
        UpdateAvailableRenderOutputs();
    }

    public void RemoveNode(Guid nodeId)
    {
        var node = AllNodes.FirstOrDefault(x => x.Id == nodeId);

        if (node != null)
        {
            AllNodes.Remove(node);
        }

        StructureTree.Update(this);
        UpdateAvailableRenderOutputs();
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
        
        var node = AllNodes.FirstOrDefault(x => x.Id == nodeId);
        if (node != null)
        {
            var input = node.Inputs.FirstOrDefault(x => x.PropertyName == property);
            if (input != null)
            {
                input.ConnectedOutput = null;
            }
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
        var finalQueue = new HashSet<INodeHandler>();
        var queueNodes = new Queue<INodeHandler>();
        queueNodes.Enqueue(outputNode);

        while (queueNodes.Count > 0)
        {
            var node = queueNodes.Dequeue();
            if (finalQueue.Contains(node))
            {
                continue;
            }

            bool canAdd = true;

            foreach (var input in node.Inputs)
            {
                if (input.ConnectedOutput == null)
                {
                    continue;
                }

                if (finalQueue.Contains(input.ConnectedOutput.Node))
                {
                    continue;
                }

                canAdd = false;

                if (finalQueue.Contains(input.ConnectedOutput.Node))
                {
                    finalQueue.Remove(input.ConnectedOutput.Node);
                    finalQueue.Add(input.ConnectedOutput.Node);
                }

                if (!queueNodes.Contains(input.ConnectedOutput.Node))
                {
                    queueNodes.Enqueue(input.ConnectedOutput.Node);
                }
            }

            if (canAdd)
            {
                finalQueue.Add(node);
            }
            else
            {
                queueNodes.Enqueue(node);
            }
        }

        return new Queue<INodeHandler>(finalQueue);
    }

    public void SetNodePositions(List<INodeHandler> node, VecD startPos)
    {
        Guid[] nodeIds = node.Select(x => x.Id).ToArray();
        Internals.ActionAccumulator.AddActions(new NodePosition_Action(nodeIds, startPos));
    }

    public void UpdatePropertyValue(INodeHandler node, string property, object? value)
    {
        Internals.ActionAccumulator.AddFinishedActions(new UpdatePropertyValue_Action(node.Id, property, value));
    }

    public void GetComputedPropertyValue(INodePropertyHandler property)
    {
        Internals.ActionAccumulator.AddFinishedActions(new GetComputedPropertyValue_Action(property.Node.Id, property.PropertyName, property.IsInput));
    }

    public void EndChangeNodePosition()
    {
        Internals.ActionAccumulator.AddFinishedActions(new EndNodePosition_Action());
    }

    public void CreateNode(Type nodeType, VecD pos = default)
    {
        IAction change;

        PairNodeAttribute? pairAttribute = nodeType.GetCustomAttribute<PairNodeAttribute>(true);

        List<IAction> changes = new();

        if (pairAttribute != null)
        {
            Guid startId = Guid.NewGuid();
            Guid endId = Guid.NewGuid();
            changes.Add(new CreateNodePair_Action(startId, endId, nodeType));

            if (pos != default)
            {
                changes.Add(new NodePosition_Action([startId], pos));
                changes.Add(new EndNodePosition_Action());
                changes.Add(new NodePosition_Action([endId], new VecD(pos.X + 400, pos.Y)));
                changes.Add(new EndNodePosition_Action());
            }
        }
        else
        {
            Guid nodeId = Guid.NewGuid();
            changes.Add(new CreateNode_Action(nodeType, nodeId, Guid.Empty));

            if (pos != default)
            {
                changes.Add(new NodePosition_Action([nodeId], pos));
                changes.Add(new EndNodePosition_Action());
            }
        }

        Internals.ActionAccumulator.AddFinishedActions(changes.ToArray());
    }

    public void RemoveNodes(Guid[] selectedNodes)
    {
        List<IAction> actions = new(); 

        for (int i = 0; i < selectedNodes.Length; i++)
        {
            actions.Add(new DeleteNode_Action(selectedNodes[i]));
        }

        Internals.ActionAccumulator.AddFinishedActions(actions.ToArray());
    }

    // TODO: Remove this
    public void CreateNodeFrameAroundEverything()
    {
        CreateNodeFrame(AllNodes);
    }

    public void CreateNodeFrame(IEnumerable<INodeHandler> nodes)
    {
        Internals.ActionAccumulator.AddFinishedActions(new CreateNodeFrame_Action(Guid.NewGuid(),
            nodes.Select(x => x.Id)));
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
            input = output.ConnectedInputs?.FirstOrDefault();
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

        IAction action = input != null && output != null
            ? new ConnectProperties_Action(inputNode.Id, outputNode.Id, inputProperty, outputProperty)
            : new DisconnectProperty_Action(inputNode.Id, inputProperty);

        Internals.ActionAccumulator.AddFinishedActions(action);
    }
    
    public void UpdateAvailableRenderOutputs()
    {
        List<string> outputs = new();
        foreach (var node in AllNodes)
        {
            if (node.InternalName == typeof(CustomOutputNode).GetCustomAttribute<NodeInfoAttribute>().UniqueName)
            {
                var nameInput =
                    node.Inputs.FirstOrDefault(x => x.PropertyName == CustomOutputNode.OutputNamePropertyName);

                if (nameInput is { Value: string name } && !string.IsNullOrEmpty(name))
                {
                    if(outputs.Contains(name)) continue;
                    
                    outputs.Add(name);
                }
            }
            else if (node.InternalName == typeof(OutputNode).GetCustomAttribute<NodeInfoAttribute>().UniqueName)
            {
                outputs.Insert(0, "DEFAULT");
            }
        }
        
        RemoveExcessiveRenderOutputs(outputs);
        AddMissingRenderOutputs(outputs);
    }

    private void RemoveExcessiveRenderOutputs(List<string> outputs)
    {
        for (int i = AvailableRenderOutputs.Count - 1; i >= 0; i--)
        {
            if (!outputs.Contains(AvailableRenderOutputs[i]))
            {
                AvailableRenderOutputs.RemoveAt(i);
            }
        }
    }
    
    private void AddMissingRenderOutputs(List<string> outputs)
    {
        foreach (var output in outputs)
        {
            if (!AvailableRenderOutputs.Contains(output))
            {
                AvailableRenderOutputs.Add(output);
            }
        }
    }

    public void Dispose()
    {
        foreach (var node in AllNodes)
        {
            node.Dispose();
        }
    }
}
