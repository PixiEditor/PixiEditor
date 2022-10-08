namespace PixiEditor.DrawingApi.Core.Surface.Vector;

/// <summary>Possible path fill type values.</summary>
public enum PathFillType
{
    /// <summary>Specifies that "inside" is computed by a non-zero sum of signed edge crossings.</summary>
    Winding,
    
    /// <summary>Specifies that "inside" is computed by an odd number of edge crossings.</summary>
    EvenOdd,
    
    /// <summary>Same as <see cref="F:SkiaSharp.SKPathFillType.Winding" />, but draws outside of the path, rather than inside.</summary>
    InverseWinding,
    
    /// <summary>Same as <see cref="F:SkiaSharp.SKPathFillType.EvenOdd" />, but draws outside of the path, rather than inside.</summary>
    InverseEvenOdd,
}
