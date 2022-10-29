using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class SelectToolExecutor : UpdateableChangeExecutor
{
    private SelectToolViewModel? toolViewModel;
    private Toolbar? toolbar;
    private VecI startPos;
    private SelectionShape selectShape;
    private SelectionMode selectMode;

    public override ExecutionState Start()
    {
        toolViewModel = ViewModelMain.Current?.ToolsSubViewModel.GetTool<SelectToolViewModel>();
        toolbar = toolViewModel?.Toolbar;

        if (toolViewModel is null || toolbar is null)
            return ExecutionState.Error;
        
        startPos = controller!.LastPixelPosition;
        selectShape = toolViewModel.SelectShape;
        selectMode = toolViewModel.SelectMode;

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

    public override void OnPixelPositionChange(VecI pos)
    {
        IAction action = CreateUpdateAction(selectShape, RectI.FromTwoPixels(startPos, pos), selectMode);
        internals!.ActionAccumulator.AddActions(action);
    }

    public override void OnLeftMouseButtonUp()
    {
        IAction action = CreateEndAction(selectShape);
        internals!.ActionAccumulator.AddFinishedActions(action);
        onEnded!(this);
    }

    public override void ForceStop()
    {
        OnLeftMouseButtonUp();
    }
}
