namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public abstract class KeyFrameData : IDisposable
{
    public int StartFrame { get; set; }
    public int Duration { get; set; }
    public Guid KeyFrameGuid { get; }

    public abstract bool RequiresUpdate { get; set; }

    public KeyFrameData(Guid keyFrameGuid, int startFrame, int duration)
    {
        KeyFrameGuid = keyFrameGuid;
        StartFrame = startFrame;
        Duration = duration;
    }

    public bool IsInFrame(int frame)
    {
        return frame >= StartFrame && frame <= StartFrame + Duration;
    }

    public abstract void Dispose();
}

public abstract class KeyFrameData<T> : KeyFrameData
{
    public T Data { get; set; }

    public KeyFrameData(Guid keyFrameGuid, T data, int startFrame, int duration) : base(keyFrameGuid, startFrame,
        duration)
    {
        Data = data;
    }

    public override void Dispose()
    {
        if (Data is IDisposable disposable)
        {
            disposable.Dispose();
        } 
    }
}
