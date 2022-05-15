using System.Windows.Input;
using ChunkyImageLib.DataHolders;

namespace PixiEditor.Zoombox;

internal class MoveDragOperation : IDragOperation
{
    private Zoombox parent;
    private VecD prevMousePos;

    public MoveDragOperation(Zoombox zoomBox)
    {
        parent = zoomBox;
    }
    public void Start(MouseButtonEventArgs e)
    {
        prevMousePos = Zoombox.ToVecD(e.GetPosition(parent.mainCanvas));
        parent.mainCanvas.CaptureMouse();
    }

    public void Update(MouseEventArgs e)
    {
        var curMousePos = Zoombox.ToVecD(e.GetPosition(parent.mainCanvas));
        parent.Center += parent.ToZoomboxSpace(prevMousePos) - parent.ToZoomboxSpace(curMousePos);
        prevMousePos = curMousePos;
    }

    public void Terminate()
    {
        parent.mainCanvas.ReleaseMouseCapture();
    }
}
