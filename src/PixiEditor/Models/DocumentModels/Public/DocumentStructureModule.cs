using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Parser;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels.Public;
#nullable enable
internal class DocumentStructureModule
{
    private readonly DocumentViewModel doc;
    public DocumentStructureModule(DocumentViewModel owner)
    {
        this.doc = owner;
    }

    public StructureMemberViewModel FindOrThrow(Guid guid) => Find(guid) ?? throw new ArgumentException("Could not find member with guid " + guid.ToString());
    public StructureMemberViewModel? Find(Guid guid)
    {
        List<StructureMemberViewModel>? list = FindPath(guid);
        return list.Count > 0 ? list[0] : null;
    }

    public StructureMemberViewModel? FindFirstWhere(Predicate<StructureMemberViewModel> predicate)
    {
        return FindFirstWhere(predicate, doc.StructureRoot);
    }
    private StructureMemberViewModel? FindFirstWhere(Predicate<StructureMemberViewModel> predicate, FolderViewModel folderVM)
    {
        foreach (StructureMemberViewModel? child in folderVM.Children)
        {
            if (predicate(child))
                return child;
            if (child is FolderViewModel innerFolderVM)
            {
                StructureMemberViewModel? result = FindFirstWhere(predicate, innerFolderVM);
                if (result is not null)
                    return result;
            }
        }
        return null;
    }

    public (StructureMemberViewModel?, FolderViewModel?) FindChildAndParent(Guid childGuid)
    {
        List<StructureMemberViewModel>? path = FindPath(childGuid);
        return path.Count switch
        {
            0 => (null, null),
            1 => (path[0], null),
            >= 2 => (path[0], (FolderViewModel)path[1]),
            _ => (null, null),
        }; 
    }

    public (StructureMemberViewModel, FolderViewModel) FindChildAndParentOrThrow(Guid childGuid)
    {
        List<StructureMemberViewModel>? path = FindPath(childGuid);
        if (path.Count < 2)
            throw new ArgumentException("Couldn't find child and parent");
        return (path[0], (FolderViewModel)path[1]);
    }
    public List<StructureMemberViewModel> FindPath(Guid guid)
    {
        List<StructureMemberViewModel>? list = new List<StructureMemberViewModel>();
        if (FillPath(doc.StructureRoot, guid, list))
            list.Add(doc.StructureRoot);
        return list;
    }
    
    /// <summary>
    ///     Returns all layers in the document.
    /// </summary>
    /// <returns>List of LayerViewModels. Empty if no layers found.</returns>
    public List<LayerViewModel> GetAllLayers()
    {
        List<LayerViewModel> layers = new List<LayerViewModel>();
        foreach (StructureMemberViewModel? member in doc.StructureRoot.Children)
        {
            if (member is LayerViewModel layer)
                layers.Add(layer);
            else if (member is FolderViewModel folder)
                layers.AddRange(GetAllLayers(folder, layers));
        }
        
        return layers;
    }
    
    private List<LayerViewModel> GetAllLayers(FolderViewModel folder, List<LayerViewModel> layers)
    {
        foreach (StructureMemberViewModel? member in folder.Children)
        {
            if (member is LayerViewModel layer)
                layers.Add(layer);
            else if (member is FolderViewModel innerFolder)
                layers.AddRange(GetAllLayers(innerFolder, layers));
        }
        return layers;
    }

    private bool FillPath(FolderViewModel folder, Guid guid, List<StructureMemberViewModel> toFill)
    {
        if (folder.GuidValue == guid)
        {
            return true;
        }
        foreach (StructureMemberViewModel? member in folder.Children)
        {
            if (member is LayerViewModel childLayer && childLayer.GuidValue == guid)
            {
                toFill.Add(member);
                return true;
            }
            if (member is FolderViewModel childFolder)
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
