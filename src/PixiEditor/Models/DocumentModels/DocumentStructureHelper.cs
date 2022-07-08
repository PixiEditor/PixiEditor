using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
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
        StructureMemberViewModel? member = doc.SelectedStructureMember;
        if (member is null)
        {
            Guid guid = Guid.NewGuid();
            //put member on top
            helpers.ActionAccumulator.AddActions(new CreateStructureMember_Action(doc.StructureRoot.GuidValue, guid, doc.StructureRoot.Children.Count, type));
            helpers.ActionAccumulator.AddFinishedActions(new StructureMemberName_Action(guid, type == StructureMemberType.Layer ? "New Layer" : "New Folder"));
            return;
        }
        if (member is FolderViewModel folder)
        {
            Guid guid = Guid.NewGuid();
            //put member inside folder on top
            helpers.ActionAccumulator.AddActions(new CreateStructureMember_Action(folder.GuidValue, guid, folder.Children.Count, type));
            helpers.ActionAccumulator.AddFinishedActions(new StructureMemberName_Action(guid, type == StructureMemberType.Layer ? "New Layer" : "New Folder"));
            return;
        }
        if (member is LayerViewModel layer)
        {
            Guid guid = Guid.NewGuid();
            //put member above the layer
            List<StructureMemberViewModel>? path = FindPath(layer.GuidValue);
            if (path.Count < 2)
                throw new InvalidOperationException("Couldn't find a path to the selected member");
            FolderViewModel? parent = (FolderViewModel)path[1];
            helpers.ActionAccumulator.AddActions(new CreateStructureMember_Action(parent.GuidValue, guid, parent.Children.IndexOf(layer) + 1, type));
            helpers.ActionAccumulator.AddFinishedActions(new StructureMemberName_Action(guid, type == StructureMemberType.Layer ? "New Layer" : "New Folder"));
            return;
        }
        throw new ArgumentException($"Unknown member type: {type}");
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

    private void HandleMoveInside(List<StructureMemberViewModel> memberToMovePath, List<StructureMemberViewModel> memberToMoveIntoPath)
    {
        if (memberToMoveIntoPath[0] is not FolderViewModel folder)
            return;
        int index = folder.Children.Count;
        if (memberToMoveIntoPath[0].GuidValue == memberToMovePath[1].GuidValue) // member is already in this folder
            index--;
        helpers.ActionAccumulator.AddFinishedActions(new MoveStructureMember_Action(memberToMovePath[0].GuidValue, folder.GuidValue, index));
        return;
    }

    private void HandleMoveAboveBelow(List<StructureMemberViewModel> memberToMovePath, List<StructureMemberViewModel> memberToMoveRelativeToPath, bool above)
    {
        FolderViewModel targetFolder = (FolderViewModel)memberToMoveRelativeToPath[1];
        if (memberToMovePath[1].GuidValue == memberToMoveRelativeToPath[1].GuidValue)
        {
            // members are in the same folder
            int indexOfMemberToMove = targetFolder.Children.IndexOf(memberToMovePath[0]);
            int indexOfMemberToMoveAbove = targetFolder.Children.IndexOf(memberToMoveRelativeToPath[0]);
            int index = indexOfMemberToMoveAbove;
            if (above)
                index++;
            if (indexOfMemberToMove < indexOfMemberToMoveAbove)
                index--;
            helpers.ActionAccumulator.AddFinishedActions(new MoveStructureMember_Action(memberToMovePath[0].GuidValue, targetFolder.GuidValue, index));
        }
        else
        {
            // members are in different folders
            int index = targetFolder.Children.IndexOf(memberToMoveRelativeToPath[0]);
            if (above)
                index++;
            helpers.ActionAccumulator.AddFinishedActions(new MoveStructureMember_Action(memberToMovePath[0].GuidValue, targetFolder.GuidValue, index));
        }
    }

    public void TryMoveStructureMember(Guid memberToMove, Guid memberToMoveIntoOrNextTo, StructureMemberPlacement placement)
    {
        List<StructureMemberViewModel> memberPath = FindPath(memberToMove);
        List<StructureMemberViewModel> refPath = FindPath(memberToMoveIntoOrNextTo);
        if (memberPath.Count < 2 || refPath.Count < 2)
            return;
        switch (placement)
        {
            case StructureMemberPlacement.Above:
                HandleMoveAboveBelow(memberPath, refPath, true);
                break;
            case StructureMemberPlacement.Below:
                HandleMoveAboveBelow(memberPath, refPath, false);
                break;
            case StructureMemberPlacement.Inside:
                HandleMoveInside(memberPath, refPath);
                break;
            case StructureMemberPlacement.BelowOutsideFolder:
                {
                    FolderViewModel refFolder = (FolderViewModel)refPath[1];
                    int refIndexInParent = refFolder.Children.IndexOf(refPath[0]);
                    if (refIndexInParent > 0 || refPath.Count == 2)
                    {
                        HandleMoveAboveBelow(memberPath, refPath, false);
                        break;
                    }
                    HandleMoveAboveBelow(memberPath, FindPath(refPath[1].GuidValue), false);
                }
                break;
        }
    }
}
