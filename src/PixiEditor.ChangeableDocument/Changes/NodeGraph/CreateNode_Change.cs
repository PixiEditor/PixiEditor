using System.Collections.Immutable;
using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Numerics;
using Type = System.Type;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class CreateNode_Change : Change
{
    private Type nodeType;
    private Guid id;
    private Guid? pairId;

    [GenerateMakeChangeAction]
    public CreateNode_Change(Type nodeType, Guid id, Guid pairId)
    {
        this.id = id;
        this.nodeType = nodeType;
        this.pairId = pairId == Guid.Empty ? null : pairId;
    }

    public override bool InitializeAndValidate(Document target)
    {
        bool canCreate = nodeType.IsSubclassOf(typeof(Node)) && nodeType is { IsAbstract: false, IsInterface: false };
        return canCreate && (!nodeType.IsAssignableTo(typeof(OutputNode)) || target.NodeGraph.OutputNode is null);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        if (id == Guid.Empty)
            id = Guid.NewGuid();

        Node node = NodeOperations.CreateNode(nodeType, target);

        if (pairId.HasValue && node is IPairNode pairNode)
        {
            pairNode.OtherNode = pairId.Value;
        }

        node.Position = new VecD(0, 0);
        node.Id = id;

        target.NodeGraph.AddNode(node);
        ignoreInUndo = false;

        return CreateNode_ChangeInfo.CreateFromNode(node);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        Node node = target.FindNodeOrThrow<Node>(id);
        target.NodeGraph.RemoveNode(node);

        return new DeleteNode_ChangeInfo(id);
    }
}
