using System.Collections.Generic;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Containers;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class DocumentStructureHelper
{
    private IDocument doc;
    private DocumentInternalParts internals;
    public DocumentStructureHelper(IDocument doc, DocumentInternalParts internals)
    {
        this.doc = doc;
        this.internals = internals;
    }

    private string GetUniqueName(string name, IFolderHandler folder)
    {
        int count = 1;
        foreach (var child in folder.Children)
        {
            string childName = child.NameBindable;
            if (childName.StartsWith(name))
                count++;
        }
        return $"{name} {count}";
    }

    public Guid CreateNewStructureMember(StructureMemberType type, string? name = null, bool finish = true)
    {
        IStructureMemberHandler? member = doc.SelectedStructureMember;
        if (member is null)
        {
            Guid guid = Guid.NewGuid();
            //put member on top
            internals.ActionAccumulator.AddActions(new CreateStructureMember_Action(doc.StructureRoot.GuidValue, guid, doc.StructureRoot.Children.Count, type));
            name ??= GetUniqueName(type == StructureMemberType.Layer ? new LocalizedString("NEW_LAYER") : new LocalizedString("NEW_FOLDER"), doc.StructureRoot);
            internals.ActionAccumulator.AddActions(new StructureMemberName_Action(guid, name));
            if (finish)
                internals.ActionAccumulator.AddFinishedActions();
            return guid;
        }
        if (member is IFolderHandler folder)
        {
            Guid guid = Guid.NewGuid();
            //put member inside folder on top
            internals.ActionAccumulator.AddActions(new CreateStructureMember_Action(folder.GuidValue, guid, folder.Children.Count, type));
            name ??= GetUniqueName(type == StructureMemberType.Layer ? new LocalizedString("NEW_LAYER") : new LocalizedString("NEW_FOLDER"), folder);
            internals.ActionAccumulator.AddActions(new StructureMemberName_Action(guid, name));
            if (finish)
                internals.ActionAccumulator.AddFinishedActions();
            return guid;
        }
        if (member is ILayerHandler layer)
        {
            Guid guid = Guid.NewGuid();
            //put member above the layer
            List<IStructureMemberHandler> path = doc.StructureHelper.FindPath(layer.GuidValue);
            if (path.Count < 2)
                throw new InvalidOperationException("Couldn't find a path to the selected member");
            IFolderHandler parent = (IFolderHandler)path[1];
            internals.ActionAccumulator.AddActions(new CreateStructureMember_Action(parent.GuidValue, guid, parent.Children.IndexOf(layer) + 1, type));
            name ??= GetUniqueName(type == StructureMemberType.Layer ? new LocalizedString("NEW_LAYER") : new LocalizedString("NEW_FOLDER"), parent);
            internals.ActionAccumulator.AddActions(new StructureMemberName_Action(guid, name));
            if (finish)
                internals.ActionAccumulator.AddFinishedActions();
            return guid;
        }
        throw new ArgumentException($"Unknown member type: {type}");
    }

    private void HandleMoveInside(List<IStructureMemberHandler> memberToMovePath, List<IStructureMemberHandler> memberToMoveIntoPath)
    {
        if (memberToMoveIntoPath[0] is not IFolderHandler folder || memberToMoveIntoPath.Contains(memberToMovePath[0]))
            return;
        int index = folder.Children.Count;
        if (memberToMoveIntoPath[0].GuidValue == memberToMovePath[1].GuidValue) // member is already in this folder
            index--;
        internals.ActionAccumulator.AddFinishedActions(new MoveStructureMember_Action(memberToMovePath[0].GuidValue, folder.GuidValue, index));
        return;
    }

    private void HandleMoveAboveBelow(List<IStructureMemberHandler> memberToMovePath, List<IStructureMemberHandler> memberToMoveRelativeToPath, bool above)
    {
        IFolderHandler targetFolder = (IFolderHandler)memberToMoveRelativeToPath[1];
        if (memberToMovePath[1].GuidValue == memberToMoveRelativeToPath[1].GuidValue)
        { // members are in the same folder
            int indexOfMemberToMove = targetFolder.Children.IndexOf(memberToMovePath[0]);
            int indexOfMemberToMoveAbove = targetFolder.Children.IndexOf(memberToMoveRelativeToPath[0]);
            int index = indexOfMemberToMoveAbove;
            if (above)
                index++;
            if (indexOfMemberToMove < indexOfMemberToMoveAbove)
                index--;
            internals.ActionAccumulator.AddFinishedActions(new MoveStructureMember_Action(memberToMovePath[0].GuidValue, targetFolder.GuidValue, index));
        }
        else
        { // members are in different folders
            if (memberToMoveRelativeToPath.Contains(memberToMovePath[0]))
                return;
            int index = targetFolder.Children.IndexOf(memberToMoveRelativeToPath[0]);
            if (above)
                index++;
            internals.ActionAccumulator.AddFinishedActions(new MoveStructureMember_Action(memberToMovePath[0].GuidValue, targetFolder.GuidValue, index));
        }
    }

    public void TryMoveStructureMember(Guid memberToMove, Guid memberToMoveIntoOrNextTo, StructureMemberPlacement placement)
    {
        List<IStructureMemberHandler> memberPath = doc.StructureHelper.FindPath(memberToMove);
        List<IStructureMemberHandler> refPath = doc.StructureHelper.FindPath(memberToMoveIntoOrNextTo);
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
                    IFolderHandler refFolder = (IFolderHandler)refPath[1];
                    int refIndexInParent = refFolder.Children.IndexOf(refPath[0]);
                    if (refIndexInParent > 0 || refPath.Count == 2)
                    {
                        HandleMoveAboveBelow(memberPath, refPath, false);
                        break;
                    }
                    HandleMoveAboveBelow(memberPath, doc.StructureHelper.FindPath(refPath[1].GuidValue), false);
                }
                break;
        }
    }
}
