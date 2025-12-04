using System.Collections.Generic;
using PixiEditor.ChangeableDocument;
using PixiEditor.ViewModels.Document;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Nodes;

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

            return Traverse.Further;
        });
        return $"{name} {count}";
    }

    public Guid CreateNewStructureMember(StructureMemberType type, string? name = null, bool finish = true)
    {
        Type nodeType = type switch
        {
            StructureMemberType.ImageLayer => typeof(ImageLayerNode),
            StructureMemberType.Folder => typeof(FolderNode),
            StructureMemberType.Document => typeof(NestedDocumentNode),
            _ => throw new ArgumentException($"Unknown structure member type: {type}")
        };

        string defaultName = type == StructureMemberType.Folder
            ? new LocalizedString("NEW_FOLDER")
            : new LocalizedString("NEW_LAYER");

        IStructureMemberHandler? member = doc.SelectedStructureMember;
        if (member is null)
        {
            Guid guid = Guid.NewGuid();

            //put member on top
            internals.ActionAccumulator.AddActions(new CreateStructureMember_Action(
                doc.NodeGraphHandler.OutputNode.Id,
                guid, nodeType));
            name ??= GetUniqueName(defaultName, doc.NodeGraphHandler.OutputNode);
            internals.ActionAccumulator.AddActions(new StructureMemberName_Action(guid, name));
            if (finish)
                internals.ActionAccumulator.AddFinishedActions();
            return guid;
        }

        if (member is IFolderHandler folder)
        {
            Guid guid = Guid.NewGuid();
            //put member inside folder on top

            internals.ActionAccumulator.AddActions(new CreateStructureMember_Action(folder.Id, guid, nodeType));
            name ??= GetUniqueName(defaultName, folder);
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
            if (parent is null)
                parent = doc.NodeGraphHandler.OutputNode;

            internals.ActionAccumulator.AddActions(new CreateStructureMember_Action(parent.Id, guid, nodeType));
            name ??= GetUniqueName(defaultName, parent);
            internals.ActionAccumulator.AddActions(new StructureMemberName_Action(guid, name));
            if (finish)
                internals.ActionAccumulator.AddFinishedActions();
            return guid;
        }

        throw new ArgumentException($"Unknown member type: {type}");
    }

    public Guid? CreateNewStructureMember(Type structureMemberType, string? name, ActionSource source)
    {
        Guid guid = Guid.NewGuid();
        var selectedMember = doc.SelectedStructureMember;

        //put member above the layer
        INodeHandler parent = selectedMember != null
            ? doc.StructureHelper.GetFirstForwardNode(selectedMember)
            : doc.NodeGraphHandler.OutputNode;
        if (parent is null)
            parent = doc.NodeGraphHandler.OutputNode;

        internals.ActionAccumulator.AddActions(source,
            new CreateStructureMember_Action(parent.Id, guid, structureMemberType));
        name ??= GetUniqueName(
            structureMemberType.IsAssignableTo(typeof(LayerNode))
                ? new LocalizedString("NEW_LAYER")
                : new LocalizedString("NEW_FOLDER"), parent);
        internals.ActionAccumulator.AddActions(source, new StructureMemberName_Action(guid, name));
        if (source == ActionSource.User)
            internals.ActionAccumulator.AddFinishedActions();
        return guid;
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
        if (memberToMoveInto.Id == memberToMove)
        {
            memberToMoveInto = doc.StructureHelper.GetFirstForwardNode(memberToMoveInto);
        }

        internals.ActionAccumulator.AddFinishedActions(
            new MoveStructureMember_Action(memberToMove, memberToMoveInto.Id,
                above && memberToMoveInto is IFolderHandler folder && folder.Children.Contains(referenceMember)));
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
