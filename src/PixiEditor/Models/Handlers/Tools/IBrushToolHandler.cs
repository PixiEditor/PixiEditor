using Drawie.Backend.Core.Vector;
using PixiEditor.Models.Input;

namespace PixiEditor.Models.Handlers.Tools;

internal interface IBrushToolHandler : IToolHandler
{
    public bool IsCustomBrushTool { get; }
    KeyCombination? DefaultShortcut { get; }
}
