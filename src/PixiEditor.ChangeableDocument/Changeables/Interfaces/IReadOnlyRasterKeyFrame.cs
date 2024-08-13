using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyRasterKeyFrame : IReadOnlyKeyFrame
{
    IReadOnlyChunkyImage GetTargetImage(IReadOnlyCollection<IReadOnlyNode> allNodes);
}
