using PixiEditor.ChangeableDocument.Changeables.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public abstract class Clip : IReadOnlyClip
{
    public int StartFrame { get; set; }
    public int Duration { get; } = 1;
    
    public abstract void ActiveFrameChanged(int atFrame);
    public abstract void Deactivated(int atFrame);
}
