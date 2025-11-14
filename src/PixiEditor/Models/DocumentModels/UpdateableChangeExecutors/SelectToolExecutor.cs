using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Enums;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Views.Overlays.SelectionOverlay;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class SelectToolExecutor : UpdateableChangeExecutor
{
    private ISelectToolHandler? toolViewModel;
    private IToolbar? toolbar;
    private VecI startPos;
    private SelectionShape selectShape;
    private SelectionMode selectMode;

    public override ExecutionState Start()
    {
        toolViewModel = GetHandler<ISelectToolHandler>();
        toolbar = toolViewModel?.Toolbar;

        if (toolViewModel is null || toolbar is null)
            return ExecutionState.Error;
        
        startPos = controller!.LastPixelPosition;
        selectShape = toolViewModel.SelectShape;
        selectMode = toolViewModel.ResultingSelectionMode;

        IAction action = CreateUpdateAction(selectShape, new RectI(startPos, new(0)), selectMode);
        internals!.ActionAccumulator.AddActions(action);
        
        return ExecutionState.Success;
    }

    private static IAction CreateUpdateAction(SelectionShape shape, RectI rect, SelectionMode mode) => shape switch
    {
        SelectionShape.Rectangle => new SelectRectangle_Action(rect, mode),
        SelectionShape.Circle => new SelectEllipse_Action(rect, mode),
        _ => throw new NotImplementedException(),
    };

    private static IAction CreateEndAction(SelectionShape shape) => shape switch
    {
        SelectionShape.Rectangle => new EndSelectRectangle_Action(),
        SelectionShape.Circle => new EndSelectEllipse_Action(),
        _ => throw new NotImplementedException(),
    };

    public override void OnPixelPositionChange(VecI pos, MouseOnCanvasEventArgs args)
    {
        IAction action = CreateUpdateAction(selectShape, RectI.FromTwoPixels(startPos, pos), selectMode);
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp(VecD argsPositionOnCanvas)
    {
        IAction action = CreateEndAction(selectShape);
        internals!.ActionAccumulator.AddFinishedActions(action);
        onEnded!(this);
    }

    public override void ForceStop()
    {

        IAction action = CreateEndAction(selectShape);
        internals!.ActionAccumulator.AddFinishedActions(action);
    }
}
