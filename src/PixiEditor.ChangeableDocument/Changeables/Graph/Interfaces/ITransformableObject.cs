using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface ITransformableObject
{
    public Matrix3X3 TransformationMatrix { get; set; }
}
