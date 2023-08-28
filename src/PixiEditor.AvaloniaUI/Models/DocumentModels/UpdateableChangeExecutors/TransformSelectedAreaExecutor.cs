using System.Collections.Generic;
using System.Linq;
using ChunkyImageLib.DataHolders;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Handlers.Tools;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class TransformSelectedAreaExecutor : UpdateableChangeExecutor
{
    private Guid[]? membersToTransform;
    private IMoveToolHandler? tool;
    public override ExecutorType Type { get; }

    public TransformSelectedAreaExecutor(bool toolLinked)
    {
        Type = toolLinked ? ExecutorType.ToolLinked : ExecutorType.Regular;
    }

    public override ExecutionState Start()
    {
        tool = GetHandler<IMoveToolHandler>();
        if (tool is null || document!.SelectedStructureMember is null || document!.SelectionPathBindable.IsEmpty)
            return ExecutionState.Error;

        tool.TransformingSelectedArea = true;
        List<IStructureMemberHandler> members = new();
        
        members = document.SoftSelectedStructureMembers
            .Append(document.SelectedStructureMember)
            .Where(static m => m is ILayerHandler).ToList();
        
        if (!members.Any())
            return ExecutionState.Error;

        ShapeCorners corners = new(document.SelectionPathBindable.TightBounds);
        document.TransformHandler.ShowTransform(DocumentTransformMode.Scale_Rotate_Shear_Perspective, true, corners, Type == ExecutorType.Regular);
        membersToTransform = members.Select(static a => a.GuidValue).ToArray();
        internals!.ActionAccumulator.AddActions(
            new TransformSelectedArea_Action(membersToTransform, corners, tool.KeepOriginalImage, false));
        return ExecutionState.Success;
    }

    public override void OnTransformMoved(ShapeCorners corners)
    {
        internals!.ActionAccumulator.AddActions(
            new TransformSelectedArea_Action(membersToTransform!, corners, tool!.KeepOriginalImage, false));
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
        
        internals!.ActionAccumulator.AddActions(new EndTransformSelectedArea_Action());
        internals!.ActionAccumulator.AddFinishedActions();
        document!.TransformHandler.HideTransform();
        onEnded!.Invoke(this);

        if (Type == ExecutorType.ToolLinked)
        {
            GetHandler<IToolsHandler>().RestorePreviousTool();
        }
    }

    public override void ForceStop()
    {
        if (tool is not null)
        {
            tool.TransformingSelectedArea = false;
        }
        
        internals!.ActionAccumulator.AddActions(new EndTransformSelectedArea_Action());
        internals!.ActionAccumulator.AddFinishedActions();
        document!.TransformHandler.HideTransform();
    }
}
