using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.Models.Position;

/// <summary>
/// Used to keep track of viewports inside DocumentViewModel without directly referencing them
/// </summary>
internal readonly record struct ViewportInfo(
    double Angle,
    VecD Center,
    VecD RealDimensions,
    VecD Dimensions,
    ChunkResolution Resolution,
    Guid GuidValue,
    bool Delayed,
    Action InvalidateVisual);
