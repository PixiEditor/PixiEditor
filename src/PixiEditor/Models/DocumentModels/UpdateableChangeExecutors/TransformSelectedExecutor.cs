﻿using System.Collections.Generic;
using System.Linq;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces.Vector;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class TransformSelectedExecutor : UpdateableChangeExecutor
{
    private Dictionary<Guid, ShapeCorners> memberCorners = new();
    private IMoveToolHandler? tool;
    private bool isInProgress;
    public override ExecutorType Type { get; }

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

    public override void OnTransformMoved(ShapeCorners corners)
    {
        if (!isInProgress)
            return;

        internals!.ActionAccumulator.AddActions(
            new TransformSelected_Action(corners, tool!.KeepOriginalImage, memberCorners, false,
                document!.AnimationHandler.ActiveFrameBindable));
    }

    public override void OnSelectedObjectNudged(VecI distance) => document!.TransformHandler.Nudge(distance);

    public override void OnMidChangeUndo() => document!.TransformHandler.Undo();

    public override void OnMidChangeRedo() => document!.TransformHandler.Redo();

    public override void OnTransformApplied()
    {
        if (tool is not null)
        {
            tool.TransformingSelectedArea = false;
        }

        internals!.ActionAccumulator.AddActions(new EndTransformSelected_Action());
        internals!.ActionAccumulator.AddFinishedActions();
        document!.TransformHandler.HideTransform();
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

        isInProgress = false;
    }
}