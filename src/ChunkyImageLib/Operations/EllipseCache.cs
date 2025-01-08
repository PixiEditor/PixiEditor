using System.Collections.Concurrent;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace ChunkyImageLib.Operations;

public static class EllipseCache
{
    public static readonly ConcurrentDictionary<VecI, VectorPath> Ellipses = new();
}
