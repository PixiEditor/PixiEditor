using Drawie.Backend.Core;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Brushes;

public interface IBrushSampleTextureNode
{
    public OutputProperty<Texture> TargetSampleTexture { get; }
    public OutputProperty<Texture> TargetFullTexture { get; }
}
