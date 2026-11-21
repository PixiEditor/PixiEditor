using Drawie.Backend.Core.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface ITransformableObject
{
    public Matrix3X3 TransformationMatrix { get; set; }

    public Matrix3X3 GetTransformationMatrix(int frame);

    public void SetTransformationMatrix(int frame, Matrix3X3 matrix);

    public Matrix3X3[] GetAllTransformationMatrices();
    public void SetAllTransformationMatrices(Matrix3X3[] matrices);
}
