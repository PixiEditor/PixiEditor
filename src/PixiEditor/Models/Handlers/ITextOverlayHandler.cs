using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables;

namespace PixiEditor.Models.Handlers;

public interface ITextOverlayHandler : IHandler
{
    public RichText Text { get; set; }
    public void Show(RichText text, VecD position, Matrix3X3 matrix);
    public void Hide();
    public VecD Position { get; set; }
    public bool IsActive { get; }
    public bool PreviewSize { get; set; }
    public void SetCursorPosition(VecD closestToPosition);
    public int? CurrentlyEditingInlineIndex { get; }
    public int CursorPosition { get; set; }
    public int SelectionEnd { get; set; }
}
