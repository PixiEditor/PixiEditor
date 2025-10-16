using Drawie.Backend.Core.Vector;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Handlers.Tools;

internal interface IBrushToolHandler : IToolHandler
{
    VectorPath? FinalBrushShape { get; set; }
    public bool IsCustomBrushTool { get; }
    KeyCombination? DefaultShortcut { get; }
}
