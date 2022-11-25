using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
internal class TransformReferenceLayerExecutor : UpdateableChangeExecutor
{
    public override ExecutionState Start()
    {
        if (document!.ReferenceLayerViewModel.ReferenceBitmap is null)
            return ExecutionState.Error;

        ShapeCorners corners = document.ReferenceLayerViewModel.ReferenceShapeBindable;
        document.TransformViewModel.ShowTransform(DocumentTransformMode.Scale_Rotate_Shear_NoPerspective, true, corners);
        internals!.ActionAccumulator.AddActions(new TransformReferenceLayer_Action(corners));
        return ExecutionState.Success;
    }

    public override void OnTransformMoved(ShapeCorners corners)
    {
        internals!.ActionAccumulator.AddActions(new TransformReferenceLayer_Action(corners));
    }

    public override void OnTransformApplied()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndTransformReferenceLayer_Action());
        document!.TransformViewModel.HideTransform();
        onEnded!.Invoke(this);
    }

    public override void ForceStop()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndTransformReferenceLayer_Action());
        document!.TransformViewModel.HideTransform();
    }
}
