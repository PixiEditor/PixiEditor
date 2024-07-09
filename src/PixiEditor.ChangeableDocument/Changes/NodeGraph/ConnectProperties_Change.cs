using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class ConnectProperties_Change : Change
{
    public Guid InputNodeId { get; }
    public Guid OutputNodeId { get; }
    public string InputProperty { get; }
    public string OutputProperty { get; }

    private IOutputProperty? originalConnection;

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

        if (inputProp == null || outputProp == null)
        {
            return false;
        }

        originalConnection = inputProp.Connection;

        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        Node inputNode = target.FindNode(InputNodeId);
        Node outputNode = target.FindNode(OutputNodeId);

        InputProperty inputProp = inputNode.GetInputProperty(InputProperty);
        OutputProperty outputProp = outputNode.GetOutputProperty(OutputProperty);

        outputProp.ConnectTo(inputProp);

        ignoreInUndo = false;

        return new ConnectProperty_ChangeInfo(outputNode.Id, inputNode.Id, OutputProperty, InputProperty);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Node inputNode = target.FindNode(InputNodeId);
        Node outputNode = target.FindNode(OutputNodeId);

        InputProperty inputProp = inputNode.GetInputProperty(InputProperty);
        OutputProperty outputProp = outputNode.GetOutputProperty(OutputProperty);

        outputProp.DisconnectFrom(inputProp);
        inputProp.Connection = originalConnection;


        return new ConnectProperty_ChangeInfo(outputNode.Id, inputNode.Id, OutputProperty, InputProperty);
    }
}
