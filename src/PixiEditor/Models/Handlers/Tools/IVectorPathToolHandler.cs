using Drawie.Backend.Core.Vector;

namespace PixiEditor.Models.Handlers.Tools;

internal interface IVectorPathToolHandler : IToolHandler
{
    public PathFillType FillMode { get; }
}
