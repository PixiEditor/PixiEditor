using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class TransformSelectedAreaExecutor : UpdateableChangeExecutor
{
    private Guid[]? membersToTransform;
    private MoveToolViewModel? tool;

    public override ExecutorType Type { get; }

    public TransformSelectedAreaExecutor(bool toolLinked)
    {
        Type = toolLinked ? ExecutorType.ToolLinked : ExecutorType.Regular;
    }

    public override ExecutionState Start()
    {
        tool = ViewModelMain.Current?.ToolsSubViewModel.GetTool<MoveToolViewModel>();
        if (tool is null || document!.SelectedStructureMember is null || document!.SelectionPathBindable.IsEmpty)
            return ExecutionState.Error;

        var members = document.SoftSelectedStructureMembers
            .Append(document.SelectedStructureMember)
            .Where(static m => m is LayerViewModel);

        if (!members.Any())
            return ExecutionState.Error;

        ShapeCorners corners = new(document.SelectionPathBindable.TightBounds);
        document.TransformViewModel.ShowTransform(DocumentTransformMode.Scale_Rotate_Shear_Perspective, true, corners, Type == ExecutorType.Regular);
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

    public override void OnSelectedObjectNudged(VecI distance) => document!.TransformViewModel.Nudge(distance);

    public override void OnMidChangeUndo() => document!.TransformViewModel.Undo();

    public override void OnMidChangeRedo() => document!.TransformViewModel.Redo();

    public override void OnTransformApplied()
    {
        if (Type == ExecutorType.ToolLinked)
            return;

        internals!.ActionAccumulator.AddActions(new EndTransformSelectedArea_Action());
        internals!.ActionAccumulator.AddFinishedActions();
        document!.TransformViewModel.HideTransform();
        onEnded!.Invoke(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddActions(new EndTransformSelectedArea_Action());
        internals!.ActionAccumulator.AddFinishedActions();
        document!.TransformViewModel.HideTransform();
    }
}
