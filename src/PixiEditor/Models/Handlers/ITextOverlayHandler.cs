using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;

namespace PixiEditor.Models.Handlers;

public interface ITextOverlayHandler : IHandler
{
    public void Show(string text, VecD position, Font font, Matrix3X3 matrix, double? spacing = null);
    public void Hide();
    public Font Font { get; set; }
    public VecD Position { get; set; }
    public double? Spacing { get; set; }
    public bool IsActive { get; }
    public bool PreviewSize { get; set; }
    public void SetCursorPosition(VecD closestToPosition);
}
