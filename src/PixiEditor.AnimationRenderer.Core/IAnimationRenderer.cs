namespace PixiEditor.AnimationRenderer.Core;

public interface IAnimationRenderer
{
    public Task<bool> RenderAsync(string framesPath, int frameRate = 60);
}
