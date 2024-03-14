using Avalonia.Media.Imaging;
using ChunkyImageLib;
using PixiEditor.AvaloniaUI.Models.Layers;
using PixiEditor.DrawingApi.Core.Surface;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface IStructureMemberHandler : IHandler
{
    public bool HasMaskBindable { get; }
    public Guid GuidValue { get; }
    public string NameBindable { get; set; }
    public Surface? MaskPreviewSurface { get; set; }
    public Surface? PreviewSurface { get; set; }
    public bool MaskIsVisibleBindable { get; set; }
    public StructureMemberSelectionType Selection { get; set; }
    public float OpacityBindable { get; set; }
    public IDocument Document { get; }
    public bool IsVisibleBindable { get; set; }
    public void SetMaskIsVisible(bool infoIsVisible);
    public void SetClipToMemberBelowEnabled(bool infoClipToMemberBelow);
    public void SetBlendMode(BlendMode infoBlendMode);
    public void SetHasMask(bool infoHasMask);
    public void SetOpacity(float infoOpacity);
    public void SetIsVisible(bool infoIsVisible);
    public void SetName(string infoName);
}
