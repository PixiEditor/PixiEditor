using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PasteImageExecutor : UpdateableChangeExecutor
{
    private readonly Surface image;
    private readonly VecI pos;
    private bool drawOnMask;
    private Guid memberGuid;

    public PasteImageExecutor(Surface image, VecI pos)
    {
        this.image = image;
        this.pos = pos;
    }

    public override ExecutionState Start()
    {
        var member = document!.SelectedStructureMember;

        if (member is null)
            return ExecutionState.Error;
        drawOnMask = member is LayerViewModel layer ? layer.ShouldDrawOnMask : true;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not LayerViewModel)
            return ExecutionState.Error;

        memberGuid = member.GuidValue;

        ShapeCorners corners = new(new RectD(pos, image.Size));
        internals!.ActionAccumulator.AddActions(new PasteImage_Action(image, corners, memberGuid, false, drawOnMask));
        document.TransformViewModel.ShowTransform(DocumentTransformMode.Freeform, true, corners);

        return ExecutionState.Success;
    }

    public override void OnTransformMoved(ShapeCorners corners)
    {
        internals!.ActionAccumulator.AddActions(new PasteImage_Action(image, corners, memberGuid, false, drawOnMask));
    }

    public override void OnTransformApplied()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndPasteImage_Action());
        document!.TransformViewModel.HideTransform();
        onEnded!.Invoke(this);
    }

    public override void ForceStop()
    {
        document!.TransformViewModel.HideTransform();
        internals!.ActionAccumulator.AddActions(new EndPasteImage_Action());
    }
}
