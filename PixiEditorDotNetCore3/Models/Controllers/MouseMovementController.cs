using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace PixiEditor.Models.Controllers
{
    public class MouseMovementController
    {
        public List<Coordinates> LastMouseMoveCoordinates { get; } = new List<Coordinates>();
        public event EventHandler<MouseMovementEventArgs> MousePositionChanged;
        public bool IsRecordingChanges { get; private set; } = false;

        public void StartRecordingMouseMovementChanges()
        {
            if (IsRecordingChanges == false)
            {
                LastMouseMoveCoordinates.Clear();
                IsRecordingChanges = true;
            }
        }
        public void RecordMouseMovementChanges(Coordinates mouseCoordinates)
        {
            if (IsRecordingChanges == true)
            {
                if (LastMouseMoveCoordinates.Count == 0 || mouseCoordinates != LastMouseMoveCoordinates[LastMouseMoveCoordinates.Count - 1])
                {
                    LastMouseMoveCoordinates.Add(mouseCoordinates);
                    MousePositionChanged?.Invoke(this, new MouseMovementEventArgs(mouseCoordinates));
                }
            }
        }

        public void StopRecordingMouseMovementChanges()
        {
            if (IsRecordingChanges)
            {
                IsRecordingChanges = false;
            }
        }
    }
}

public class MouseMovementEventArgs : EventArgs
{
    public Coordinates NewPosition { get; set; }

    public MouseMovementEventArgs(Coordinates mousePosition)
    {
        NewPosition = mousePosition;
    }
}
