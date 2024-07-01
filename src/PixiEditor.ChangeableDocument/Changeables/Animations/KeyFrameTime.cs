namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public record KeyFrameTime
{
    public KeyFrameTime(int Frame, float NormalizedTime)
    {
        this.Frame = Frame;
        this.NormalizedTime = NormalizedTime;
    }

    public int Frame { get; init; }
    public float NormalizedTime { get; init; }
    
    public static implicit operator KeyFrameTime(int frame) => new KeyFrameTime(frame, 0);
}
