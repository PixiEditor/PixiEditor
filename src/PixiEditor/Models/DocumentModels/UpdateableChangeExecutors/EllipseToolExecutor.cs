using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
#nullable enable
internal class EllipseToolExecutor : UpdateableChangeExecutor
{
    private int strokeWidth;
    private SKColor fillColor;
    private SKColor strokeColor;
    private Guid memberGuid;
    private bool drawOnMask;

    private bool transforming = false;
    private EllipseToolViewModel? ellipseTool;
    private VecI startPos;
    private RectI lastRect;

    public override OneOf<Success, Error> Start()
    {
        ColorsViewModel? colorsVM = ViewModelMain.Current?.ColorsSubViewModel;
        ellipseTool = (EllipseToolViewModel?)(ViewModelMain.Current?.ToolsSubViewModel.GetTool<EllipseToolViewModel>());
        BasicShapeToolbar? toolbar = (BasicShapeToolbar?)ellipseTool?.Toolbar;
        ViewModels.SubViewModels.Document.StructureMemberViewModel? member = document?.SelectedStructureMember;
        if (colorsVM is null || toolbar is null || member is null || ellipseTool is null)
            return new Error();

        fillColor = toolbar.Fill ? toolbar.FillColor.ToSKColor() : SKColors.Transparent;
        startPos = controller!.LastPixelPosition;
        strokeColor = colorsVM.PrimaryColor;
        strokeWidth = toolbar.ToolSize;
        memberGuid = member.GuidValue;
        drawOnMask = member.ShouldDrawOnMask;

        DrawEllipseOrCircle(startPos);
        return new Success();
    }

    private void DrawEllipseOrCircle(VecI curPos)
    {
        RectI rect = RectI.FromTwoPoints(startPos, curPos);
        if (rect.Width == 0)
            rect.Width = 1;
        if (rect.Height == 0)
            rect.Height = 1;

        if (ellipseTool!.DrawCircle)
            rect.Width = rect.Height = Math.Min(rect.Width, rect.Height);
        lastRect = rect;

        helpers!.ActionAccumulator.AddActions(new DrawEllipse_Action(memberGuid, rect, strokeColor, fillColor, strokeWidth, drawOnMask));
    }

    public override void OnTransformMoved(ShapeCorners corners)
    {
        if (!transforming)
            return;

        helpers!.ActionAccumulator.AddActions(
            new DrawEllipse_Action(memberGuid, (RectI)RectD.FromCenterAndSize(corners.RectCenter, corners.RectSize), strokeColor, fillColor, strokeWidth, drawOnMask));
    }

    public override void OnTransformApplied()
    {
        helpers!.ActionAccumulator.AddFinishedActions(new EndDrawEllipse_Action());
        document!.TransformViewModel.HideTransform();
        onEnded?.Invoke(this);
    }

    public override void OnPixelPositionChange(VecI pos)
    {
        if (transforming)
            return;
        DrawEllipseOrCircle(pos);
    }

    public override void OnLeftMouseButtonUp()
    {
        if (transforming)
            return;
        transforming = true;
        document!.TransformViewModel.ShowFixedAngleShapeTransform(new ShapeCorners(lastRect));
    }

    public override void OnKeyDown(Key key)
    {
        if (key is not Key.Enter || !transforming)
            return;
        OnTransformApplied();
    }

    public override void ForceStop()
    {
        if (transforming)
            document!.TransformViewModel.HideTransform();
        helpers!.ActionAccumulator.AddFinishedActions(new EndDrawEllipse_Action());
    }
}
