using ChunkyImageLib;
using Drawie.Backend.Core;
using PixiEditor.Models.Rendering;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Handlers;

internal interface ICelHandler : IDisposable
{
    TexturePreview? PreviewTexture { get; set; }
    public int StartFrameBindable { get; }
    public int DurationBindable { get; }
    public bool IsSelected { get; set; }
    public Guid LayerGuid { get; }
    public Guid Id { get; }
    public bool IsVisible { get; }
    public IDocument Document { get; }
    bool IsWithinRange(int frame);
}
