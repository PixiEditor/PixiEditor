using System.Reflection;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.NodeGraph;

internal class CreateNodePair_Change : Change
{
    private Guid startId;
    private Guid endId;
    private Type nodeType;

    [GenerateMakeChangeAction]
    public CreateNodePair_Change(Guid startId, Guid endId, Type nodeType)
    {
        this.startId = startId;
        this.endId = endId;
        this.nodeType = nodeType;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return nodeType.GetCustomAttribute<PairNodeAttribute>() != null &&
               nodeType is { IsAbstract: false, IsInterface: false };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply,
        out bool ignoreInUndo)
    {
        if (startId == Guid.Empty)
            startId = Guid.NewGuid();
        if (endId == Guid.Empty)
            endId = Guid.NewGuid();

        PairNodeAttribute attribute = nodeType.GetCustomAttribute<PairNodeAttribute>();
        Type startingType = attribute.IsStartingType ? nodeType : attribute.OtherType;
        Type endingType = attribute.IsStartingType ? attribute.OtherType : nodeType;

        var start = NodeOperations.CreateNode(startingType, target);
        var end = NodeOperations.CreateNode(endingType, target);

        start.Id = startId;
        end.Id = endId;

        if (start is IPairNode pairStart)
            pairStart.OtherNode = end.Id;

        if (end is IPairNode pairEnd)
            pairEnd.OtherNode = start.Id;

        end.Position = new VecD(100, 0);

        target.NodeGraph.AddNode(start);
        target.NodeGraph.AddNode(end);

        ignoreInUndo = false;

        return new List<IChangeInfo>
        {
            CreateNode_ChangeInfo.CreateFromNode(start),
            CreateNode_ChangeInfo.CreateFromNode(end),
        };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        var startChange = RemoveNode(target, startId);
        var endChange = RemoveNode(target, endId);

        return new List<IChangeInfo> { startChange, endChange };
    }

    private static DeleteNode_ChangeInfo RemoveNode(Document target, Guid id)
    {
        Node node = target.FindNodeOrThrow<Node>(id);
        target.NodeGraph.RemoveNode(node);

        return new DeleteNode_ChangeInfo(id);
    }
}
