using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.Models.Handlers;

internal interface IVectorLayerHandler : ILayerHandler
{
    public IReadOnlyShapeVectorData? GetShapeData(KeyFrameTime frameTime);
}
