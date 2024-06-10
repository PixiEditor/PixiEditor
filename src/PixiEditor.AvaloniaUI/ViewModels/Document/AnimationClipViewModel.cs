using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

public class AnimationClipViewModel(int startFrame, int duration) : IClipHandler
{
    public int StartFrame { get; } = startFrame;
    public int Duration { get; } = duration;
}
