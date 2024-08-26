using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IBackgroundInput
{
    InputProperty<Texture?> Background { get; }
}
