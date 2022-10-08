namespace PixiEditor.DrawingApi.Core.Surface.Vector;

/// <summary>The logical operations that can be performed when combining two paths using <see cref="VectorPath.Op(VectorPath, VectorPathOp)" />.</summary>
public enum VectorPathOp
{
    /// <summary>Subtract the op path from the current path.</summary>
    Difference,
    
    /// <summary>Intersect the two paths.</summary>
    Intersect,
    
    /// <summary>Union (inclusive-or) the two paths.</summary>
    Union,
    
    /// <summary>Exclusive-or the two paths.</summary>
    Xor,
    
    /// <summary>Subtract the current path from the op path.</summary>
    ReverseDifference,
}
