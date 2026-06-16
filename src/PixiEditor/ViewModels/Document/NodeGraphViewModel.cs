using System.Collections.Immutable;
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
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph.Blackboard;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.ViewModels.Document.Blackboard;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document;

internal class NodeGraphViewModel : ViewModelBase, INodeGraphHandler, IDisposable
{
    private bool isFullyCreated;

    public DocumentViewModel DocumentViewModel { get; }
    public ObservableCollection<INodeHandler> AllNodes { get; } = new();
    public ObservableCollection<NodeConnectionViewModel> Connections { get; } = new();
    public ObservableCollection<NodeFrameViewModelBase> Frames { get; } = new();
    public ObservableCollection<string> AvailableRenderOutputs { get; } = new();
    public StructureTree StructureTree { get; } = new();
    public INodeHandler? OutputNode { get; private set; }
    public Dictionary<string, INodeHandler> CustomRenderOutputs { get; } = new();

    public Dictionary<Guid, INodeHandler> NodeLookup { get; } = new();

    IReadOnlyDictionary<Guid, INodeHandler> INodeGraphHandler.NodeLookup => NodeLookup;

    public BlackboardViewModel Blackboard { get; }

    IBlackboardHandler INodeGraphHandler.Blackboard => Blackboard;

    private DocumentInternalParts Internals { get; }

    public NodeGraphViewModel(DocumentViewModel documentViewModel, DocumentInternalParts internals)
    {
        DocumentViewModel = documentViewModel;
        Internals = internals;
        Blackboard = new BlackboardViewModel(internals);
    }

    internal void InitFrom(IReadOnlyNodeGraph nodeGraph)
    {
        foreach (var node in nodeGraph.AllNodes)
        {
            Internals.Updater.ApplyChangeFromChangeInfo(CreateNode_ChangeInfo.CreateFromNode(node));
            Internals.Updater.ApplyChangeFromChangeInfo(new NodePosition_ChangeInfo(node.Id, node.Position));
        }

        foreach (var node in nodeGraph.AllNodes)
        {
            foreach (var inputProperty in node.InputProperties)
            {
                Internals.Updater.ApplyChangeFromChangeInfo(
                    new ConnectProperty_ChangeInfo(
                        inputProperty.Connection?.Node.Id,
                        inputProperty.Node.Id,
                        inputProperty.Connection?.InternalPropertyName,
                        inputProperty.InternalPropertyName));
            }
        }

        foreach (var var in nodeGraph.Blackboard.Variables)
        {
            Internals.Updater.ApplyChangeFromChangeInfo(
                new BlackboardVariable_ChangeInfo(var.Value.Name, var.Value.Type, var.Value.Value,
                    var.Value.Min ?? double.MinValue,
                    var.Value.Max ?? double.MaxValue,
                    var.Value.Unit));
        }
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
        NodeLookup[node.Id] = node;
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
        NodeLookup.Remove(nodeId);
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

        if(connection.InputProperty == null || connection.OutputProperty == null)
            return;

        connection.InputProperty.ConnectedOutput = connection.OutputProperty;
        connection.OutputProperty.ConnectedInputs.Add(connection.InputProperty);

        Connections.Add(connection);

        UpdatesFramesPartOf(connection.InputNode);
        UpdatesFramesPartOf(connection.OutputNode);

        StructureTree.Update(this);
    }

    public void RemoveConnection(Guid nodeId, string property)
    {
        RemoveInput(nodeId, property);

        var outputConnection = Connections.FirstOrDefault(x =>
            x.OutputProperty.Node.Id == nodeId && x.OutputProperty.PropertyName == property);
        if (outputConnection != null)
        {
            outputConnection.OutputProperty.ConnectedInputs.Remove(outputConnection.InputProperty);
            Connections.Remove(outputConnection);
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

    private void RemoveInput(Guid nodeId, string property)
    {
        var inputConnection = Connections.FirstOrDefault(x =>
            x.InputProperty.Node.Id == nodeId && x.InputProperty.PropertyName == property);
        if (inputConnection != null)
        {
            inputConnection.InputProperty.ConnectedOutput = null;
            inputConnection.OutputProperty.ConnectedInputs.Remove(inputConnection.InputProperty);
            Connections.Remove(inputConnection);

            UpdatesFramesPartOf(inputConnection.InputNode);
            UpdatesFramesPartOf(inputConnection.OutputNode);
        }
    }

    public void UpdatesFramesPartOf(INodeHandler node)
    {
        if (!isFullyCreated)
            return;

        var lastKnownFramesPartOf = node.Frames.OfType<NodeZoneViewModel>().ToHashSet();
        var startLookup = Frames.OfType<NodeZoneViewModel>().ToDictionary(x => x.Start);
        var currentlyPartOf = new HashSet<NodeZoneViewModel>();

        node.TraverseBackwards(x =>
        {
            if (x is IPairNodeEndViewModel)
                return Traverse.NoFurther;

            if (x is not IPairNodeStartViewModel)
                return Traverse.Further;

            if (startLookup != null && startLookup.TryGetValue(x, out var zone))
            {
                currentlyPartOf.Add(zone);
            }

            return Traverse.Further;
        });

        foreach (var frame in currentlyPartOf)
        {
            frame.Nodes.Add(node);
            node.Frames.Add(frame);
        }

        lastKnownFramesPartOf.ExceptWith(currentlyPartOf);
        foreach (var removedFrom in lastKnownFramesPartOf)
        {
            removedFrom.Nodes.Remove(node);
            node.Frames.Remove(removedFrom);
        }
    }

    public void FinalizeCreation()
    {
        if (isFullyCreated)
            return;

        isFullyCreated = true;

        foreach (var nodeZoneViewModel in Frames.OfType<NodeZoneViewModel>())
        {
            UpdateNodesPartOf(nodeZoneViewModel);
        }
    }

    private static void UpdateNodesPartOf(NodeZoneViewModel zone)
    {
        var currentlyPartOf = new HashSet<INodeHandler>([zone.Start, zone.End]);

        foreach (var node in zone.Start
                     .Outputs
                     .SelectMany(x => x.ConnectedInputs)
                     .Select(x => x.Node))
        {
            node.TraverseForwards((x) =>
            {
                if (x is IPairNodeEndViewModel)
                    return Traverse.NoFurther;

                currentlyPartOf.Add(x);

                return Traverse.Further;
            });
        }

        zone.Nodes.ReplaceBy(currentlyPartOf);
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
        Internals.ActionAccumulator.AddFinishedActions(new UpdatePropertyValue_Action(node.Id, property, value),
            new EndUpdatePropertyValue_Action());
    }

    public void BeginUpdatePropertyValue(INodeHandler node, string property, object value)
    {
        Internals.ActionAccumulator.AddActions(new UpdatePropertyValue_Action(node.Id, property, value));
    }

    public void EndUpdatePropertyValue()
    {
        Internals.ActionAccumulator.AddFinishedActions(new EndUpdatePropertyValue_Action());
    }

    public void RequestUpdateComputedPropertyValue(INodePropertyHandler property)
    {
        Internals.ActionAccumulator.AddActions(
            new GetComputedPropertyValue_Action(property.Node.Id, property.PropertyName, property.IsInput));
    }

    public T GetComputedPropertyValue<T>(INodePropertyHandler property)
    {
        var node = Internals.Tracker.Document.NodeGraph.AllNodes.FirstOrDefault(x => x.Id == property.Node.Id);
        if (property.IsInput)
        {
            var prop = node.GetInputProperty(property.PropertyName);
            if (prop == null) return default;
            return prop.Value is T value ? value : default;
        }

        var output = node.GetOutputProperty(property.PropertyName);
        if (output == null) return default;
        return output.Value is T outputValue ? outputValue : default;
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
        Dictionary<string, INodeHandler> outputs = new();
        foreach (var node in AllNodes)
        {
            if (node.InternalName == typeof(CustomOutputNode).GetCustomAttribute<NodeInfoAttribute>().UniqueName)
            {
                var nameInput =
                    node.Inputs.FirstOrDefault(x => x.PropertyName == CustomOutputNode.OutputNamePropertyName);

                if (nameInput is { Value: string name } && !string.IsNullOrEmpty(name))
                {
                    outputs[name] = node;
                }
            }
            else if (node.InternalName == typeof(OutputNode).GetCustomAttribute<NodeInfoAttribute>().UniqueName)
            {
                outputs["DEFAULT"] = node;
            }
        }

        RemoveExcessiveRenderOutputs(outputs);
        AddMissingRenderOutputs(outputs);
    }

    private void RemoveExcessiveRenderOutputs(Dictionary<string, INodeHandler> outputs)
    {
        for (int i = AvailableRenderOutputs.Count - 1; i >= 0; i--)
        {
            var outputName = AvailableRenderOutputs[i];
            if (!outputs.ContainsKey(outputName))
            {
                AvailableRenderOutputs.RemoveAt(i);
            }

            CustomRenderOutputs.Remove(outputName);
        }
    }

    private void AddMissingRenderOutputs(Dictionary<string, INodeHandler> outputs)
    {
        foreach (var output in outputs)
        {
            if (!AvailableRenderOutputs.Contains(output.Key))
            {
                AvailableRenderOutputs.Add(output.Key);
            }

            CustomRenderOutputs[output.Key] = output.Value;
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
