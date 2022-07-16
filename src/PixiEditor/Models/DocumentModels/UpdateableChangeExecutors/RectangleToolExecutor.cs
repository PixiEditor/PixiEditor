using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class RectangleToolExecutor : UpdateableChangeExecutor
{
    private int strokeWidth;
    private SKColor fillColor;
    private SKColor strokeColor;
    private Guid memberGuid;
    private bool drawOnMask;

    private bool transforming = false;
    private RectangleToolViewModel? rectangleTool;
    private VecI startPos;
    private RectI lastRect;

    public override ExecutionState Start()
    {
        ColorsViewModel? colorsVM = ViewModelMain.Current?.ColorsSubViewModel;
        rectangleTool = (RectangleToolViewModel?)(ViewModelMain.Current?.ToolsSubViewModel.GetTool<RectangleToolViewModel>());
        BasicShapeToolbar? toolbar = (BasicShapeToolbar?)rectangleTool?.Toolbar;
        StructureMemberViewModel? member = document?.SelectedStructureMember;
        if (colorsVM is null || toolbar is null || member is null || rectangleTool is null)
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
        DrawRectangle(startPos);
        return ExecutionState.Success;
    }

    private void DrawRectangle(VecI curPos)
    {
        RectI rect = RectI.FromTwoPoints(startPos, curPos);
        if (rect.Width == 0)
            rect.Width = 1;
        if (rect.Height == 0)
            rect.Height = 1;
        
        lastRect = rect;

        helpers!.ActionAccumulator.AddActions(new DrawRectangle_Action(memberGuid, new ShapeData(rect.Center, rect.Size, 0, strokeWidth, strokeColor, fillColor), drawOnMask));
    }

    public override void OnTransformMoved(ShapeCorners corners)
    {
        if (!transforming)
            return;

        var rect = (RectI)RectD.FromCenterAndSize(corners.RectCenter, corners.RectSize);
        
        helpers!.ActionAccumulator.AddActions(
            new DrawRectangle_Action(memberGuid, new ShapeData(rect.Center, rect.Size, corners.RectRotation, strokeWidth, strokeColor, fillColor), drawOnMask));
    }

    public override void OnTransformApplied()
    {
        helpers!.ActionAccumulator.AddFinishedActions(new EndDrawRectangle_Action());
        document!.TransformViewModel.HideTransform();
        onEnded?.Invoke(this);
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        if (transforming)
            return;
        DrawRectangle(pos);
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
        helpers!.ActionAccumulator.AddFinishedActions(new EndDrawRectangle_Action());
    }
}
