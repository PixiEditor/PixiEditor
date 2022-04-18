using ChunkyImageLib;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces
{
    public interface IReadOnlyStructureMember
    {
        bool IsVisible { get; }
        string Name { get; }
        Guid GuidValue { get; }
        float Opacity { get; }
        BlendMode BlendMode { get; }
        IReadOnlyChunkyImage? ReadOnlyMask { get; }
    }
}
