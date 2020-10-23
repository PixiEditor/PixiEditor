using System;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.Controllers
{
    public class MouseMovementEventArgs : EventArgs
    {
        public MouseMovementEventArgs(Coordinates mousePosition)
        {
            NewPosition = mousePosition;
        }

        public Coordinates NewPosition { get; set; }
    }
}