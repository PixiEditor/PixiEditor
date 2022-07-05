using ChunkyImageLib.DataHolders;

namespace PixiEditor.Models.Controllers;

public class MouseMovementEventArgs : EventArgs
{
    public MouseMovementEventArgs(VecI mousePosition)
    {
        NewPosition = mousePosition;
    }

    public VecI NewPosition { get; set; }
}
