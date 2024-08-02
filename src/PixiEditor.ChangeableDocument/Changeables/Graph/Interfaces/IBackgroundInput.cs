using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.Surface.ImageData;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public interface IBackgroundInput
{
    InputProperty<Surface?> Background { get; }
}
