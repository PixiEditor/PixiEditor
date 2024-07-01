namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public record KeyFrameTime
{
    public KeyFrameTime(int Frame)
    {
        this.Frame = Frame;
    }

    public int Frame { get; init; }
    
    public static implicit operator KeyFrameTime(int frame) => new KeyFrameTime(frame);
}
