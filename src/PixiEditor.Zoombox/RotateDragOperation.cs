using ChunkyImageLib.DataHolders;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Zoombox
{
    internal class RotateDragOperation : IDragOperation
    {
        private Zoombox parent;
        private double prevAngle;


        public RotateDragOperation(Zoombox zoomBox)
        {
            parent = zoomBox;
        }
        public void Start(MouseButtonEventArgs e)
        {
            Point pointCur = e.GetPosition(parent.mainCanvas);
            prevAngle = GetAngle(new(pointCur.X, pointCur.Y));

            parent.mainCanvas.CaptureMouse();
        }

        private double GetAngle(Vector2d point)
        {
            Vector2d center = new(parent.mainCanvas.ActualWidth / 2, parent.mainCanvas.ActualHeight / 2);
            double angle = (point - center).Angle;
            if (double.IsNaN(angle) || double.IsInfinity(angle))
                return 0;
            return angle;
        }

        public void Update(MouseEventArgs e)
        {
            Point pointCur = e.GetPosition(parent.mainCanvas);
            double curAngle = GetAngle(new(pointCur.X, pointCur.Y));
            double delta = curAngle - prevAngle;
            if (parent.FlipX ^ parent.FlipY)
                delta = -delta;
            prevAngle = curAngle;
            parent.Angle += delta;
        }

        public void Terminate()
        {
            parent.mainCanvas.ReleaseMouseCapture();
        }
    }
}
