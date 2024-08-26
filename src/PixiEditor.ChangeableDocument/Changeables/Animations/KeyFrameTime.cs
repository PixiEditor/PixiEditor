namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public struct KeyFrameTime
{
    public KeyFrameTime(int frame, double normalizedTime)
    {
        this.Frame = frame;
        this.NormalizedTime = normalizedTime;
    }

    public int Frame { get; init; }
    public double NormalizedTime { get; init; }
    
    public static implicit operator KeyFrameTime(int frame) => new KeyFrameTime(frame, 0);

    public bool Equals(KeyFrameTime other)
    {
        return Frame == other.Frame && NormalizedTime.Equals(other.NormalizedTime);
    }
}
