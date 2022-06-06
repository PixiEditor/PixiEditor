using System;
using System.Collections.Generic;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditorPrototype.ViewModels;

namespace PixiEditorPrototype.Models;

internal class DocumentStructureHelper
{
    private DocumentViewModel doc;
    private DocumentHelpers helpers;
    public DocumentStructureHelper(DocumentViewModel doc, DocumentHelpers helpers)
    {
        this.doc = doc;
        this.helpers = helpers;
    }

    public void CreateNewStructureMember(StructureMemberType type)
    {
        var member = doc.FindFirstSelectedMember();
        if (member is null)
        {
            var guid = Guid.NewGuid();
            //put member on top
            helpers.ActionAccumulator.AddActions(new CreateStructureMember_Action(doc.StructureRoot.GuidValue, guid, doc.StructureRoot.Children.Count, type));
            helpers.ActionAccumulator.AddFinishedActions(new StructureMemberName_Action(guid, type == StructureMemberType.Layer ? "New Layer" : "New Folder"));
            return;
        }
        if (member is FolderViewModel folder)
        {
            var guid = Guid.NewGuid();
            //put member inside folder on top
            helpers.ActionAccumulator.AddActions(new CreateStructureMember_Action(folder.GuidValue, guid, folder.Children.Count, type));
            helpers.ActionAccumulator.AddFinishedActions(new StructureMemberName_Action(guid, type == StructureMemberType.Layer ? "New Layer" : "New Folder"));
            return;
        }
        if (member is LayerViewModel layer)
        {
            var guid = Guid.NewGuid();
            //put member above the layer
            var path = FindPath(layer.GuidValue);
            if (path.Count < 2)
                throw new InvalidOperationException("Couldn't find a path to the selected member");
            var parent = (FolderViewModel)path[1];
            helpers.ActionAccumulator.AddActions(new CreateStructureMember_Action(parent.GuidValue, guid, parent.Children.IndexOf(layer) + 1, type));
            helpers.ActionAccumulator.AddFinishedActions(new StructureMemberName_Action(guid, type == StructureMemberType.Layer ? "New Layer" : "New Folder"));
            return;
        }
        throw new ArgumentException("Unknown member type: " + type.ToString());
    }

    public StructureMemberViewModel FindOrThrow(Guid guid) => Find(guid) ?? throw new ArgumentException("Could not find member with guid " + guid.ToString());
    public StructureMemberViewModel? Find(Guid guid)
    {
        var list = FindPath(guid);
        return list.Count > 0 ? list[0] : null;
    }

    public StructureMemberViewModel? FindFirstWhere(Predicate<StructureMemberViewModel> predicate)
    {
        return FindFirstWhere(predicate, doc.StructureRoot);
    }
    private StructureMemberViewModel? FindFirstWhere(Predicate<StructureMemberViewModel> predicate, FolderViewModel folderVM)
    {
        foreach (var child in folderVM.Children)
        {
            if (predicate(child))
                return child;
            if (child is FolderViewModel innerFolderVM)
            {
                var result = FindFirstWhere(predicate, innerFolderVM);
                if (result is not null)
                    return result;
            }
        }
        return null;
    }

    public (StructureMemberViewModel, FolderViewModel) FindChildAndParentOrThrow(Guid childGuid)
    {
        var path = FindPath(childGuid);
        if (path.Count < 2)
            throw new ArgumentException("Couldn't find child and parent");
        return (path[0], (FolderViewModel)path[1]);
    }
    public List<StructureMemberViewModel> FindPath(Guid guid)
    {
        var list = new List<StructureMemberViewModel>();
        if (FillPath(doc.StructureRoot, guid, list))
            list.Add(doc.StructureRoot);
        return list;
    }

    private bool FillPath(FolderViewModel folder, Guid guid, List<StructureMemberViewModel> toFill)
    {
        if (folder.GuidValue == guid)
        {
            return true;
        }
        foreach (var member in folder.Children)
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

    public void MoveStructureMember(Guid guid, bool toSmallerIndex)
    {
        var path = FindPath(guid);
        if (path.Count < 2)
            throw new ArgumentException("Couldn't find the member to be moved");
        if (path.Count == 2)
        {
            int curIndex = doc.StructureRoot.Children.IndexOf(path[0]);
            if (curIndex == 0 && toSmallerIndex || curIndex == doc.StructureRoot.Children.Count - 1 && !toSmallerIndex)
                return;
            helpers.ActionAccumulator.AddFinishedActions(new MoveStructureMember_Action(guid, doc.StructureRoot.GuidValue, toSmallerIndex ? curIndex - 1 : curIndex + 1));
            return;
        }
        var folder = (FolderViewModel)path[1];
        int index = folder.Children.IndexOf(path[0]);
        if (toSmallerIndex && index > 0 || !toSmallerIndex && index < folder.Children.Count - 1)
        {
            helpers.ActionAccumulator.AddFinishedActions(new MoveStructureMember_Action(guid, path[1].GuidValue, toSmallerIndex ? index - 1 : index + 1));
        }
        else
        {
            int parentIndex = ((FolderViewModel)path[2]).Children.IndexOf(folder);
            helpers.ActionAccumulator.AddFinishedActions(new MoveStructureMember_Action(guid, path[2].GuidValue, toSmallerIndex ? parentIndex : parentIndex + 1));
        }
    }
}
