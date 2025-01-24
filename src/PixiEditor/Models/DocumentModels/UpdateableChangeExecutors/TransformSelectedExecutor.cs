using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Vector;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class TransformSelectedExecutor : UpdateableChangeExecutor, ITransformableExecutor, IMidChangeUndoableExecutor,
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
        var allLayers = document.StructureHelper.GetAllLayers();
        var topMostWithinClick = allLayers.Where(x =>
                x is { IsVisibleBindable: true, TightBounds: not null } &&
                x.TightBounds.Value.ContainsInclusive(args.PositionOnCanvas))
            .OrderByDescending(x => allLayers.IndexOf(x));

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
            internals!.ActionAccumulator.AddActions(new EndPreviewTransformSelected_Action());
            document!.TransformHandler.HideTransform();
            AddSnappingForMembers(selectedMembers);

            selectedMembers.Clear();
            memberCorners.Clear();
            isInProgress = false;
        }
    }

    public bool IsTransforming => isInProgress;

    public void OnTransformMoved(ShapeCorners corners)
    {
        DoTransform(corners);
        lastCorners = corners;
    }

    private void DoTransform(ShapeCorners corners)
    {
        if (!isInProgress)
            return;

        if (tool.DuplicateOnMove)
        {
            if (!duplicateOnStop)
            {
                cornersOnStartDuplicate = corners;
                duplicateOnStop = true;
                internals.ActionAccumulator.AddActions(new EndTransformSelected_Action());
            }

            internals.ActionAccumulator.AddActions(new PreviewTransformSelected_Action(corners, memberCorners,
                false,
                document!.AnimationHandler.ActiveFrameBindable));
            return;
        }

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

        VectorPath? original = document.SelectionPathBindable != null
            ? new VectorPath(document.SelectionPathBindable)
            : null;
        if (original != null)
        {
            var selection = document.SelectionPathBindable;
            var inverse = new VectorPath();
            inverse.AddRect(new RectD(new(0, 0), document.SizeBindable));

            actions.Add(new SetSelection_Action(inverse.Op(selection, VectorPathOp.Difference)));
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
                    actions.Add(new ClearSelectedArea_Action(newGuid, false,
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

                newLayerGuids.AddRange(newGuidsArray);
            }

            newGuidsOfOriginal.Add(newGuid);
        }

        if (original != null)
        {
            actions.Add(new SetSelection_Action(original));
        }

        internals!.ActionAccumulator.AddFinishedActions(actions.ToArray());

        actions.Clear();

        actions.Add(new ClearSoftSelectedMembers_PassthroughAction());
        foreach (var newGuid in newGuidsOfOriginal)
        {
            actions.Add(new AddSoftSelectedMember_PassthroughAction(newGuid));
        }
        
        actions.Add(new SetSelectedMember_PassthroughAction(newGuidsOfOriginal.Last()));

        internals!.ActionAccumulator.AddFinishedActions(actions.ToArray());

        actions.Clear();

        Dictionary<Guid, ShapeCorners> newMemberCorners = new();
        for (var i = 0; i < memberCorners.Count; i++)
        {
            var member = memberCorners.ElementAt(i);
            newMemberCorners.Add(newLayerGuids[i], member.Value);
        }

        actions.Add(new TransformSelected_Action(cornersOnStartDuplicate, false, newMemberCorners,
            false, document!.AnimationHandler.ActiveFrameBindable));
        actions.Add(new TransformSelected_Action(lastCorners, false, memberCorners, false,
            document!.AnimationHandler.ActiveFrameBindable));
        actions.Add(new EndTransformSelected_Action());

        internals!.ActionAccumulator.AddFinishedActions(actions.ToArray());

        internals.ActionAccumulator.EndChangeBlock();

        tool!.DuplicateOnMove = false;
    }

    public void OnLineOverlayMoved(VecD start, VecD end) { }

    public void OnSelectedObjectNudged(VecI distance) => document!.TransformHandler.Nudge(distance);

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

        internals!.ActionAccumulator.AddActions(new EndTransformSelected_Action());
        internals!.ActionAccumulator.AddActions(new EndPreviewTransformSelected_Action());
        internals!.ActionAccumulator.AddFinishedActions();
        document!.TransformHandler.HideTransform();
        AddSnappingForMembers(memberCorners.Keys.ToList());
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

        internals!.ActionAccumulator.AddActions(new EndTransformSelected_Action());
        internals!.ActionAccumulator.AddActions(new EndPreviewTransformSelected_Action());
        internals!.ActionAccumulator.AddFinishedActions();
        document!.TransformHandler.HideTransform();
        AddSnappingForMembers(memberCorners.Keys.ToList());

        isInProgress = false;
        document.TransformHandler.PassthroughPointerPressed -= OnLeftMouseButtonDown;
        DuplicateIfRequired();
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

            if (member is ILayerHandler layer)
            {
                document!.SnappingHandler.AddFromBounds(layer.Id.ToString(), () => layer.TightBounds ?? RectD.Empty);
            }
        }
    }

    public bool IsFeatureEnabled(IExecutorFeature feature)
    {
        return feature is ITransformableExecutor && IsTransforming || feature is IMidChangeUndoableExecutor ||
               feature is ITransformStoppedEvent;
    }
}
