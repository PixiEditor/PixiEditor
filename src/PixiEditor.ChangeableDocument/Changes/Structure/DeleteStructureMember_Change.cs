using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class DeleteStructureMember_Change : Change
{
    private Guid memberGuid;
    private int originalIndex;
    private List<IInputProperty> originalOutputConnections = new();
    private List<(IInputProperty, IOutputProperty?)> originalInputConnections = new();
    private StructureNode? savedCopy;

    [GenerateMakeChangeAction]
    public DeleteStructureMember_Change(Guid memberGuid)
    {
        this.memberGuid = memberGuid;
    }

    public override bool InitializeAndValidate(Document document)
    {
        var member = document.FindMember(memberGuid);
        if (member is null)
            return false;

        originalOutputConnections = member.Output.Connections.ToList();
        originalInputConnections = member.InputProperties.Select(x => ((IInputProperty)x, x.Connection)).ToList();
        savedCopy = (StructureNode)member.Clone();
        savedCopy.Id = memberGuid;
        return true;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document document, bool firstApply,
        out bool ignoreInUndo)
    {
        StructureNode node = document.FindMember(memberGuid);

        var bgConnection = node.Background.Connection;
        var outputConnections = node.Output.Connections.ToArray();

        document.NodeGraph.RemoveNode(node);

        List<IChangeInfo> changes = new();

        if (outputConnections != null && bgConnection != null)
        {
            foreach (var connection in outputConnections)
            {
                bgConnection.ConnectTo(connection);
                changes.Add(new ConnectProperty_ChangeInfo(bgConnection.Node.Id, connection.Node.Id,
                    bgConnection.InternalPropertyName, connection.InternalPropertyName));
            }
        }

        node.Dispose();
        ignoreInUndo = false;

        changes.Add(new DeleteStructureMember_ChangeInfo(memberGuid));
        return changes;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document doc)
    {
        var copy = (StructureNode)savedCopy!.Clone();
        copy.Id = memberGuid;

        doc.NodeGraph.AddNode(copy);

        List<IChangeInfo> changes = new();

        IChangeInfo createChange = copy switch
        {
            LayerNode => CreateLayer_ChangeInfo.FromLayer(memberGuid, (LayerNode)copy),
            FolderNode => CreateFolder_ChangeInfo.FromFolder(memberGuid, (FolderNode)copy),
            _ => throw new NotSupportedException(),
        };
        
        changes.Add(createChange);

        changes.AddRange(NodeOperations.ConnectStructureNodeProperties(originalOutputConnections, originalInputConnections, copy)); 
        
        return changes;
    }

    public override void Dispose()
    {
        savedCopy?.Dispose();
    }
}
