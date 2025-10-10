using Drawie.Backend.Core.Vector;

namespace PixiEditor.Models.Handlers.Tools;

internal interface IBrushToolHandler : IToolHandler
{
    VectorPath? FinalBrushShape { get; set; }
}
