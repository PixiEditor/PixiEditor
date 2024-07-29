using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class CreateStructureMember_Change : Change
{
    private Guid newMemberGuid;

    private Guid parentGuid;
    private StructureMemberType type;

    [GenerateMakeChangeAction]
    public CreateStructureMember_Change(Guid parent, Guid newGuid,
        StructureMemberType type)
    {
        this.parentGuid = parent;
        this.type = type;
        newMemberGuid = newGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.TryFindNode<Node>(parentGuid, out _);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document document, bool firstApply,
        out bool ignoreInUndo)
    {
        StructureNode member = type switch
        {
            // TODO: Add support for other types
            StructureMemberType.Layer => new ImageLayerNode(document.Size) { Id = newMemberGuid },
            StructureMemberType.Folder => new FolderNode() { Id = newMemberGuid },
            _ => throw new NotSupportedException(),
        };

        document.TryFindNode<Node>(parentGuid, out var parentNode);

        List<IChangeInfo> changes = new() { CreateChangeInfo(member) };
        
        InputProperty<Surface> targetInput = parentNode.InputProperties.FirstOrDefault(x => 
            x.ValueType == typeof(Surface) && 
            x.Connection.Node is StructureNode) as InputProperty<Surface>;
        
        
        
        if (member is FolderNode folder)
        {
            document.NodeGraph.AddNode(member);
            AppendFolder(targetInput, folder, changes);
        }
        else
        {
            document.NodeGraph.AddNode(member);
            List<ConnectProperty_ChangeInfo> connectPropertyChangeInfo =
                NodeOperations.AppendMember(targetInput, member.Output, member.Background, member.Id);
            changes.AddRange(connectPropertyChangeInfo);
        }


        ignoreInUndo = false;

        return changes;
    }

    private IChangeInfo CreateChangeInfo(StructureNode member)
    {
        return type switch
        {
            StructureMemberType.Layer => CreateLayer_ChangeInfo.FromLayer((LayerNode)member),
            StructureMemberType.Folder => CreateFolder_ChangeInfo.FromFolder((FolderNode)member),
            _ => throw new NotSupportedException(),
        };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document document)
    {
        var container = document.FindNodeOrThrow<Node>(parentGuid);
        if (container is not IBackgroundInput backgroundInput)
        {
            throw new InvalidOperationException("Parent folder is not a valid container.");
        }

        StructureNode child = document.FindMemberOrThrow(newMemberGuid);
        var childBackgroundConnection = child.Background.Connection;
        child.Dispose();

        document.NodeGraph.RemoveNode(child);

        List<IChangeInfo> changes = new() { new DeleteStructureMember_ChangeInfo(newMemberGuid), };

        if (childBackgroundConnection != null)
        {
            childBackgroundConnection?.ConnectTo(backgroundInput.Background);
            ConnectProperty_ChangeInfo change = new(childBackgroundConnection.Node.Id,
                backgroundInput.Background.Node.Id, childBackgroundConnection.InternalPropertyName,
                backgroundInput.Background.InternalPropertyName);
            changes.Add(change);
        }

        return changes;
    }

    private static void AppendFolder(InputProperty<Surface> backgroundInput, FolderNode folder, List<IChangeInfo> changes)
    {
        var appened = NodeOperations.AppendMember(backgroundInput, folder.Output, folder.Background, folder.Id);
        changes.AddRange(appened);
    }

    
}
