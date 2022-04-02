using ChunkyImageLib.DataHolders;
using System;
using System.Windows.Input;

namespace PixiEditor.Zoombox
{
    internal class ZoomDragOperation : IDragOperation
    {
        private Zoombox parent;

        private double initScale;

        private Vector2d scaleOrigin;
        private Vector2d screenScaleOrigin;

        public ZoomDragOperation(Zoombox zoomBox)
        {
            parent = zoomBox;
        }
        public void Start(MouseButtonEventArgs e)
        {
            screenScaleOrigin = parent.ToZoomboxSpace(Zoombox.ToVector2d(e.GetPosition(parent.mainCanvas)));
            scaleOrigin = parent.ToZoomboxSpace(screenScaleOrigin);
            initScale = parent.Scale;
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
}
