using Drawie.Backend.Core.Text;
using Drawie.Numerics;

namespace PixiEditor.Models.Handlers;

public interface ITextOverlayHandler : IHandler
{
    public void Show(string text, VecD position, Font font);
    public void Hide();
    public Font Font { get; set; }
    public VecD Position { get; set; }
}
