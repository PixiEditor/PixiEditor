using System;
using System.Windows;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.Zoombox.Operations;

internal class ZoomDragOperation : IDragOperation
{
    private readonly Zoombox parent;

    private VecD scaleOrigin;
    private VecD screenScaleOrigin;
    private double originalScale;

    public ZoomDragOperation(Zoombox zoomBox)
    {
        parent = zoomBox;
    }

    public void Start(MouseButtonEventArgs e)
    {
        screenScaleOrigin = Zoombox.ToVecD(e.GetPosition(parent.mainCanvas));
        scaleOrigin = parent.ToZoomboxSpace(screenScaleOrigin);
        originalScale = parent.Scale;
        parent.mainGrid.CaptureMouse();
    }

    public void Update(MouseEventArgs e)
    {
        Point curScreenPos = e.GetPosition(parent.mainCanvas);
        double deltaX = curScreenPos.X - screenScaleOrigin.X;
        double deltaPower = deltaX / 10.0;

        parent.Scale = originalScale * Math.Pow(Zoombox.ScaleFactor, deltaPower);

        VecD shiftedOrigin = parent.ToZoomboxSpace(screenScaleOrigin);
        parent.Center += scaleOrigin - shiftedOrigin;
    }

    public void Terminate()
    {
        parent.mainGrid.ReleaseMouseCapture();
    }
}
