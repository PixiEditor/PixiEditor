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
        return FindFirstWhere(predicate, doc.StructureRoot);
    }
    private IStructureMemberHandler? FindFirstWhere(Predicate<IStructureMemberHandler> predicate, IFolderHandler folderVM)
    {
        foreach (IStructureMemberHandler? child in folderVM.Children)
        {
            if (predicate(child))
                return child;
            if (child is IFolderHandler innerFolderVM)
            {
                IStructureMemberHandler? result = FindFirstWhere(predicate, innerFolderVM);
                if (result is not null)
                    return result;
            }
        }
        return null;
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
        List<IStructureMemberHandler>? list = new List<IStructureMemberHandler>();
        if (FillPath(doc.StructureRoot, guid, list))
            list.Add(doc.StructureRoot);
        return list;
    }
    
    /// <summary>
    ///     Returns all layers in the document.
    /// </summary>
    /// <returns>List of ILayerHandlers. Empty if no layers found.</returns>
    public List<ILayerHandler> GetAllLayers()
    {
        List<ILayerHandler> layers = new List<ILayerHandler>();
        foreach (IStructureMemberHandler? member in doc.StructureRoot.Children)
        {
            if (member is ILayerHandler layer)
                layers.Add(layer);
            else if (member is IFolderHandler folder)
                layers.AddRange(GetAllLayers(folder, layers));
        }
        
        return layers;
    }
    
    private List<ILayerHandler> GetAllLayers(IFolderHandler folder, List<ILayerHandler> layers)
    {
        foreach (IStructureMemberHandler? member in folder.Children)
        {
            if (member is ILayerHandler layer)
                layers.Add(layer);
            else if (member is IFolderHandler innerFolder)
                layers.AddRange(GetAllLayers(innerFolder, layers));
        }
        return layers;
    }

    private bool FillPath(IFolderHandler folder, Guid guid, List<IStructureMemberHandler> toFill)
    {
        if (folder.GuidValue == guid)
        {
            return true;
        }
        foreach (IStructureMemberHandler? member in folder.Children)
        {
            if (member is ILayerHandler childLayer && childLayer.GuidValue == guid)
            {
                toFill.Add(member);
                return true;
            }

            if (member is IFolderHandler childFolder)
            {
                if (FillPath(childFolder, guid, toFill))
                {
                    toFill.Add(childFolder);
                    return true;
                }
            }
        }
        return false;
    }
}
