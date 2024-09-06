using System.Collections.Generic;
using System.Linq;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces.Vector;
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

        memberCorners = new();
        foreach (IStructureMemberHandler member in members)
        {
            ShapeCorners targetCorners = member.TransformationCorners;

            if (member is IRasterLayerHandler && !document.SelectionPathBindable.IsEmpty)
            {
                targetCorners = new ShapeCorners(document.SelectionPathBindable.TightBounds);
            }
            
            memberCorners.Add(member.Id, targetCorners);
        }
        
        ShapeCorners masterCorners = memberCorners.Count == 1 ? memberCorners.FirstOrDefault().Value : new ShapeCorners(memberCorners.Values.Select(static c => c.AABBBounds).Aggregate((a, b) => a.Union(b)));
        
        document.TransformHandler.ShowTransform(DocumentTransformMode.Scale_Rotate_Shear_Perspective, true, masterCorners, Type == ExecutorType.Regular);
        internals!.ActionAccumulator.AddActions(
            new TransformSelected_Action(masterCorners, tool.KeepOriginalImage, memberCorners, false, document.AnimationHandler.ActiveFrameBindable));
        return ExecutionState.Success;
    }

    public override void OnTransformMoved(ShapeCorners corners)
    {
        internals!.ActionAccumulator.AddActions(
            new TransformSelected_Action(corners, tool!.KeepOriginalImage, memberCorners, false, document!.AnimationHandler.ActiveFrameBindable));
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
    }
}
