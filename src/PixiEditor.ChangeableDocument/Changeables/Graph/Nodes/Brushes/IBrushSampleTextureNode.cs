using Drawie.Backend.Core;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

public interface IBrushSampleTextureNode
{
    public OutputProperty<Texture> TargetSampleTexture { get; }
    public OutputProperty<Texture> LatestSampleTexture { get; }
    public OutputProperty<Texture> StartingSampleTexture { get; }
    public OutputProperty<Texture> TargetFullTexture { get; }
    public OutputProperty<Texture> LatestFullTexture { get; }
    public OutputProperty<Texture> StartingFullTexture { get; }
}
