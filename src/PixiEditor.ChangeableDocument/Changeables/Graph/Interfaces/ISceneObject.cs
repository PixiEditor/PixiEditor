using PixiEditor.ChangeableDocument.Changeables.Animations;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface ISceneObject
{
    public VecD GetScenePosition(KeyFrameTime atTime);
    public VecD GetSceneSize(KeyFrameTime atTime);
    
    public RectD GetGlobalBounds(KeyFrameTime atTime) => new RectD(GetScenePosition(atTime), GetSceneSize(atTime));
    
    public void Render(SceneObjectRenderContext context);
}
