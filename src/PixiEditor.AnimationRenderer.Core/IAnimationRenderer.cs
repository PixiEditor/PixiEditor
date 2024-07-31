using PixiEditor.DrawingApi.Core.Surfaces.ImageData;

namespace PixiEditor.AnimationRenderer.Core;

public interface IAnimationRenderer
{
    public Task<bool> RenderAsync(List<Image> imageStream, string outputPath);
}
