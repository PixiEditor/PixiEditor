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
        public event EventHandler StartedRecordingChanges;
        public event EventHandler<MouseMovementEventArgs> MousePositionChanged;
        public event EventHandler StoppedRecordingChanges;
        public bool IsRecordingChanges { get; private set; } = false;

        public void StartRecordingMouseMovementChanges()
        {
            if (IsRecordingChanges == false)
            {
                LastMouseMoveCoordinates.Clear();
                IsRecordingChanges = true;
                StartedRecordingChanges?.Invoke(this, EventArgs.Empty);
            }
        }
        public void RecordMouseMovementChange(Coordinates mouseCoordinates)
        {
            if (IsRecordingChanges == true)
            {
                if (LastMouseMoveCoordinates.Count == 0 || mouseCoordinates != LastMouseMoveCoordinates[^1])
                {
                    LastMouseMoveCoordinates.Add(mouseCoordinates);
                    MousePositionChanged?.Invoke(this, new MouseMovementEventArgs(mouseCoordinates));
                }
            }
        }

        /// <summary>
        /// Plain mose move, does not affect mouse drag recordings
        /// </summary>
        /// <param name="mouseCoordinates"></param>
        public void MouseMoved(Coordinates mouseCoordinates)
        {
            MousePositionChanged?.Invoke(this, new MouseMovementEventArgs(mouseCoordinates));
        }

        public void StopRecordingMouseMovementChanges()
        {
            if (IsRecordingChanges)
            {
                IsRecordingChanges = false;
                StoppedRecordingChanges?.Invoke(this, EventArgs.Empty);
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
