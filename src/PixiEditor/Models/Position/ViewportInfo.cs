using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace PixiEditor.Models.Position;

/// <summary>
/// Used to keep track of viewports inside DocumentViewModel without directly referencing them
/// </summary>
internal readonly record struct ViewportInfo(
    double Angle,
    VecD Center,
    VecD RealDimensions,
    Matrix3X3 Transform,
    RectI? VisibleDocumentRegion,
    string RenderOutput,
    SamplingOptions Sampling,
    VecD Dimensions,
    ChunkResolution Resolution,
    Guid Id,
    bool Delayed,
    bool IsScene,
    Action InvalidateVisual)
{
}
