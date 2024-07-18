using System.Collections.Generic;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.Public;
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
        return doc.NodeGraphHandler.AllNodes.FirstOrDefault(x => x.Id == guid && x is T) as T;
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

    public (IStructureMemberHandler?, INodeHandler?) FindChildAndParent(Guid childGuid)
    {
        List<IStructureMemberHandler>? path = FindPath(childGuid);
        return path.Count switch
        {
            0 => (null, null),
            1 => (path[0], null),
            >= 2 => (path[0], path[1]),
            _ => (null, null),
        };
    }

    public (IStructureMemberHandler, IFolderHandler) FindChildAndParentOrThrow(Guid childGuid)
    {
        List<IStructureMemberHandler>? path = FindPath(childGuid);
        if (path.Count < 2)
            throw new ArgumentException("Couldn't find child and parent");
        return (path[0], (IFolderHandler)path[1]);
    }

    public List<IStructureMemberHandler> FindPath(Guid guid)
    {
        List<INodeHandler>? list = new List<INodeHandler>();
        var targetNode = FindNode<INodeHandler>(guid);
        if (targetNode == null) return [];
        FillPath(targetNode, list);
        return list.Cast<IStructureMemberHandler>().ToList();
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

    private void FillPath(INodeHandler node, List<INodeHandler> toFill)
    {
        node.TraverseForwards(newNode =>
        {
            if (newNode is IStructureMemberHandler strNode)
            {
                toFill.Add(strNode);
            }

            return true;
        });
    }

    public INodeHandler? GetFirstForwardNode(INodeHandler startNode)
    {
        INodeHandler? result = null;
        startNode.TraverseForwards(node =>
        {
            if(node == startNode)
                return true;
            
            result = node;
            return false;
        });

        return result;
    }

    public IStructureMemberHandler? GetAboveMember(Guid memberId, bool includeFolders)
    {
        INodeHandler member = FindNode<INodeHandler>(memberId);
        if (member == null)
            return null;
        
        IStructureMemberHandler? result = null;
        member.TraverseForwards(node =>
        {
            if (node != member && node is IStructureMemberHandler structureMemberNode)
            {
                if(node is IFolderHandler && !includeFolders)
                    return true;
                
                result = structureMemberNode;
                return false;
            }

            return true;
        });
        
        return result;
    }
    
    public IStructureMemberHandler? GetBelowMember(Guid memberId, bool includeFolders)
    {
        INodeHandler member = FindNode<INodeHandler>(memberId);
        if (member == null)
            return null;
        
        IStructureMemberHandler? result = null;
        member.TraverseBackwards(node =>
        {
            if (node != member && node is IStructureMemberHandler structureMemberNode)
            {
                if(node is IFolderHandler && !includeFolders)
                    return true;
                
                result = structureMemberNode;
                return false;
            }

            return true;
        });
        
        return result;
    }
}
