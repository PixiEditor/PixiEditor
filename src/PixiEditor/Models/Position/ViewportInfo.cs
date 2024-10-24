using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

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
    Guid Id,
    bool Delayed,
    Action InvalidateVisual);
