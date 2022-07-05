using System;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;

namespace PixiEditor.Zoombox.Operations;

internal class ZoomDragOperation : IDragOperation
{
    private readonly Zoombox parent;

    private VecD scaleOrigin;
    private VecD screenScaleOrigin;

    public ZoomDragOperation(Zoombox zoomBox)
    {
        parent = zoomBox;
    }

    public void Start(MouseButtonEventArgs e)
    {
        screenScaleOrigin = parent.ToZoomboxSpace(Zoombox.ToVecD(e.GetPosition(parent.mainCanvas)));
        scaleOrigin = parent.ToZoomboxSpace(screenScaleOrigin);
        parent.mainCanvas.CaptureMouse();
    }

    public void Update(MouseEventArgs e)
    {
        var curScreenPos = e.GetPosition(parent.mainCanvas);
        double deltaX = screenScaleOrigin.X - curScreenPos.X;
        double deltaPower = deltaX / 10.0;

        parent.Scale *= Math.Pow(Zoombox.ScaleFactor, deltaPower);

        var shiftedOrigin = parent.ToZoomboxSpace(screenScaleOrigin);
        parent.Center += scaleOrigin - shiftedOrigin;
    }

    public void Terminate()
    {
        parent.mainCanvas.ReleaseMouseCapture();
    }
}
