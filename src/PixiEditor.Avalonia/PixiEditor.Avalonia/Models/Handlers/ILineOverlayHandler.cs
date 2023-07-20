using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.Models.Containers;

internal interface ILineOverlayHandler
{
    public void Hide();
    public void Nudge(VecI distance);
    public void Undo();
    public void Redo();
    public void Show(VecD startPos, VecD curPos);
}
