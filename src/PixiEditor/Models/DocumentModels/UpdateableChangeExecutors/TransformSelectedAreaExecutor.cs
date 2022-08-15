using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class TransformSelectedAreaExecutor : UpdateableChangeExecutor
{
    private Guid[]? membersToTransform;
    private MoveToolToolbar? toolbar;

    public override ExecutorType Type { get; }

    public TransformSelectedAreaExecutor(bool toolLinked)
    {
        Type = toolLinked ? ExecutorType.ToolLinked : ExecutorType.Regular;
    }

    public override ExecutionState Start()
    {
        toolbar = (MoveToolToolbar?)(ViewModelMain.Current?.ToolsSubViewModel.GetTool<MoveToolViewModel>()?.Toolbar);
        if (toolbar is null || document!.SelectedStructureMember is null || document!.SelectionPathBindable.IsEmpty)
            return ExecutionState.Error;

        var members = document.SoftSelectedStructureMembers
            .Append(document.SelectedStructureMember)
            .Where(static m => m is LayerViewModel);

        if (!members.Any())
            return ExecutionState.Error;

        ShapeCorners corners = new(document.SelectionPathBindable.TightBounds);
        document.TransformViewModel.ShowTransform(DocumentTransformMode.Freeform, true, corners);
        membersToTransform = members.Select(static a => a.GuidValue).ToArray();
        internals!.ActionAccumulator.AddActions(
            new TransformSelectedArea_Action(membersToTransform, corners, toolbar.KeepOriginalImage, false));
        return ExecutionState.Success;
    }

    public override void OnTransformMoved(ShapeCorners corners)
    {
        internals!.ActionAccumulator.AddActions(
            new TransformSelectedArea_Action(membersToTransform!, corners, toolbar!.KeepOriginalImage, false));
    }

    public override void OnTransformApplied()
    {
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
