using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using Drawie.Backend.Core.Shaders.Generation;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class ConnectProperties_Change : Change
{
    public Guid InputNodeId { get; }
    public Guid OutputNodeId { get; }
    public string InputProperty { get; }
    public string OutputProperty { get; }

    private PropertyConnection originalConnection;
    private List<PropertyConnection> originalConnectionsAtOutput;

    [GenerateMakeChangeAction]
    public ConnectProperties_Change(Guid inputNodeId, Guid outputNodeId, string inputProperty, string outputProperty)
    {
        InputNodeId = inputNodeId;
        OutputNodeId = outputNodeId;
        InputProperty = inputProperty;
        OutputProperty = outputProperty;
    }

    public override bool InitializeAndValidate(Document target)
    {
        Node inputNode = target.FindNode(InputNodeId);
        Node outputNode = target.FindNode(OutputNodeId);

        if (inputNode == null || outputNode == null)
        {
            return false;
        }

        InputProperty? inputProp = inputNode.GetInputProperty(InputProperty);
        OutputProperty? outputProp = outputNode.GetOutputProperty(OutputProperty);

        if (inputNode == outputNode && outputProp == null)
        {
            var input = inputNode.GetInputProperty(OutputProperty);
            if (input is { Connection: not null })
            {
                outputProp = input.Connection as OutputProperty;
            }
        }

        if (inputProp == null || outputProp == null)
        {
            return false;
        }

        if (GraphUtils.IsLoop(inputProp, outputProp))
        {
            return false;
        }

        bool canConnect = inputProp.CanConnect(outputProp);

        if (!canConnect)
        {
            return false;
        }

        originalConnection =
            new PropertyConnection(inputProp.Connection?.Node.Id, inputProp.Connection?.InternalPropertyName);
        originalConnectionsAtOutput = new List<PropertyConnection>();

        foreach (var connection in outputProp.Connections)
        {
            originalConnectionsAtOutput.Add(new PropertyConnection(connection.Node.Id,
                connection.InternalPropertyName));
        }

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        Node inputNode = target.FindNode(InputNodeId);
        Node outputNode = target.FindNode(OutputNodeId);

        InputProperty inputProp = inputNode.GetInputProperty(InputProperty);
        OutputProperty outputProp = outputNode.GetOutputProperty(OutputProperty);

        List<IChangeInfo> changes = new();

        target.NodeGraph.StartListenToPropertyChanges();

        if (inputNode == outputNode && outputProp == null)
        {
            var input = inputNode.GetInputProperty(OutputProperty);
            if (input is { Connection: not null })
            {
                outputProp = input.Connection as OutputProperty;

                if (outputProp != null)
                {
                    changes.Add(new ConnectProperty_ChangeInfo(null, inputNode.Id, null, OutputProperty));
                }

                outputProp.DisconnectFrom(input);
            }
        }

        inputProp = inputNode.GetInputProperty(InputProperty);
        if (inputProp.Connection != null)
        {
            changes.Add(new ConnectProperty_ChangeInfo(null, inputProp.Connection.Node.Id, null,
                inputProp.Connection.InternalPropertyName));
            inputProp.Connection.DisconnectFrom(inputProp);
            inputProp = inputNode.GetInputProperty(InputProperty);
        }

        outputProp.ConnectTo(inputProp);

        ignoreInUndo = false;

        int newInputsHash = GraphUtils.CalculateInputsHash(inputNode);
        int newOutputsHash = GraphUtils.CalculateOutputsHash(inputNode);

        List<Guid> nodesWithChangedIO = target.NodeGraph.StopListenToPropertyChanges();

        foreach (var nodeId in nodesWithChangedIO)
        {
            Node node = target.FindNode(nodeId);

            changes.Add(NodeInputsChanged_ChangeInfo.FromNode(node));
            changes.Add(NodeOutputsChanged_ChangeInfo.FromNode(node));
        }

        changes.Add(new ConnectProperty_ChangeInfo(outputProp.Node.Id, InputNodeId, outputProp.InternalPropertyName,
            InputProperty));

        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Node inputNode = target.FindNode(InputNodeId);
        Node outputNode = target.FindNode(OutputNodeId);

        InputProperty inputProp = inputNode.GetInputProperty(InputProperty);
        OutputProperty outputProp = outputNode.GetOutputProperty(OutputProperty);

        int inputsHash = GraphUtils.CalculateInputsHash(inputNode);
        int outputsHash = GraphUtils.CalculateOutputsHash(inputNode);

        List<IChangeInfo> changes = new();

        if (inputNode == outputNode && outputProp == null)
        {
            var input = inputNode.GetInputProperty(OutputProperty);

            if (inputProp.Connection != null)
            {
                inputProp.Connection.ConnectTo(input);
                changes.Add(new ConnectProperty_ChangeInfo(inputProp.Connection.Node.Id, input.Node.Id,
                    inputProp.Connection.InternalPropertyName, input.InternalPropertyName));

                outputProp = input.Connection as OutputProperty;
            }
        }

        outputProp.DisconnectFrom(inputProp);

        changes.Add(new ConnectProperty_ChangeInfo(null, InputNodeId, null, InputProperty));

        if (originalConnection.NodeId != null)
        {
            Node originalNode = target.FindNode(originalConnection.NodeId.Value);
            IOutputProperty? originalOutput = originalNode.GetOutputProperty(originalConnection.PropertyName);
            originalOutput.ConnectTo(inputProp);

            changes.Add(new ConnectProperty_ChangeInfo(originalOutput.Node.Id, inputProp.Node.Id,
                originalOutput.InternalPropertyName, InputProperty));
        }

        foreach (var connection in originalConnectionsAtOutput)
        {
            if (connection.NodeId.HasValue)
            {
                Node originalNode = target.FindNode(connection.NodeId.Value);
                IInputProperty? originalInput = originalNode.GetInputProperty(connection.PropertyName);
                outputProp.ConnectTo(originalInput);

                changes.Add(new ConnectProperty_ChangeInfo(outputProp.Node.Id, originalInput.Node.Id,
                    outputProp.InternalPropertyName, originalInput.InternalPropertyName));
            }
        }

        int newInputsHash = GraphUtils.CalculateInputsHash(inputNode);
        int newOutputsHash = GraphUtils.CalculateOutputsHash(inputNode);

        if (inputsHash != newInputsHash)
        {
            changes.Add(NodeInputsChanged_ChangeInfo.FromNode(inputNode));
        }

        if (outputsHash != newOutputsHash)
        {
            changes.Add(NodeOutputsChanged_ChangeInfo.FromNode(inputNode));
        }

        return changes;
    }
}
