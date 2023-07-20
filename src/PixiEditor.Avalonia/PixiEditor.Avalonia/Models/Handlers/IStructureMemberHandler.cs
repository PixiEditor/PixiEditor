using Avalonia.Media.Imaging;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.Models.Enums;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.Models.Containers;

internal interface IStructureMemberHandler : IHandler
{
    public bool HasMaskBindable { get; }
    public Guid GuidValue { get; }
    public string NameBindable { get; set; }
    public DrawingSurface? MaskPreviewSurface { get; set; }
    public DrawingSurface? PreviewSurface { get; set; }
    public WriteableBitmap? PreviewBitmap { get; set; }
    public WriteableBitmap? MaskPreviewBitmap { get; set; }
    public bool MaskIsVisibleBindable { get; set; }
    public StructureMemberSelectionType Selection { get; set; }
    public float OpacityBindable { get; set; }
    public void SetMaskIsVisible(bool infoIsVisible);
    public void SetClipToMemberBelowEnabled(bool infoClipToMemberBelow);
    public void SetBlendMode(BlendMode infoBlendMode);
    public void SetHasMask(bool infoHasMask);
    public void SetOpacity(float infoOpacity);
    public void SetIsVisible(bool infoIsVisible);
    public void SetName(string infoName);
}
