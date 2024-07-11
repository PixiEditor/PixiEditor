using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.ChangeInfos.Structure;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class CreateStructureMember_Change : Change
{
    private Guid newMemberGuid;

    private Guid parentFolderGuid;
    private StructureMemberType type;

    [GenerateMakeChangeAction]
    public CreateStructureMember_Change(Guid parentFolder, Guid newGuid,
        StructureMemberType type)
    {
        this.parentFolderGuid = parentFolder;
        this.type = type;
        newMemberGuid = newGuid;
    }

    public override bool InitializeAndValidate(Document target)
    {
        return target.TryFindNode<Node>(parentFolderGuid, out var targetNode) && targetNode is IBackgroundInput;
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

        document.TryFindNode<Node>(parentFolderGuid, out var parentNode);

        List<IChangeInfo> changes = new() { CreateChangeInfo(member) };

        IBackgroundInput backgroundInput = (IBackgroundInput)parentNode;
        
        if (member is FolderNode folder)
        {
            MergeNode mergeNode = new() { Id = Guid.NewGuid() };
            document.NodeGraph.AddNode(mergeNode);
            document.NodeGraph.AddNode(member);
            
            changes.Add(CreateNode_ChangeInfo.CreateFromNode(mergeNode));
            AppendFolder(backgroundInput, folder, mergeNode, changes);
        }
        else
        {
            document.NodeGraph.AddNode(member);
            List<ConnectProperty_ChangeInfo> connectPropertyChangeInfo =
                NodeOperations.AppendMember(backgroundInput.Background, member.Output, member.Background, member.Id);
            changes.AddRange(connectPropertyChangeInfo);
        }


        ignoreInUndo = false;

        return changes;
    }

    private IChangeInfo CreateChangeInfo(StructureNode member)
    {
        return type switch
        {
            StructureMemberType.Layer => CreateLayer_ChangeInfo.FromLayer(parentFolderGuid,
                (LayerNode)member),
            StructureMemberType.Folder => CreateFolder_ChangeInfo.FromFolder(parentFolderGuid,
                (FolderNode)member),
            _ => throw new NotSupportedException(),
        };
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document document)
    {
        var container = document.FindNodeOrThrow<Node>(parentFolderGuid);
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

    private static void AppendFolder(IBackgroundInput backgroundInput, FolderNode folder, MergeNode mergeNode, List<IChangeInfo> changes)
    {
        var appened = NodeOperations.AppendMember(backgroundInput.Background, mergeNode.Output, mergeNode.Bottom, mergeNode.Id);
        changes.AddRange(appened);
        
        appened = NodeOperations.AppendMember(mergeNode.Top, folder.Output, folder.Background, folder.Id);
        changes.AddRange(appened);
    }

    
}
