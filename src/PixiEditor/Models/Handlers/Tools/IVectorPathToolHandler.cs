using Drawie.Backend.Core.Vector;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.Models.Handlers.Tools;

internal interface IVectorPathToolHandler : IToolHandler
{
    public VectorPathFillType FillMode { get; }
}
