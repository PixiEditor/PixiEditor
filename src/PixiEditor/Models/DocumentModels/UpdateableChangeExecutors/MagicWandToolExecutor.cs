using System.Collections.Generic;
using System.Linq;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class MagicWandToolExecutor : UpdateableChangeExecutor
{
    private bool considerAllLayers;
    private bool drawOnMask;
    private List<Guid> memberGuids;
    private SelectionMode mode;
    private float tolerance;
    private FloodMode floodMode;

    public override ExecutionState Start()
    {
        var magicWand = GetHandler<IMagicWandToolHandler>();
        var members = document!.ExtractSelectedLayers(true).ToList();

        if (magicWand is null || members.Count == 0)
            return ExecutionState.Error;

        mode = magicWand.ResultingSelectionMode;
        floodMode = magicWand.FloodMode;
        memberGuids = members;
        considerAllLayers = magicWand.DocumentScope == DocumentScope.Canvas;
        if (considerAllLayers)
            memberGuids = document!.StructureHelper.GetAllLayers().Select(x => x.Id).ToList();
        var pos = controller!.LastPixelPosition;
        tolerance = (float)magicWand.Tolerance;

        AddUpdateAction(pos);

        return ExecutionState.Success;
    }

    public override void OnPixelPositionChange(VecI pos, MouseOnCanvasEventArgs args)
    {
        AddUpdateAction(pos);
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        AddFinishAction();
        onEnded!(this);
    }

    public override void ForceStop()
    {
        AddFinishAction();
    }

    private void AddUpdateAction(VecI pos)
    {
        var action = new MagicWand_Action(
            memberGuids,
            pos,
            mode,
            tolerance,
            document!.AnimationHandler.ActiveFrameBindable,
            floodMode
        );
        internals!.ActionAccumulator.AddActions(action);
    }
    private void AddFinishAction()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndMagicWand_Action());
    }
}
