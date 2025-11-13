using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Rendering.ContextData;

public struct ViewportData
{
    public Matrix3X3 Transform { get; set; } = Matrix3X3.Identity;
    public VecD Translation { get; set; }
    public double Zoom { get; set; } = 1.0;
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }

    public ViewportData()
    {
    }

    public ViewportData(Matrix3X3 toMatrix3X3, VecD sceneCanvasPos, double sceneScale, bool flipX, bool flipY)
    {
        Transform = toMatrix3X3;
        Translation = sceneCanvasPos;
        Zoom = sceneScale;
        FlipX = flipX;
        FlipY = flipY;
    }
}
