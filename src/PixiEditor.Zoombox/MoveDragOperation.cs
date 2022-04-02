using ChunkyImageLib.DataHolders;
using System.Windows.Input;

namespace PixiEditor.Zoombox
{
    internal class MoveDragOperation : IDragOperation
    {
        private Zoombox parent;
        private Vector2d prevMousePos;

        public MoveDragOperation(Zoombox zoomBox)
        {
            parent = zoomBox;
        }
        public void Start(MouseButtonEventArgs e)
        {
            prevMousePos = Zoombox.ToVector2d(e.GetPosition(parent.mainCanvas));
            parent.mainCanvas.CaptureMouse();
        }

        public void Update(MouseEventArgs e)
        {
            var curMousePos = Zoombox.ToVector2d(e.GetPosition(parent.mainCanvas));
            parent.Center += parent.ToZoomboxSpace(prevMousePos) - parent.ToZoomboxSpace(curMousePos);
            prevMousePos = curMousePos;
        }

        public void Terminate()
        {
            parent.mainCanvas.ReleaseMouseCapture();
        }
    }
}
