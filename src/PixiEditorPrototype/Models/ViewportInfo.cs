using System;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.Models;
internal readonly record struct ViewportInfo(
    double Angle,
    VecD Center,
    VecD RealDimensions,
    VecD Dimensions,
    ChunkResolution Resolution,
    Guid GuidValue,
    bool Delayed,
    Action InvalidateVisual);
