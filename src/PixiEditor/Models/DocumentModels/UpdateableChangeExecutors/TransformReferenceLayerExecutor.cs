using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
internal class TransformReferenceLayerExecutor : UpdateableChangeExecutor
{
    public override ExecutionState Start()
    {
        if (document!.ReferenceLayerHandler.ReferenceBitmap is null)
            return ExecutionState.Error;

        ShapeCorners corners = document.ReferenceLayerHandler.ReferenceShapeBindable;
        document.TransformHandler.ShowTransform(DocumentTransformMode.Scale_Rotate_Shear_NoPerspective, true, corners, true);
        document.ReferenceLayerHandler.IsTransforming = true;
        internals!.ActionAccumulator.AddActions(new TransformReferenceLayer_Action(corners));
        return ExecutionState.Success;
    }

    public override void OnTransformMoved(ShapeCorners corners)
    {
        internals!.ActionAccumulator.AddActions(new TransformReferenceLayer_Action(corners));
    }

    public override void OnSelectedObjectNudged(VecI distance) => document!.TransformHandler.Nudge(distance);

    public override void OnMidChangeUndo() => document!.TransformHandler.Undo();

    public override void OnMidChangeRedo() => document!.TransformHandler.Redo();

    public override void OnTransformApplied()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndTransformReferenceLayer_Action());
        document!.TransformHandler.HideTransform();
        document.ReferenceLayerHandler.IsTransforming = false;
        onEnded!.Invoke(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndTransformReferenceLayer_Action());
        document!.TransformHandler.HideTransform();
        document.ReferenceLayerHandler.IsTransforming = false;
    }
}
