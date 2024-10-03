using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IBackgroundInput
{
    InputProperty<DrawingSurface?> Background { get; }
}
