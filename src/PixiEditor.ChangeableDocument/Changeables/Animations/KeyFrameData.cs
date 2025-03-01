using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changeables.Animations;

public class KeyFrameData : IDisposable, IReadOnlyKeyFrameData
{
    public int StartFrame { get; set; }
    public int Duration { get; set; }
    public Guid KeyFrameGuid { get; }
    public string AffectedElement { get; set; }
    public object Data { get; set; }
    public bool IsVisible { get; set; } = true;

    private int _lastCacheHash;

    public bool RequiresUpdate
    {
        get
        {
            if (Data is ICacheable cacheable)
            {
                return cacheable.GetCacheHash() != _lastCacheHash;
            }

            return false;
        }
        set
        {
            if (Data is ICacheable cacheable)
            {
                _lastCacheHash = cacheable.GetCacheHash();
            }
        }
    }


    public KeyFrameData(Guid keyFrameGuid, int startFrame, int duration, string affectedElement)
    {
        KeyFrameGuid = keyFrameGuid;
        StartFrame = startFrame;
        Duration = duration;
        AffectedElement = affectedElement;
    }

    public bool IsInFrame(int frame)
    {
        return IsVisible && frame >= StartFrame && frame < StartFrame + Duration;
    }

    public void Dispose()
    {
        if (Data is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public KeyFrameData Clone(bool newGuid = false)
    {
        if (Data is not ICloneable && !Data.GetType().IsValueType && Data is not string)
        {
            throw new InvalidOperationException("Data must be ICloneable, ValueType or string to be cloned");
        }
        
        Guid newGuidValue = newGuid ? Guid.NewGuid() : KeyFrameGuid;
        
        return new KeyFrameData(newGuidValue, StartFrame, Duration, AffectedElement)
        {
            Data = Data is ICloneable cloneable ? cloneable.Clone() : Data,
            IsVisible = IsVisible
        };
    }

    public int GetCacheHash()
    {
        HashCode hash = new();
        hash.Add(StartFrame);
        hash.Add(Duration);
        hash.Add(KeyFrameGuid);
        hash.Add(AffectedElement);
        if (Data != null)
        {
            hash.Add(Data is ICacheable cacheable ? cacheable.GetCacheHash() : Data.GetHashCode());
        }

        return hash.ToHashCode();
    }
}
