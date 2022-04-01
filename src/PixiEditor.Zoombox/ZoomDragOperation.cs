using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Zoombox
{
    internal class ZoomDragOperation : IDragOperation
    {
        private Zoombox parent;

        private double initZoomPower;
        private Point initSpaceOriginPos;

        private Point zoomOrigin;
        private Point screenZoomOrigin;

        public ZoomDragOperation(Zoombox zoomBox)
        {
            parent = zoomBox;
        }
        public void Start(MouseButtonEventArgs e)
        {
            screenZoomOrigin = e.GetPosition(parent.mainCanvas);
            zoomOrigin = parent.ToZoomboxSpace(screenZoomOrigin);
            initZoomPower = parent.ZoomPowerClamped;
            initSpaceOriginPos = parent.SpaceOriginPos;
            parent.mainCanvas.CaptureMouse();
        }

        public void Update(MouseEventArgs e)
        {
            var curScreenPos = e.GetPosition(parent.mainCanvas);
            double deltaX = screenZoomOrigin.X - curScreenPos.X;
            double deltaPower = deltaX / 10.0;
            parent.ZoomPowerClamped = initZoomPower - deltaPower;

            parent.SpaceOriginPos = initSpaceOriginPos;
            var shiftedOriginPos = parent.ToScreenSpace(zoomOrigin);
            var deltaOriginPos = shiftedOriginPos - screenZoomOrigin;
            parent.SpaceOriginPos = initSpaceOriginPos - deltaOriginPos;
        }

        public void Terminate()
        {
            parent.mainCanvas.ReleaseMouseCapture();
        }
    }
}
