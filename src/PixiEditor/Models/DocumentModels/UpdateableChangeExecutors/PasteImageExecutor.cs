using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PasteImageExecutor : UpdateableChangeExecutor
{
    private readonly Surface image;
    private readonly VecI pos;
    private bool drawOnMask;
    private Guid? memberGuid;

    public PasteImageExecutor(Surface image, VecI pos)
    {
        this.image = image;
        this.pos = pos;
    }

    public PasteImageExecutor(Surface image, VecI pos, Guid memberGuid, bool drawOnMask)
    {
        this.image = image;
        this.pos = pos;
        this.memberGuid = memberGuid;
        this.drawOnMask = drawOnMask;
    }
    
    public override ExecutionState Start()
    {
        if (memberGuid == null)
        {
            var member = document!.SelectedStructureMember;

            if (member is null)
                return ExecutionState.Error;
            drawOnMask = member is not LayerViewModel layer || layer.ShouldDrawOnMask;
            
            switch (drawOnMask)
            {
                case true when !member.HasMaskBindable:
                case false when member is not LayerViewModel:
                    return ExecutionState.Error;
            }
            
            memberGuid = member.GuidValue;
        }

        ShapeCorners corners = new(new RectD(pos, image.Size));
        internals!.ActionAccumulator.AddActions(new PasteImage_Action(image, corners, memberGuid.Value, false, drawOnMask));
        document.TransformViewModel.ShowTransform(DocumentTransformMode.Scale_Rotate_Shear_Perspective, true, corners, true);

        return ExecutionState.Success;
    }

    public override void OnTransformMoved(ShapeCorners corners)
    {
        internals!.ActionAccumulator.AddActions(new PasteImage_Action(image, corners, memberGuid.Value, false, drawOnMask));
    }

    public override void OnSelectedObjectNudged(VecI distance) => document!.TransformViewModel.Nudge(distance);

    public override void OnMidChangeUndo() => document!.TransformViewModel.Undo();

    public override void OnMidChangeRedo() => document!.TransformViewModel.Redo();

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
