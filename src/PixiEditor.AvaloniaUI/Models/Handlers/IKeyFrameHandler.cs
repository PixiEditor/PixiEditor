using ChunkyImageLib;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface IKeyFrameHandler
{
    public Surface? PreviewSurface { get; set; }
    public int StartFrameBindable { get; }
    public int DurationBindable { get; }
    public bool IsSelected { get; set; }
    public Guid LayerGuid { get; }
    public Guid Id { get; }
    public bool IsVisible { get; }
    public IDocument Document { get; }
}
