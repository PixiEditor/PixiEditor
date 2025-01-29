using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class VectorTextToolExecutor : UpdateableChangeExecutor
{
    private ITextToolHandler textHandler;
    private IFillableShapeToolbar toolbar;

    public override ExecutionState Start()
    {
        textHandler = GetHandler<ITextToolHandler>();
        if (textHandler == null)
        {
            return ExecutionState.Error;
        }

        toolbar = textHandler.Toolbar as IFillableShapeToolbar;
        if (toolbar == null)
        {
            return ExecutionState.Error;
        }

        var selectedMember = document.SelectedStructureMember;

        if (selectedMember is not IVectorLayerHandler layerHandler)
        {
            return ExecutionState.Error;
        }

        var shape = layerHandler.GetShapeData(document.AnimationHandler.ActiveFrameBindable);
        if (shape != null && shape is not TextVectorData textData)
        {
            return ExecutionState.Error;
        }

        internals.ActionAccumulator.AddFinishedActions(
            new SetShapeGeometry_Action(selectedMember.Id,
                new TextVectorData() { Text = "Test", Position = document.SizeBindable / 2f }),
            new EndSetShapeGeometry_Action());
        //document.TextHandler.ShowOverlay(textData.Text

        return ExecutionState.Success;
    }

    public override void ForceStop()
    {
    }
}
