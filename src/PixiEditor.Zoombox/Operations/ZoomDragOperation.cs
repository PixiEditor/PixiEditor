using System;
using System.Windows;
using System.Windows.Input;
using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using Point = Avalonia.Point;

namespace PixiEditor.Zoombox.Operations;

internal class ZoomDragOperation : IDragOperation
{
    private readonly Zoombox parent;

    private VecD scaleOrigin;
    private VecD screenScaleOrigin;
    private double originalScale;
    private IPointer? capturedPointer = null!;

    public ZoomDragOperation(Zoombox zoomBox)
    {
        parent = zoomBox;
    }

    public void Start(PointerEventArgs e)
    {
        screenScaleOrigin = Zoombox.ToVecD(e.GetPosition(parent));
        scaleOrigin = parent.ToZoomboxSpace(screenScaleOrigin);
        originalScale = parent.Scale;
        capturedPointer = e.Pointer;
        e.Pointer.Capture(parent);
    }

    public void Update(PointerEventArgs e)
    {
        Point curScreenPos = e.GetPosition(parent);
        double deltaX = curScreenPos.X - screenScaleOrigin.X;
        double deltaPower = deltaX / 10.0;

        parent.Scale = Math.Clamp(originalScale * Math.Pow(Zoombox.ScaleFactor, deltaPower), parent.MinScale, Zoombox.MaxScale);

        VecD shiftedOrigin = parent.ToZoomboxSpace(screenScaleOrigin);
        parent.Center += scaleOrigin - shiftedOrigin;
    }

    public void Terminate()
    {
        capturedPointer?.Capture(null);
        capturedPointer = null!;
    }
}
