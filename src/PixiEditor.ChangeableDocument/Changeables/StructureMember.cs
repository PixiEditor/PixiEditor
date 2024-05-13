using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables;

internal abstract class StructureMember : IChangeable, IReadOnlyStructureMember, IDisposable
{
    // Don't forget to update CreateStructureMember_ChangeInfo, CreateLayer_ChangeInfo, CreateFolder_ChangeInfo,
    // DocumentUpdater.ProcessCreateStructureMember, Layer.Clone and Folder.Clone when adding new properties
    public float Opacity { get; set; } = 1f;
    public bool IsVisible { get; set; } = true;
    public bool ClipToMemberBelow { get; set; } = false;
    public string Name { get; set; } = "Unnamed";
    public BlendMode BlendMode { get; set; } = BlendMode.Normal;
    public Guid GuidValue { get; set; }
    public ChunkyImage? Mask { get; set; } = null;
    public abstract RectI? GetTightBounds();

    public bool MaskIsVisible { get; set; } = true;
    IReadOnlyChunkyImage? IReadOnlyStructureMember.Mask => Mask;
    internal abstract StructureMember Clone();
    public abstract void Dispose();
}
