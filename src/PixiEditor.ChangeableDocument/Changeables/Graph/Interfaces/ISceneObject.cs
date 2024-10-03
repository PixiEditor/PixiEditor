using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface ISceneObject
{
    public VecD ScenePosition { get; }
    public VecD SceneSize { get; }
    
    public RectD GlobalBounds => new RectD(ScenePosition, SceneSize);
    
    public void Render(SceneObjectRenderContext context);
}
