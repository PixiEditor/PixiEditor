using System;

namespace PixiEditor.UpdateModule;

public class UpdateProgressChangedEventArgs : EventArgs
{
    public UpdateProgressChangedEventArgs(float progress)
    {
        Progress = progress;
    }

    public float Progress { get; set; }
}