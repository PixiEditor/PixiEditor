using ChunkyImageLib;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Models.Rendering;

namespace PixiEditor.Models.Handlers;

internal interface IKeyFrameHandler
{
    public PreviewPainter? PreviewPainter { get; set; }
    public int StartFrameBindable { get; }
    public int DurationBindable { get; }
    public bool IsSelected { get; set; }
    public Guid LayerGuid { get; }
    public Guid Id { get; }
    public bool IsVisible { get; }
    public IDocument Document { get; }
}
