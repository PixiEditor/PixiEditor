using System.Collections.Immutable;
using Avalonia;
using Avalonia.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Handlers;

public interface IReferenceLayerHandler : IHandler
{
    public Texture? ReferenceTexture { get; }
    public ShapeCorners ReferenceShapeBindable { get; set; }
    public bool IsTopMost { get; set; }
    public bool IsVisible { get;  }
    public bool IsTransforming { get; set; }
    public Matrix3X3 ReferenceTransformMatrix { get; }
    public void SetReferenceLayerIsVisible(bool infoIsVisible);
    public void TransformReferenceLayer(ShapeCorners infoCorners);
    public void DeleteReferenceLayer();
    public void SetReferenceLayer(ImmutableArray<byte> imageBgra8888Bytes, VecI infoImageSize, ShapeCorners infoShape);
    public void SetReferenceLayerTopMost(bool infoIsTopMost);
}
