using Drawie.Backend.Core.Vector;

namespace PixiEditor.Models.Handlers.Tools;

internal interface IPenToolHandler : IToolHandler
{
    public bool PixelPerfectEnabled { get; }
    VectorPath? FinalBrushShape { get; set; }
}
