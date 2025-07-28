using System.Collections.Immutable;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record NodeInputsChanged_ChangeInfo(Guid NodeId, ImmutableArray<NodePropertyInfo> Inputs) : IChangeInfo
{
    public static NodeInputsChanged_ChangeInfo FromNode(Node node)
    {
        var infos = CreateNode_ChangeInfo.CreatePropertyInfos(node.InputProperties, true, node.Id);
        return new NodeInputsChanged_ChangeInfo(node.Id, infos);
    }
}

public record NodeOutputsChanged_ChangeInfo(Guid NodeId, ImmutableArray<NodePropertyInfo> Outputs) : IChangeInfo
{
    public static NodeOutputsChanged_ChangeInfo FromNode(Node node)
    {
        var infos = CreateNode_ChangeInfo.CreatePropertyInfos(node.OutputProperties, false, node.Id);
        return new NodeOutputsChanged_ChangeInfo(node.Id, infos);
    }
}
