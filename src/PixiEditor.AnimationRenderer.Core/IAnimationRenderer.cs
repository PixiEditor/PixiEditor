using Drawie.Backend.Core.Surfaces.ImageData;

namespace PixiEditor.AnimationRenderer.Core;

public interface IAnimationRenderer
{
    public Task<bool> RenderAsync(List<Image> imageStream, string outputPath, CancellationToken cancellationToken, Action<double>? progressCallback);
    public bool Render(List<Image> imageStream, string outputPath, CancellationToken cancellationToken, Action<double>? progressCallback);
}

public enum QualityPreset
{
    VeryLow = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    VeryHigh = 4,
}
