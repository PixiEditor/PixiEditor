using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.ChangeableDocument.Changeables;

internal abstract class StructureMember : IChangeable, IReadOnlyStructureMember, IDisposable
{
    public float Opacity { get; set; } = 1f;
    public bool IsVisible { get; set; } = true;
    public bool ClipToMemberBelow { get; set; } = false;
    public string Name { get; set; } = "Unnamed";
    public BlendMode BlendMode { get; set; } = BlendMode.Normal;
    public Guid GuidValue { get; init; }
    public ChunkyImage? Mask { get; set; } = null;
    IReadOnlyChunkyImage? IReadOnlyStructureMember.Mask => Mask;

    internal abstract StructureMember Clone();
    public abstract void Dispose();
}
