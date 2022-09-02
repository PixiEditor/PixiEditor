using System;

namespace PixiEditor.DrawingApi.Core.Surface.Vector;

[Flags]
public enum PathSegmentMask
{
    /// <summary>The path contains one or more line segments.</summary>
    Line = 1,
    
    /// <summary>The path contains one or more quad segments.</summary>
    Quad = 2,
    
    /// <summary>The path contains one or more conic segments.</summary>
    Conic = 4,
    
    /// <summary>The path contains one or more cubic segments.</summary>
    Cubic = 8,
}
