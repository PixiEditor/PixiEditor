using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Tools;
using Drawie.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class PasteImageExecutor : UpdateableChangeExecutor, ITransformableExecutor, IMidChangeUndoableExecutor
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
            drawOnMask = member is not ILayerHandler layer || layer.ShouldDrawOnMask;
            
            switch (drawOnMask)
            {
                case true when !member.HasMaskBindable:
                case false when member is not ILayerHandler:
                    return ExecutionState.Error;
            }
            
            memberGuid = member.Id;
        }

        ShapeCorners corners = new(new RectD(pos, image.Size));
        internals!.ActionAccumulator.AddActions(
            new ClearSelection_Action(),
            new PasteImage_Action(image, corners, memberGuid.Value, false, drawOnMask, document.AnimationHandler.ActiveFrameBindable, default));
        document.TransformHandler.ShowTransform(DocumentTransformMode.Scale_Rotate_Shear_Perspective, true, corners, true);

        return ExecutionState.Success;
    }

    public bool IsTransforming => true; 

    public void OnTransformChanged(ShapeCorners corners)
    {
        internals!.ActionAccumulator.AddActions(new PasteImage_Action(image, corners, memberGuid.Value, false, drawOnMask, document!.AnimationHandler.ActiveFrameBindable, default));
    }

    public void OnLineOverlayMoved(VecD start, VecD end) { }

    public void OnSelectedObjectNudged(VecI distance) => document!.TransformHandler.Nudge(distance);

    public void OnMidChangeUndo() => document!.TransformHandler.Undo();

    public void OnMidChangeRedo() => document!.TransformHandler.Redo();
    public bool CanUndo => document!.TransformHandler.HasUndo;
    public bool CanRedo => document!.TransformHandler.HasRedo;

    public void OnTransformApplied()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndPasteImage_Action());
        document!.TransformHandler.HideTransform();
        onEnded!.Invoke(this);
    }

    public override void ForceStop()
    {
        document!.TransformHandler.HideTransform();
        internals!.ActionAccumulator.AddActions(new EndPasteImage_Action());
    }

    public bool IsFeatureEnabled<T>()
    {
        Type featureType = typeof(T);
        if (featureType == typeof(ITransformableExecutor))
            return IsTransforming;
        
        if (featureType == typeof(IMidChangeUndoableExecutor))
            return true;

        return false;
    }
}
