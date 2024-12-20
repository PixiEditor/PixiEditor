using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.Models.Handlers;

internal interface ILineOverlayHandler
{
    public void Hide();
    public bool Nudge(VecD distance);
    public void Show(VecD startPos, VecD endPos, bool showApplyButton, Action<(VecD, VecD)> addToUndo);
    public VecD LineStart { get; set; }
    public VecD LineEnd { get; set; }
    public bool ShowHandles { get; set; }
    public bool IsSizeBoxEnabled { get; set; }
}
