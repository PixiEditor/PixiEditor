using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;

#nullable enable

internal abstract class ShapeToolExecutor<T> : UpdateableChangeExecutor where T : ShapeTool
{
    protected int strokeWidth;
    protected SKColor fillColor;
    protected SKColor strokeColor;
    protected Guid memberGuid;
    protected bool drawOnMask;

    protected bool transforming = false;
    protected T? toolViewModel;
    protected VecI startPos;
    protected RectI lastRect;
    
    public override ExecutionState Start()
    {
        ColorsViewModel? colorsVM = ViewModelMain.Current?.ColorsSubViewModel;
        toolViewModel = ViewModelMain.Current?.ToolsSubViewModel.GetTool<T>();
        BasicShapeToolbar? toolbar = (BasicShapeToolbar?)toolViewModel?.Toolbar;
        StructureMemberViewModel? member = document?.SelectedStructureMember;
        if (colorsVM is null || toolbar is null || member is null)
            return ExecutionState.Error;
        drawOnMask = member.ShouldDrawOnMask;
        if (drawOnMask && !member.HasMaskBindable)
            return ExecutionState.Error;
        if (!drawOnMask && member is not LayerViewModel)
            return ExecutionState.Error;

        fillColor = toolbar.Fill ? toolbar.FillColor.ToSKColor() : SKColors.Transparent;
        startPos = controller!.LastPixelPosition;
        strokeColor = colorsVM.PrimaryColor;
        strokeWidth = toolbar.ToolSize;
        memberGuid = member.GuidValue;

        colorsVM.AddSwatch(strokeColor);
        DrawShape(startPos);
        return ExecutionState.Success;
    }

    protected abstract void DrawShape(VecI currentPos);
    protected abstract IAction TransformMovedAction(ShapeData data, ShapeCorners corners);
    protected abstract IAction EndDrawAction();

    public override void OnTransformMoved(ShapeCorners corners)
    {
        if (!transforming)
            return;
        
        var rect = (RectI)RectD.FromCenterAndSize(corners.RectCenter, corners.RectSize);
        ShapeData shapeData = new ShapeData(rect.Center, rect.Size, corners.RectRotation, strokeWidth, strokeColor,
            fillColor);
        IAction drawAction = TransformMovedAction(shapeData, corners);
        
        helpers!.ActionAccumulator.AddActions(drawAction);
    }

    public override void OnTransformApplied()
    {
        helpers!.ActionAccumulator.AddFinishedActions(EndDrawAction());
        document!.TransformViewModel.HideTransform();
        onEnded?.Invoke(this);
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        if (transforming)
            return;
        
        DrawShape(pos);
    }

    public override void OnLeftMouseButtonUp()
    {
        if (transforming)
            return;
        transforming = true;
        document!.TransformViewModel.ShowFixedAngleShapeTransform(new ShapeCorners(lastRect));
    }
    
    public override void ForceStop()
    {
        if (transforming)
            document!.TransformViewModel.HideTransform();
        helpers!.ActionAccumulator.AddFinishedActions(EndDrawAction());
    }
}
