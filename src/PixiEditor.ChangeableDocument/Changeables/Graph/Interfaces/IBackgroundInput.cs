using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IBackgroundInput
{
    InputProperty<ChunkyImage?> Background { get; }
}
