using System.Collections.Immutable;
using Avalonia;
using Avalonia.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

public interface IReferenceLayerHandler : IHandler
{
    public Surface? ReferenceBitmap { get; }
    public ShapeCorners ReferenceShapeBindable { get; set; }
    public bool IsTopMost { get; set; }
    public bool IsTransforming { get; set; }
    public Matrix ReferenceTransformMatrix { get; }
    public void SetReferenceLayerIsVisible(bool infoIsVisible);
    public void TransformReferenceLayer(ShapeCorners infoCorners);
    public void DeleteReferenceLayer();
    public void SetReferenceLayer(ImmutableArray<byte> imageBgra8888Bytes, VecI infoImageSize, ShapeCorners infoShape);
    public void SetReferenceLayerTopMost(bool infoIsTopMost);
}
