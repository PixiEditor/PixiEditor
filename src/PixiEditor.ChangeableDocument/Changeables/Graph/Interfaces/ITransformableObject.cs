using Drawie.Backend.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface ITransformableObject
{
    public Matrix3X3 TransformationMatrix { get; set; }
}
