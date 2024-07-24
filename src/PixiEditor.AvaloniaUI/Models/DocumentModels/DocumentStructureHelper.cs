using System.Collections.Generic;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Layers;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Extensions.Common.Localization;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels;
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

    private string GetUniqueName(string name, INodeHandler node)
    {
        int count = 1;
        node.TraverseBackwards(newNode =>
        {
            if (newNode is IStructureMemberHandler structureMemberHandler)
            {
                string childName = structureMemberHandler.NodeNameBindable;
                if (childName.StartsWith(name))
                    count++;
            }

            return true;
        });
        return $"{name} {count}";
    }

    public Guid CreateNewStructureMember(StructureMemberType type, string? name = null, bool finish = true)
    {
        IStructureMemberHandler? member = doc.SelectedStructureMember;
        if (member is null)
        {
            Guid guid = Guid.NewGuid();
            //put member on top
            internals.ActionAccumulator.AddActions(new CreateStructureMember_Action(
                doc.NodeGraphHandler.OutputNode.Id,
                guid, type));
            name ??= GetUniqueName(
                type == StructureMemberType.Layer
                    ? new LocalizedString("NEW_LAYER")
                    : new LocalizedString("NEW_FOLDER"), doc.NodeGraphHandler.OutputNode);
            internals.ActionAccumulator.AddActions(new StructureMemberName_Action(guid, name));
            if (finish)
                internals.ActionAccumulator.AddFinishedActions();
            return guid;
        }

        if (member is IFolderHandler folder)
        {
            Guid guid = Guid.NewGuid();
            //put member inside folder on top
            internals.ActionAccumulator.AddActions(new CreateStructureMember_Action(folder.Id, guid, type));
            name ??= GetUniqueName(
                type == StructureMemberType.Layer
                    ? new LocalizedString("NEW_LAYER")
                    : new LocalizedString("NEW_FOLDER"), folder);
            internals.ActionAccumulator.AddActions(new StructureMemberName_Action(guid, name));
            if (finish)
                internals.ActionAccumulator.AddFinishedActions();
            return guid;
        }

        if (member is ILayerHandler layer)
        {
            Guid guid = Guid.NewGuid();
            //put member above the layer
            INodeHandler parent = doc.StructureHelper.GetFirstForwardNode(layer);
            if(parent is null)
                parent = doc.NodeGraphHandler.OutputNode;
            
            internals.ActionAccumulator.AddActions(new CreateStructureMember_Action(parent.Id, guid, type));
            name ??= GetUniqueName(
                type == StructureMemberType.Layer
                    ? new LocalizedString("NEW_LAYER")
                    : new LocalizedString("NEW_FOLDER"), parent);
            internals.ActionAccumulator.AddActions(new StructureMemberName_Action(guid, name));
            if (finish)
                internals.ActionAccumulator.AddFinishedActions();
            return guid;
        }

        throw new ArgumentException($"Unknown member type: {type}");
    }

    private void HandleMoveInside(Guid memberToMove, Guid memberToMoveInto)
    {
        if (memberToMoveInto == memberToMove)
            return;

        internals.ActionAccumulator.AddFinishedActions(new MoveStructureMember_Action(memberToMove, memberToMoveInto,
            true));
    }

    private void HandleMoveAboveBelow(Guid memberToMove, Guid referenceMemberId, bool above)
    {
        var referenceMember = doc.StructureHelper.FindNode<INodeHandler>(referenceMemberId);
        var memberToMoveInto = !above ? referenceMember : doc.StructureHelper.GetFirstForwardNode(referenceMember);
        internals.ActionAccumulator.AddFinishedActions(
            new MoveStructureMember_Action(memberToMove, memberToMoveInto.Id, above && memberToMoveInto is IFolderHandler));
    }

    public void TryMoveStructureMember(Guid memberToMove, Guid memberToMoveIntoOrNextTo,
        StructureMemberPlacement placement)
    {
        switch (placement)
        {
            case StructureMemberPlacement.Above:
                HandleMoveAboveBelow(memberToMove, memberToMoveIntoOrNextTo, true);
                break;
            case StructureMemberPlacement.Below:
                HandleMoveAboveBelow(memberToMove, memberToMoveIntoOrNextTo, false);
                break;
            case StructureMemberPlacement.Inside:
                HandleMoveInside(memberToMove, memberToMoveIntoOrNextTo);
                break;
            case StructureMemberPlacement.BelowOutsideFolder:
            {
                var path = doc.StructureHelper.FindPath(memberToMoveIntoOrNextTo);
                var folder = path.FirstOrDefault(x => x is IFolderHandler) as IFolderHandler;
                if (folder is null)
                    HandleMoveAboveBelow(memberToMove, memberToMoveIntoOrNextTo, false);
                else
                    HandleMoveAboveBelow(memberToMove, folder.Id, false);
            }
                break;
        }
    }
}
