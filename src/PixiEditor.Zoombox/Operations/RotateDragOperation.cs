using System.Windows;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;

namespace PixiEditor.Zoombox.Operations;

internal class RotateDragOperation : IDragOperation
{
    private Zoombox owner;
    private double initialZoomboxAngle;
    private double initialClickAngle;
    private LockingRotationProcess? rotationProcess;

    public RotateDragOperation(Zoombox zoomBox)
    {
        owner = zoomBox;
    }

    public void Start(MouseButtonEventArgs e)
    {
        Point pointCur = e.GetPosition(owner.mainCanvas);
        initialClickAngle = GetAngle(new(pointCur.X, pointCur.Y));
        initialZoomboxAngle = owner.Angle;
        rotationProcess = new LockingRotationProcess(initialZoomboxAngle);
        owner.mainCanvas.CaptureMouse();
    }

    private double GetAngle(VecD point)
    {
        VecD center = new(owner.mainCanvas.ActualWidth / 2, owner.mainCanvas.ActualHeight / 2);
        double angle = (point - center).Angle;
        if (double.IsNaN(angle) || double.IsInfinity(angle))
            return 0;
        return angle;
    }

    public void Update(MouseEventArgs e)
    {
        Point pointCur = e.GetPosition(owner.mainCanvas);
        double clickAngle = GetAngle(new(pointCur.X, pointCur.Y));
        double newZoomboxAngle = initialZoomboxAngle;
        if (owner.FlipX ^ owner.FlipY)
            newZoomboxAngle += initialClickAngle - clickAngle;
        else
            newZoomboxAngle += clickAngle - initialClickAngle;
        owner.Angle = rotationProcess!.UpdateRotation(newZoomboxAngle);
    }

    public void Terminate()
    {
        owner.mainCanvas.ReleaseMouseCapture();
    }
}
