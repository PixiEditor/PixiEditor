namespace PixiEditor.DrawingApi.Core.Surface;

/// <summary>The logical operations that can be performed when combining two regions.</summary>
public enum ClipOperation
{
    /// <summary>Subtract the op region from the first region.</summary>
    Difference,
    
    /// <summary>Intersect the two regions.</summary>
    Intersect,
}
