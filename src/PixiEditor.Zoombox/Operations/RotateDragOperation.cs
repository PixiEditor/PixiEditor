using System.Windows;
using System.Windows.Input;
using Avalonia.Input;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using Point = Avalonia.Point;

namespace PixiEditor.Zoombox.Operations;

internal class RotateDragOperation : IDragOperation
{
    private Zoombox owner;
    private double initialZoomboxAngle;
    private double initialClickAngle;
    private LockingRotationProcess? rotationProcess;
    private IPointer? capturedPointer = null!;

    public RotateDragOperation(Zoombox zoomBox)
    {
        owner = zoomBox;
    }

    public void Start(PointerEventArgs e)
    {
        Point pointCur = e.GetPosition(owner);
        initialClickAngle = GetAngle(new(pointCur.X, pointCur.Y));
        initialZoomboxAngle = owner.AngleRadians;
        rotationProcess = new LockingRotationProcess(initialZoomboxAngle);
        e.Pointer.Capture(owner);
        capturedPointer = e.Pointer;
    }

    private double GetAngle(VecD point)
    {
        VecD center = new(owner.Bounds.Width / 2, owner.Bounds.Height / 2);
        double angle = (point - center).Angle;
        if (double.IsNaN(angle) || double.IsInfinity(angle))
            return 0;
        return angle;
    }

    public void Update(PointerEventArgs e)
    {
        Point pointCur = e.GetPosition(owner);
        double clickAngle = GetAngle(new(pointCur.X, pointCur.Y));
        double newZoomboxAngle = initialZoomboxAngle;
        if (owner.FlipX ^ owner.FlipY)
            newZoomboxAngle += initialClickAngle - clickAngle;
        else
            newZoomboxAngle += clickAngle - initialClickAngle;
        owner.AngleRadians = rotationProcess!.UpdateRotation(newZoomboxAngle);
    }

    public void Terminate()
    {
        capturedPointer?.Capture(null);
        capturedPointer = null!;
    }
}
