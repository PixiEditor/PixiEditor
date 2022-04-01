using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Zoombox
{
    internal class MoveDragOperation : IDragOperation
    {
        private Zoombox parent;
        private Point prevMousePos;

        public MoveDragOperation(Zoombox zoomBox)
        {
            parent = zoomBox;
        }
        public void Start(MouseButtonEventArgs e)
        {
            prevMousePos = e.GetPosition(parent.mainCanvas);
            parent.mainCanvas.CaptureMouse();
        }

        public void Update(MouseEventArgs e)
        {
            var curMousePos = e.GetPosition(parent.mainCanvas);
            parent.SpaceOriginPos += curMousePos - prevMousePos;
            prevMousePos = curMousePos;
        }

        public void Terminate()
        {
            parent.mainCanvas.ReleaseMouseCapture();
        }
    }
}
