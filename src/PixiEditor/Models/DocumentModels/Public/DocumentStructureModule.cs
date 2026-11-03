using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.GraphNavigation;
using PixiEditor.Helpers.Nodes;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.DocumentModels.Public;
#nullable enable
internal class DocumentStructureModule
{
    private readonly IDocument doc;

    public DocumentStructureModule(IDocument owner)
    {
        this.doc = owner;
    }

    public IStructureMemberHandler FindOrThrow(Guid guid) => Find(guid) ??
                                                             throw new ArgumentException(
                                                                 "Could not find member with guid " + guid.ToString());

    public IStructureMemberHandler? Find(Guid guid)
    {
        return FindNode<IStructureMemberHandler>(guid);
    }

    public T? FindNode<T>(Guid guid) where T : class, INodeHandler
    {
        return doc.NodeGraphHandler.NodeLookup.TryGetValue(guid, out var found) ? found as T : null;
    }

    public bool TryFindNode<T>(Guid guid, out T found) where T : class, INodeHandler
    {
        found = FindNode<T>(guid);
        return found != null;
    }

    public Guid FindClosestMember(IReadOnlyList<Guid> guids)
    {
        IStructureMemberHandler? firstNode = FindNode<IStructureMemberHandler>(guids[0]);
        if (firstNode is null)
            return Guid.Empty;

        var parent = firstNode.Navigate().FirstOrDefault(
            NavigationDirection.Forwards,
            node => !guids.Contains(node.Id) && node is IStructureMemberHandler);

        if (parent is null)
        {
            var lastNode = FindNode<IStructureMemberHandler>(guids[^1]);
            if (lastNode is null)
                return Guid.Empty;

            parent = lastNode
                .Navigate()
                .FirstOrDefault(NavigationDirection.Backwards, traversedNode => !guids.Contains(traversedNode.Id) && traversedNode is IStructureMemberHandler);
        }

        if (parent is null)
            return Guid.Empty;

        return parent.Id;
    }

    public INodeHandler? FindFirstWhere(Predicate<INodeHandler> predicate)
    {
        return FindFirstWhere(predicate, doc.NodeGraphHandler);
    }

    private INodeHandler? FindFirstWhere(
        Predicate<INodeHandler> predicate,
        INodeGraphHandler graphVM)
    {
        INodeHandler? result = null;
        graphVM.TryTraverse(node =>
        {
            if (predicate(node))
            {
                result = node;
                return false;
            }

            return true;
        });

        return result;
    }

    public IEnumerable<IStructureMemberHandler> EnumerateParents(Guid child)
    {
        var childNode = FindNode<IStructureMemberHandler>(child);
        if (childNode == null)
            return [];

        return childNode.Navigate()
            .NodesOfTypeWhere<IStructureMemberHandler>(NavigationDirection.Forwards,
                x => x.InputProperty.PropertyName == FolderNode.ContentInternalName);
    }

    public (IStructureMemberHandler, IFolderHandler) FindChildAndParentOrThrow(Guid childGuid)
    {
        var path = FindPath(childGuid).ToList();
        
        return path.Count < 2 ? throw new ArgumentException("Couldn't find child and parent") : (path[0], (IFolderHandler)path[1]);
    }

    public IEnumerable<IStructureMemberHandler> FindPath(Guid guid)
    {
        var targetNode = FindNode<INodeHandler>(guid);
        
        return targetNode != null
            ? targetNode.Navigate().NodesOfType<IStructureMemberHandler>(NavigationDirection.Forwards)
            : [];
    }

    /// <summary>
    ///     Returns all layers in the document.
    /// </summary>
    /// <returns>List of ILayerHandlers. Empty if no layers found.</returns>
    public List<ILayerHandler> GetAllLayers()
    {
        List<ILayerHandler> layers = new List<ILayerHandler>();

        doc.NodeGraphHandler.TryTraverse(node =>
        {
            if (node is ILayerHandler layer)
                layers.Add(layer);
            return true;
        });

        return layers;
    }

    public List<IStructureMemberHandler> TraverseAllMembers()
    {
        List<IStructureMemberHandler> members = new List<IStructureMemberHandler>();

        doc.NodeGraphHandler.TryTraverse(node =>
        {
            if (node is IStructureMemberHandler member)
                members.Add(member);
            return true;
        });

        return members;
    }

    public IEnumerable<IStructureMemberHandler> EnumerateAllMembers() =>
        doc.NodeGraphHandler.AllNodes.OfType<IStructureMemberHandler>();

    public IStructureMemberHandler? GetAboveMember(Guid memberId, bool includeFolders)
    {
        var member = FindNode<INodeHandler>(memberId);

        var result = member?.Navigate().FirstOrDefault(
            NavigationDirection.Forwards,
            node => node is IStructureMemberHandler && (includeFolders || node is not IFolderHandler),
            yieldOrigin: false) as IStructureMemberHandler;

        return result;
    }

    public IStructureMemberHandler? GetBelowMember(Guid memberId, bool includeFolders)
    {
        var member = FindNode<INodeHandler>(memberId);

        var result = member?.Navigate().FirstOrDefault(
            NavigationDirection.Backwards,
            node => node is IStructureMemberHandler && (includeFolders || node is not IFolderHandler),
            yieldOrigin: false) as IStructureMemberHandler;

        return result;
    }

    public IEnumerable<IStructureMemberHandler> EnumerateFolderChildren(Guid folderId)
    {
        var folder = FindNode<INodeHandler>(folderId);
        var connectionInput = folder?.Inputs.FirstOrDefault(x => x.PropertyName == FolderNode.ContentInternalName);
        if (folder == null || connectionInput?.ConnectedOutput == null)
            return [];

        return connectionInput.ConnectedOutput.Node.Navigate().NodesOfType<IStructureMemberHandler>(NavigationDirection.Forwards);
    }

    public List<IStructureMemberHandler> GetAllMembersInOrder()
    {
        var allMembers = doc.NodeGraphHandler.StructureTree.Members;

        List<IStructureMemberHandler> membersInOrder = new List<IStructureMemberHandler>();
        for (var i = allMembers.Count - 1; i >= 0; i--)
        {
            var member = allMembers[i];
            membersInOrder.Add(member);

            if (member is IFolderHandler folder)
            {
                membersInOrder.AddRange(EnumerateFolderChildren(folder.Id));
            }
            else
            {
                membersInOrder.Add(member);
            }
        }

        return membersInOrder;
    }
}
