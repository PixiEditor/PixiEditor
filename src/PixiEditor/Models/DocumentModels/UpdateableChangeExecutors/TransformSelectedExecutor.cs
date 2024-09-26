using System.Collections.Generic;
using System.Linq;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces.Vector;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;
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
            .Where(static m => m is ILayerHandler).ToList();

        if (!members.Any())
            return ExecutionState.Error;

        return SelectMembers(members);
    }

    private ExecutionState SelectMembers(List<IStructureMemberHandler> members)
    {
        bool allRaster = true;
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

            memberCorners.Add(member.Id, targetCorners);
        }

        ShapeCorners masterCorners = memberCorners.Count == 1
            ? memberCorners.FirstOrDefault().Value
            : new ShapeCorners(memberCorners.Values.Select(static c => c.AABBBounds).Aggregate((a, b) => a.Union(b)));

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
        
        document.TransformHandler.ShowTransform(mode, true, masterCorners, Type == ExecutorType.Regular);
        internals!.ActionAccumulator.AddActions(
            new TransformSelected_Action(masterCorners, tool.KeepOriginalImage, memberCorners, false,
                document.AnimationHandler.ActiveFrameBindable));

        isInProgress = true;
        return ExecutionState.Success;
    }

    public override void OnMembersSelected(List<Guid> memberGuids)
    {
        if (isInProgress)
        {
            internals.ActionAccumulator.AddActions(new EndTransformSelected_Action());
            document!.TransformHandler.HideTransform();
            AddSnappingForMembers(memberGuids);
            
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
        if (!isInProgress)
            return;

        internals!.ActionAccumulator.AddActions(
            new TransformSelected_Action(corners, tool!.KeepOriginalImage, memberCorners, false,
                document!.AnimationHandler.ActiveFrameBindable));
    }

    public void OnLineOverlayMoved(VecD start, VecD end) { }

    public void OnSelectedObjectNudged(VecI distance) => document!.TransformHandler.Nudge(distance);

    public void OnMidChangeUndo() => document!.TransformHandler.Undo();

    public void OnMidChangeRedo() => document!.TransformHandler.Redo();

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
