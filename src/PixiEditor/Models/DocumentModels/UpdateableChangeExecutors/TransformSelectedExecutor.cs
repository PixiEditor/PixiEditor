using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class TransformSelectedExecutor : UpdateableChangeExecutor, ITransformableExecutor, IMidChangeUndoableExecutor
{
    private Dictionary<Guid, ShapeCorners> memberCorners = new();
    private IMoveToolHandler? tool;
    private bool isInProgress;
    public override ExecutorType Type { get; }

    public override bool BlocksOtherActions => false;

    private List<Guid> selectedMembers = new();

    private ShapeCorners lastCorners = new();
    private bool movedOnce;

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

        members = document.SoftSelectedStructureMembers
            .Append(document.SelectedStructureMember)
            .Where(static m => m is ILayerHandler)
            .Distinct().ToList();

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
            document!.TransformHandler.HideTransform();
            AddSnappingForMembers(selectedMembers);

            selectedMembers.Clear();
            memberCorners.Clear();
            isInProgress = false;
        }

        internals.ActionAccumulator.AddActions(new InvokeAction_PassthroughAction(() =>
        {
            List<IStructureMemberHandler> members = memberGuids.Select(g => document!.StructureHelper.Find(g))
                .Where(x => x is ILayerHandler).Distinct().ToList();
            SelectMembers(members);
        }));
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
        internals!.ActionAccumulator.AddFinishedActions();
        document!.TransformHandler.HideTransform();
        AddSnappingForMembers(memberCorners.Keys.ToList());

        isInProgress = false;
        document.TransformHandler.PassthroughPointerPressed -= OnLeftMouseButtonDown;
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
        return feature is ITransformableExecutor && IsTransforming;
    }
}
