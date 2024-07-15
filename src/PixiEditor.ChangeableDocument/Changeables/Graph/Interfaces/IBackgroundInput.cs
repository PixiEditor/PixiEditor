using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core.Surface.ImageData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IBackgroundInput
{
    InputProperty<Image?> Background { get; }
}
