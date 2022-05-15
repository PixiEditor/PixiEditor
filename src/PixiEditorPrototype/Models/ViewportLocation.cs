using System;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.Models;
internal readonly record struct ViewportLocation
    (double Angle, VecD Center, VecD RealDimensions, VecD Dimensions, ChunkResolution Resolution, Guid GuidValue, Action InvalidateVisual);
