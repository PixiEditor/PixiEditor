using System.Collections.Immutable;
using Avalonia.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.Models.Containers;

public interface IReferenceLayerHandler : IHandler
{
    public WriteableBitmap? ReferenceBitmap { get; protected set; }
    public ShapeCorners ReferenceShapeBindable { get; set; }
    public bool IsTopMost { get; set; }
    public bool IsTransforming { get; set; }
    public void SetReferenceLayerIsVisible(bool infoIsVisible);
    public void TransformReferenceLayer(ShapeCorners infoCorners);
    public void DeleteReferenceLayer();
    public void SetReferenceLayer(ImmutableArray<byte> infoImagePbgra32Bytes, VecI infoImageSize, ShapeCorners infoShape);
    public void SetReferenceLayerTopMost(bool infoIsTopMost);
}
