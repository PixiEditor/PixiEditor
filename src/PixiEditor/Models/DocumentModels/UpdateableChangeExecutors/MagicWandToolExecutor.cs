using System.Collections.Generic;
using System.Linq;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

internal class MagicWandToolExecutor : UpdateableChangeExecutor
{
    private bool considerRenderOutput;
    private bool drawOnMask;
    private List<Guid> memberGuids;
    private SelectionMode mode;
    private float tolerance;
    private bool contiguous;
    private string renderOutput;

    public override ExecutionState Start()
    {
        var magicWand = GetHandler<IMagicWandToolHandler>();
        renderOutput = GetHandler<WindowViewModel>().LastActiveViewport?.RenderOutputName ?? string.Empty;
        var members = document!.ExtractSelectedLayers(true).ToList();

        if (magicWand is null || members.Count == 0)
            return ExecutionState.Error;

        mode = magicWand.ResultingSelectionMode;
        memberGuids = members;
        considerRenderOutput = magicWand.DocumentScope == DocumentScope.Canvas;
        if (considerRenderOutput)
            memberGuids = null;

        var pos = controller!.LastPixelPosition;
        tolerance = (float)magicWand.Tolerance;
        contiguous = magicWand.Contiguous;

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
        var action = new MagicWand_Action(memberGuids, pos, mode, tolerance, document!.AnimationHandler.ActiveFrameBindable, contiguous, renderOutput);
        internals!.ActionAccumulator.AddActions(action);
    }
    private void AddFinishAction()
    {
        internals!.ActionAccumulator.AddFinishedActions(new EndMagicWand_Action());
    }
}
