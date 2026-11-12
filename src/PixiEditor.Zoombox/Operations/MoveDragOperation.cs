using Avalonia.Input;
using Drawie.Numerics;

namespace PixiEditor.Zoombox.Operations;

internal class MoveDragOperation : IDragOperation
{
    private Zoombox parent;
    private VecD prevMousePos;
    private IPointer? capturedPointer = null!;

    public MoveDragOperation(Zoombox zoomBox)
    {
        parent = zoomBox;
    }

    public void Start(PointerEventArgs e)
    {
        prevMousePos = Zoombox.ToVecD(e.GetPosition(parent));
        e.Pointer.Capture(parent);
        capturedPointer = e.Pointer;
    }

    public void Update(PointerEventArgs e)
    {
        var curMousePos = Zoombox.ToVecD(e.GetPosition(parent));
        var delta = parent.ToZoomboxSpace(prevMousePos) - parent.ToZoomboxSpace(curMousePos);
        parent.Center += delta;
        parent.Pan += prevMousePos - curMousePos;
        prevMousePos = curMousePos;
    }

    public void Terminate()
    {
        capturedPointer?.Capture(null);
        capturedPointer = null!;
    }
}
