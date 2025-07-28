using PixiEditor.Common;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyKeyFrameData : ICacheable
{
    int StartFrame { get; }
    int Duration { get; }
    Guid KeyFrameGuid { get; }
    object Data { get; }
    string AffectedElement { get; }
    bool IsVisible { get; }
    bool IsInFrame(int frame);
}
