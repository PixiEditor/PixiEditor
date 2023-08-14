using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.Models.Handlers;

internal interface ILineOverlayHandler
{
    public void Hide();
    public bool Nudge(VecD distance);
    public bool Undo();
    public bool Redo();
    public void Show(VecD startPos, VecD curPos);
}
