using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels.Public;
#nullable enable
internal class DocumentOperationsModule
{
    private DocumentViewModel Document { get; }
    private DocumentInternalParts Internals { get; }

    public DocumentOperationsModule(DocumentViewModel document, DocumentInternalParts internals)
    {
        Document = document;
        Internals = internals;
    }

    public void SelectAll()
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddFinishedActions(
            new SelectRectangle_Action(new RectI(VecI.Zero, Document.SizeBindable), SelectionMode.Add),
            new EndSelectRectangle_Action());
    }

    public void ClearSelection()
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddFinishedActions(new ClearSelection_Action());
    }

    public void DeleteSelectedPixels(bool clearSelection = false)
    {
        var member = Document.SelectedStructureMember;
        if (Internals.ChangeController.IsChangeActive || member is null)
            return;
        bool drawOnMask = member is LayerViewModel layer ? layer.ShouldDrawOnMask : true;
        if (drawOnMask && !member.HasMaskBindable)
            return;
        Internals.ActionAccumulator.AddActions(new ClearSelectedArea_Action(member.GuidValue, drawOnMask));
        if (clearSelection)
            Internals.ActionAccumulator.AddActions(new ClearSelection_Action());
        Internals.ActionAccumulator.AddFinishedActions();
    }

    public void SetMemberOpacity(Guid memberGuid, float value)
    {
        if (Internals.ChangeController.IsChangeActive || value is > 1 or < 0)
            return;
        Internals.ActionAccumulator.AddFinishedActions(
            new StructureMemberOpacity_Action(memberGuid, value),
            new EndStructureMemberOpacity_Action());
    }

    public void AddOrUpdateViewport(ViewportInfo info) => Internals.ActionAccumulator.AddActions(new RefreshViewport_PassthroughAction(info));

    public void RemoveViewport(Guid viewportGuid) => Internals.ActionAccumulator.AddActions(new RemoveViewport_PassthroughAction(viewportGuid));

    public void ClearUndo()
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddActions(new DeleteRecordedChanges_Action());
    }

    public void PasteImagesAsLayers(List<(string? name, Surface image)> images)
    {
        if (Internals.ChangeController.IsChangeActive)
            return;

        RectI maxSize = new RectI(VecI.Zero, Document.SizeBindable);
        foreach (var imageWithName in images)
        {
            maxSize = maxSize.Union(new RectI(VecI.Zero, imageWithName.image.Size));
        }

        if (maxSize.Size != Document.SizeBindable)
            Internals.ActionAccumulator.AddActions(new ResizeCanvas_Action(maxSize.Size, ResizeAnchor.TopLeft));

        foreach (var imageWithName in images)
        {
            var layerGuid = Internals.StructureHelper.CreateNewStructureMember(StructureMemberType.Layer, imageWithName.name, true);
            DrawImage(imageWithName.image, new ShapeCorners(new RectD(VecD.Zero, imageWithName.image.Size)), layerGuid, true, false, false);
        }
        Internals.ActionAccumulator.AddFinishedActions();
    }

    public Guid? CreateStructureMember(StructureMemberType type, string? name = null)
    {
        if (Internals.ChangeController.IsChangeActive)
            return null;
        return Internals.StructureHelper.CreateNewStructureMember(type, name, true);
    }

    public void DuplicateLayer(Guid guidValue)
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddFinishedActions(new DuplicateLayer_Action(guidValue));
    }

    public void DeleteStructureMember(Guid guidValue)
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddFinishedActions(new DeleteStructureMember_Action(guidValue));
    }

    public void DeleteStructureMembers(IReadOnlyList<Guid> guids)
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddFinishedActions(guids.Select(static guid => new DeleteStructureMember_Action(guid)).ToArray());
    }

    public void ResizeCanvas(VecI newSize, ResizeAnchor anchor)
    {
        if (Internals.ChangeController.IsChangeActive || newSize.X > 9999 || newSize.Y > 9999 || newSize.X < 1 || newSize.Y < 1)
            return;
        Internals.ActionAccumulator.AddFinishedActions(new ResizeCanvas_Action(newSize, anchor));
    }

    public void ResizeImage(VecI newSize, ResamplingMethod resampling)
    {
        if (Internals.ChangeController.IsChangeActive || newSize.X > 9999 || newSize.Y > 9999 || newSize.X < 1 || newSize.Y < 1)
            return;
        Internals.ActionAccumulator.AddFinishedActions(new ResizeImage_Action(newSize, resampling));
    }

    public void ReplaceColor(Color oldColor, Color newColor)
    {
        if (Internals.ChangeController.IsChangeActive || oldColor == newColor)
            return;
        Internals.ActionAccumulator.AddFinishedActions(new ReplaceColor_Action(oldColor, newColor));
    }

    public void CreateMask(StructureMemberViewModel member)
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        if (!member.MaskIsVisibleBindable)
            Internals.ActionAccumulator.AddActions(new StructureMemberMaskIsVisible_Action(true, member.GuidValue));
        Internals.ActionAccumulator.AddFinishedActions(new CreateStructureMemberMask_Action(member.GuidValue));
    }

    public void DeleteMask(StructureMemberViewModel member)
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddFinishedActions(new DeleteStructureMemberMask_Action(member.GuidValue));
    }

    public void SetSelectedMember(Guid memberGuid) => Internals.ActionAccumulator.AddActions(new SetSelectedMember_PassthroughAction(memberGuid));

    public void AddSoftSelectedMember(Guid memberGuid) => Internals.ActionAccumulator.AddActions(new AddSoftSelectedMember_PassthroughAction(memberGuid));

    public void RemoveSoftSelectedMember(Guid memberGuid) => Internals.ActionAccumulator.AddActions(new RemoveSoftSelectedMember_PassthroughAction(memberGuid));

    public void ClearSoftSelectedMembers() => Internals.ActionAccumulator.AddActions(new ClearSoftSelectedMembers_PassthroughAction());

    public void Undo()
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddActions(new Undo_Action());
    }

    public void Redo()
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddActions(new Redo_Action());
    }

    public void MoveStructureMember(Guid memberToMove, Guid memberToMoveIntoOrNextTo, StructureMemberPlacement placement)
    {
        if (Internals.ChangeController.IsChangeActive || memberToMove == memberToMoveIntoOrNextTo)
            return;
        Internals.StructureHelper.TryMoveStructureMember(memberToMove, memberToMoveIntoOrNextTo, placement);
    }

    public void MergeStructureMembers(IReadOnlyList<Guid> members)
    {
        if (Internals.ChangeController.IsChangeActive || members.Count < 2)
            return;
        var (child, parent) = Document.StructureHelper.FindChildAndParent(members[0]);
        if (child is null || parent is null)
            return;
        int index = parent.Children.IndexOf(child);
        Guid newGuid = Guid.NewGuid();

        //make a new layer, put combined image onto it, delete layers that were merged
        Internals.ActionAccumulator.AddActions(
            new CreateStructureMember_Action(parent.GuidValue, newGuid, index, StructureMemberType.Layer),
            new StructureMemberName_Action(newGuid, child.NameBindable),
            new CombineStructureMembersOnto_Action(members.ToHashSet(), newGuid));
        foreach (var member in members)
            Internals.ActionAccumulator.AddActions(new DeleteStructureMember_Action(member));
        Internals.ActionAccumulator.AddActions(new ChangeBoundary_Action());
    }

    public void PasteImageWithTransform(Surface image, VecI startPos)
    {
        if (Document.SelectedStructureMember is null)
            return;
        Internals.ChangeController.TryStartExecutor(new PasteImageExecutor(image, startPos));
    }

    public void TransformSelectedArea(bool toolLinked)
    {
        if (Document.SelectedStructureMember is null ||
            Internals.ChangeController.IsChangeActive && !toolLinked ||
            Document.SelectionPathBindable.IsEmpty)
            return;
        Internals.ChangeController.TryStartExecutor(new TransformSelectedAreaExecutor(toolLinked));
    }

    public void TryStopToolLinkedExecutor()
    {
        if (Internals.ChangeController.GetCurrentExecutorType() == ExecutorType.ToolLinked)
            Internals.ChangeController.TryStopActiveExecutor();
    }

    public void DrawImage(Surface image, ShapeCorners corners, Guid memberGuid, bool ignoreClipSymmetriesEtc, bool drawOnMask) =>
        DrawImage(image, corners, memberGuid, ignoreClipSymmetriesEtc, drawOnMask, true);

    private void DrawImage(Surface image, ShapeCorners corners, Guid memberGuid, bool ignoreClipSymmetriesEtc, bool drawOnMask, bool finish)
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddActions(
            new PasteImage_Action(image, corners, memberGuid, ignoreClipSymmetriesEtc, drawOnMask),
            new EndPasteImage_Action());
        if (finish)
            Internals.ActionAccumulator.AddFinishedActions();
    }

    public void ClipCanvas()
    {
        if (Internals.ChangeController.IsChangeActive)
            return;
        Internals.ActionAccumulator.AddFinishedActions(new ClipCanvas_Action());
    }
}
