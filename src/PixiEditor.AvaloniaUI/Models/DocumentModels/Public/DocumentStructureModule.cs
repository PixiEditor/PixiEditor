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

    public IStructureMemberHandler FindOrThrow(Guid guid) => Find(guid) ?? throw new ArgumentException("Could not find member with guid " + guid.ToString());
    public IStructureMemberHandler? Find(Guid guid)
    {
        List<IStructureMemberHandler>? list = FindPath(guid);
        return list.Count > 0 ? list[0] : null;
    }

    public IStructureMemberHandler? FindFirstWhere(Predicate<IStructureMemberHandler> predicate)
    {
        return FindFirstWhere(predicate, doc.NodeGraphHandler);
    }
    private IStructureMemberHandler? FindFirstWhere(Predicate<IStructureMemberHandler> predicate, INodeGraphHandler graphVM)
    {
        IStructureMemberHandler? result = null;
        graphVM.TryTraverse(node =>
        {
            if (node is IStructureMemberHandler structureMemberNode && predicate(structureMemberNode))
            {
                result = structureMemberNode;
                return false;
            }

            return true;
        });
        
        return result;
    }

    public (IStructureMemberHandler?, IFolderHandler?) FindChildAndParent(Guid childGuid)
    {
        List<IStructureMemberHandler>? path = FindPath(childGuid);
        return path.Count switch
        {
            0 => (null, null),
            1 => (path[0], null),
            >= 2 => (path[0], (IFolderHandler)path[1]),
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
        FillPath(doc.NodeGraphHandler.OutputNode, guid, list);
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
    
    private bool FillPath(INodeHandler node, Guid guid, List<INodeHandler> toFill)
    {
        if (node.Id == guid)
        {
            return true;
        }

        if (node is IStructureMemberHandler structureNode)
        {
            toFill.Add(structureNode);
        }

        bool found = false;

        node.TraverseBackwards(newNode =>
        {
            if (newNode is IStructureMemberHandler strNode && newNode.Id == guid)
            {
                toFill.Add(strNode);
                found = true;
                return false;
            }

            return true;
        });

        return found;
    }
}
