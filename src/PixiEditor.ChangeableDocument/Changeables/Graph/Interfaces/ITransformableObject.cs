using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface ITransformableObject
{
    /// <summary>
    ///     Position in x and y.
    /// </summary>
    public VecD Position { get; set; }
    
    /// <summary>
    ///     Scale in x and y.
    /// </summary>
    public VecD Size { get; set; }
    
    /// <summary>
    ///     Rotation in radians.
    /// </summary>
    public double RotationRadians { get; set; }
}
