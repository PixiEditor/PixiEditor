using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

public class KeyFrameViewModel(int startFrame, int duration) : IKeyFrameHandler
{
    public int StartFrame { get; } = startFrame;
    public int Duration { get; } = duration;
}
