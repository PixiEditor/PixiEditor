using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.Models.Handlers.Tools;

internal interface IVectorPathToolHandler : IToolHandler
{
    public VectorPathFillType FillMode { get; }
    public StrokeCap StrokeLineCap { get; }
    public StrokeJoin StrokeLineJoin { get; }
}
