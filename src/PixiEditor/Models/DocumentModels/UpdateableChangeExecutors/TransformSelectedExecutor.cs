using System.Collections.Immutable;
using Avalonia.Input;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.Document.Nodes;
using Type = System.Type;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class TransformSelectedExecutor : UpdateableChangeExecutor, ITransformableExecutor, ITransformDraggedEvent,
    IMidChangeUndoableExecutor,
    ITransformStoppedEvent
{
    private Dictionary<Guid, ShapeCorners> memberCorners = new();
    private IMoveToolHandler? tool;
    private bool isInProgress;
    public override ExecutorType Type { get; }

    public override bool BlocksOtherActions => false;

    private List<Guid> selectedMembers = new();
    private List<Guid> originalSelectedMembers = new();

    private ShapeCorners cornersOnStartDuplicate;
    private ShapeCorners lastCorners = new();
    private bool movedOnce;
    private bool duplicateOnStop = false;

    private List<Guid> disabledSnappingMembers = new();

    public TransformSelectedExecutor(bool toolLinked)
    {
        Type = toolLinked ? ExecutorType.ToolLinked : ExecutorType.Regular;
    }

    public override ExecutionState Start()
    {
        tool = GetHandler<IMoveToolHandler>();
        if (tool is null || document!.SelectedStructureMember is null)
            return ExecutionState.Error;

        tool.TransformingSelectedArea = true;
        List<IStructureMemberHandler> members = new();
        originalSelectedMembers = document.SelectedMembers.ToList();
        var guids = document.ExtractSelectedLayers(false);
        members = guids.Select(g => document.StructureHelper.Find(g)).ToList();

        if (!members.Any())
            return ExecutionState.Error;

        document.TransformHandler.PassthroughPointerPressed += OnLeftMouseButtonDown;

        return SelectMembers(members);
    }

    private ExecutionState SelectMembers(List<IStructureMemberHandler> members)
    {
        bool allRaster = true;
        bool anyRaster = false;
        memberCorners = new();
        foreach (IStructureMemberHandler member in members)
        {
            ShapeCorners targetCorners = member.TransformationCorners;

            if (member is IRasterLayerHandler && !document.SelectionPathBindable.IsEmpty)
            {
                targetCorners = new ShapeCorners(document.SelectionPathBindable.TightBounds);
            }
            else if (member is not IRasterLayerHandler)
            {
                allRaster = false;
            }

            if (member is IRasterLayerHandler)
            {
                anyRaster = true;
            }

            memberCorners.Add(member.Id, targetCorners);
        }

        ShapeCorners masterCorners;
        if (memberCorners.Count == 1)
        {
            masterCorners = memberCorners.FirstOrDefault().Value;
        }
        else
        {
            var aabbBounds = memberCorners.Values.Select(static c => c.AABBBounds);
            var bounds = aabbBounds as RectD[] ?? aabbBounds.ToArray();
            if (bounds.Length != 0)
            {
                masterCorners = new ShapeCorners(bounds.Aggregate((a, b) => a.Union(b)));
            }
            else
            {
                return ExecutionState.Error;
            }
        }

        if (masterCorners.AABBBounds.Width == 0 || masterCorners.AABBBounds.Height == 0)
        {
            return ExecutionState.Error;
        }

        DocumentTransformMode mode = allRaster
            ? DocumentTransformMode.Scale_Rotate_Shear_Perspective
            : DocumentTransformMode.Scale_Rotate_Shear_NoPerspective;

        foreach (var structureMemberHandler in members)
        {
            document.SnappingHandler.Remove(structureMemberHandler.Id.ToString());
            disabledSnappingMembers.Add(structureMemberHandler.Id);
            var parents = document.StructureHelper.GetParents(structureMemberHandler.Id);

            foreach (var parent in parents)
            {
                document.SnappingHandler.Remove(parent.Id.ToString());
                if (!disabledSnappingMembers.Contains(parent.Id))
                {
                    disabledSnappingMembers.Add(parent.Id);
                }
            }
        }

        selectedMembers = members.Select(m => m.Id).ToList();

        lastCorners = masterCorners;


        document.TransformHandler.ShowTransform(mode, true, masterCorners,
            Type == ExecutorType.Regular || tool.KeepOriginalImage);

        document.TransformHandler.CanAlignToPixels = anyRaster;

        movedOnce = false;
        isInProgress = true;

        return ExecutionState.Success;
    }

    public override void OnLeftMouseButtonDown(MouseOnCanvasEventArgs args)
    {
        args.Handled = true;

        if (args.ClickCount >= 2)
        {
            if (SwitchToLayerTool())
            {
                return;
            }
        }

        var topMostWithinClick = QueryLayers<ILayerHandler>(args.PositionOnCanvas);

        var nonSelected = topMostWithinClick.Where(x => x != document.SelectedStructureMember
                                                        && !document.SoftSelectedStructureMembers.Contains(x))
            .ToArray();

        bool isHoldingShift = args.KeyModifiers.HasFlag(KeyModifiers.Shift);

        if (nonSelected.Any())
        {
            var topMost = nonSelected.First();

            if (!isHoldingShift)
            {
                document.Operations.ClearSoftSelectedMembers();
                document.Operations.SetSelectedMember(topMost.Id);
            }
            else
            {
                document.Operations.AddSoftSelectedMember(topMost.Id);
            }
        }
        else if (isHoldingShift)
        {
            var topMostList = topMostWithinClick.ToList();
            if (document.SoftSelectedStructureMembers.Count > 0)
            {
                Deselect(topMostList);
            }
        }
    }

    private bool SwitchToLayerTool()
    {
        if (document.SelectedStructureMember is ILayerHandler layerHandler && layerHandler.QuickEditTool != null)
        {
            ViewModelMain.Current.ToolsSubViewModel.SetActiveTool(layerHandler.QuickEditTool, false);
            ViewModelMain.Current.ToolsSubViewModel.QuickToolSwitchInlet();
            return true;
        }

        return false;
    }

    public void OnTransformStopped()
    {
        DuplicateIfRequired();
    }

    private void Deselect(List<ILayerHandler> topMostWithinClick)
    {
        var topMost = topMostWithinClick.FirstOrDefault();
        if (topMost is not null)
        {
            bool deselectingWasMain = document.SelectedStructureMember.Id == topMost.Id;
            if (deselectingWasMain)
            {
                Guid? nextMain = document.SoftSelectedStructureMembers.FirstOrDefault().Id;
                List<Guid> softSelected = document.SoftSelectedStructureMembers
                    .Select(x => x.Id).Where(x => x != nextMain.Value).ToList();

                document.Operations.ClearSoftSelectedMembers();
                document.Operations.SetSelectedMember(nextMain.Value);

                foreach (var guid in softSelected)
                {
                    document.Operations.AddSoftSelectedMember(guid);
                }
            }
            else
            {
                List<Guid> softSelected = document.SoftSelectedStructureMembers
                    .Select(x => x.Id).Where(x => x != topMost.Id).ToList();

                document.Operations.ClearSoftSelectedMembers();

                foreach (var guid in softSelected)
                {
                    document.Operations.AddSoftSelectedMember(guid);
                }
            }
        }
    }

    public override void OnSettingsChanged(string name, object value)
    {
        if (name == nameof(IMoveToolHandler.KeepOriginalImage))
        {
            DoTransform(lastCorners);
        }
    }

    public override void OnMembersSelected(List<Guid> memberGuids)
    {
        if (isInProgress)
        {
            internals.ActionAccumulator.AddActions(new EndTransformSelected_Action());
            internals!.ActionAccumulator.AddActions(new EndPreviewShiftLayers_Action());
            document!.TransformHandler.HideTransform();
            AddSnappingForMembers(selectedMembers);

            selectedMembers.Clear();
            memberCorners.Clear();
            isInProgress = false;
        }
    }

    public bool IsTransforming => isInProgress;

    public void OnTransformChanged(ShapeCorners corners)
    {
        DoTransform(corners);
        lastCorners = corners;
    }

    public void OnTransformDragged(VecD from, VecD to)
    {
        if (!isInProgress)
            return;

        if (tool.DuplicateOnMove)
        {
            if (!duplicateOnStop)
            {
                cornersOnStartDuplicate = lastCorners;
                duplicateOnStop = true;
                internals.ActionAccumulator.AddFinishedActions(new EndTransformSelected_Action());
            }

            VecD delta = new VecD(
                to.X - from.X,
                to.Y - from.Y);

            internals.ActionAccumulator.AddActions(new PreviewShiftLayers_Action(selectedMembers, delta,
                document!.AnimationHandler.ActiveFrameBindable));
        }
    }

    private void DoTransform(ShapeCorners corners)
    {
        if (!isInProgress)
            return;

        if (duplicateOnStop) return;

        if (!movedOnce)
        {
            internals!.ActionAccumulator.AddActions(
                new TransformSelected_Action(lastCorners, tool.KeepOriginalImage, memberCorners, false,
                    document.AnimationHandler.ActiveFrameBindable));

            movedOnce = true;
        }

        internals!.ActionAccumulator.AddActions(
            new TransformSelected_Action(corners, tool!.KeepOriginalImage, memberCorners, false,
                document!.AnimationHandler.ActiveFrameBindable));
    }

    private void DuplicateSelected()
    {
        List<IAction> actions = new();

        List<Guid> newLayerGuids = new();
        List<Guid> newGuidsOfOriginal = new();

        internals.ActionAccumulator.StartChangeBlock();

        actions.Add(new EndPreviewShiftLayers_Action());

        VectorPath? original = document.SelectionPathBindable != null
            ? new VectorPath(document.SelectionPathBindable)
            : null;

        VectorPath? clearArea = null;
        if (original != null)
        {
            var selection = document.SelectionPathBindable;
            var inverse = new VectorPath();
            inverse.AddRect(new RectD(new(0, 0), document.SizeBindable));

            clearArea = inverse.Op(selection, VectorPathOp.Difference);
        }

        for (var i = 0; i < originalSelectedMembers.Count; i++)
        {
            var member = originalSelectedMembers[i];
            Guid newGuid = Guid.NewGuid();
            if (document.StructureHelper.Find(member) is not FolderNodeViewModel folder)
            {
                newLayerGuids.Add(newGuid);
                actions.Add(new DuplicateLayer_Action(member, newGuid));
                if (document.SelectionPathBindable is { IsEmpty: false })
                {
                    actions.Add(new ClearSelectedArea_Action(newGuid, clearArea, false,
                        document.AnimationHandler.ActiveFrameBindable));
                }
            }
            else
            {
                int childCount = folder.CountChildrenRecursive();
                Guid[] newGuidsArray = new Guid[childCount];
                for (var j = 0; j < childCount; j++)
                {
                    newGuidsArray[j] = Guid.NewGuid();
                }

                actions.Add(new DuplicateFolder_Action(member, newGuid, newGuidsArray.ToImmutableList()));

                for (int j = 0; j < childCount; j++)
                {
                    if (document.SelectionPathBindable is { IsEmpty: false })
                    {
                        actions.Add(new ClearSelectedArea_Action(newGuidsArray[j], clearArea, false,
                            document.AnimationHandler.ActiveFrameBindable));
                    }
                }

                newLayerGuids.AddRange(newGuidsArray);
            }

            newGuidsOfOriginal.Add(newGuid);
        }

        internals!.ActionAccumulator.AddFinishedActions(actions.ToArray());

        actions.Clear();

        VecD delta = new VecD(
            lastCorners.AABBBounds.TopLeft.X - cornersOnStartDuplicate.AABBBounds.TopLeft.X,
            lastCorners.AABBBounds.TopLeft.Y - cornersOnStartDuplicate.AABBBounds.TopLeft.Y);

        actions.Add(new ShiftLayer_Action(newLayerGuids, delta, document!.AnimationHandler.ActiveFrameBindable));

        internals!.ActionAccumulator.AddFinishedActions(actions.ToArray());

        actions.Clear();

        actions.Add(new ClearSoftSelectedMembers_PassthroughAction());
        foreach (var newGuid in newGuidsOfOriginal)
        {
            actions.Add(new AddSoftSelectedMember_PassthroughAction(newGuid));
        }

        actions.Add(new SetSelectedMember_PassthroughAction(newGuidsOfOriginal.Last()));

        internals!.ActionAccumulator.AddFinishedActions(actions.ToArray());


        internals.ActionAccumulator.EndChangeBlock();

        tool!.DuplicateOnMove = false;
    }

    public void OnLineOverlayMoved(VecD start, VecD end) { }

    public void OnSelectedObjectNudged(VecI distance) => document!.TransformHandler.Nudge(distance);
    public bool IsTransformingMember(Guid id)
    {
        if (document!.SelectedStructureMember is null)
            return false;

        return selectedMembers.Contains(id) && IsTransforming;
    }

    public void OnMidChangeUndo() => document!.TransformHandler.Undo();

    public void OnMidChangeRedo() => document!.TransformHandler.Redo();
    public bool CanUndo => document!.TransformHandler.HasUndo;
    public bool CanRedo => document!.TransformHandler.HasRedo;

    public void OnTransformApplied()
    {
        if (tool is not null)
        {
            tool.TransformingSelectedArea = false;
        }

        internals!.ActionAccumulator.AddActions(new EndPreviewShiftLayers_Action());
        internals!.ActionAccumulator.AddActions(new EndTransformSelected_Action());
        internals!.ActionAccumulator.AddFinishedActions();
        document!.TransformHandler.HideTransform();
        RestoreSnapping();
        onEnded!.Invoke(this);

        if (Type == ExecutorType.ToolLinked)
        {
            GetHandler<IToolsHandler>().RestorePreviousTool();
        }

        isInProgress = false;
        document.TransformHandler.PassthroughPointerPressed -= OnLeftMouseButtonDown;
    }

    public override void ForceStop()
    {
        if (tool is not null)
        {
            tool.TransformingSelectedArea = false;
        }

        internals!.ActionAccumulator.AddActions(new EndPreviewShiftLayers_Action());
        internals!.ActionAccumulator.AddActions(new EndTransformSelected_Action());
        internals!.ActionAccumulator.AddFinishedActions();
        document!.TransformHandler.HideTransform();
        RestoreSnapping();

        isInProgress = false;
        document.TransformHandler.PassthroughPointerPressed -= OnLeftMouseButtonDown;
    }

    private void DuplicateIfRequired()
    {
        if (duplicateOnStop)
        {
            DuplicateSelected();
            duplicateOnStop = false;
        }
    }

    private void AddSnappingForMembers(List<Guid> memberGuids)
    {
        foreach (Guid memberGuid in memberGuids)
        {
            IStructureMemberHandler? member = document!.StructureHelper.Find(memberGuid);
            if (member is null)
            {
                continue;
            }

            if (member is ILayerHandler layer && layer.IsVisibleStructurally)
            {
                document!.SnappingHandler.AddFromBounds(layer.Id.ToString(), () => layer.TightBounds ?? RectD.Empty);
            }
        }
    }

    private void RestoreSnapping()
    {
        foreach (var id in disabledSnappingMembers)
        {
            var member = document!.StructureHelper.Find(id);
            if (member is null || !member.IsVisibleStructurally)
            {
                continue;
            }

            document!.SnappingHandler.AddFromBounds(id.ToString(), () => member?.TightBounds ?? RectD.Empty);
        }
    }

    public bool IsFeatureEnabled<T>()
    {
        Type feature = typeof(T);
        return feature == typeof(ITransformableExecutor) && IsTransforming ||
               feature == typeof(IMidChangeUndoableExecutor) ||
               feature == typeof(ITransformStoppedEvent) || feature == typeof(ITransformDraggedEvent);
    }
}
