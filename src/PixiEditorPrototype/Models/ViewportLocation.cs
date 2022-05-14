using System;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.Models;
internal readonly record struct ViewportLocation
    (double Angle, Vector2d Center, Vector2d RealDimensions, Vector2d Dimensions, ChunkResolution Resolution, Guid GuidValue, Action InvalidateVisual);
