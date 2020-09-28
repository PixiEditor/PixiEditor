using System;

namespace PixiEditor.UpdateModule
{
    public class UpdateProgressChangedEventArgs : EventArgs
    {
        public float Progress { get; set; }

        public UpdateProgressChangedEventArgs(float progress)
        {
            Progress = progress;
        }
    }
}