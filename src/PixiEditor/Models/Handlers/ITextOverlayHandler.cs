using Drawie.Numerics;

namespace PixiEditor.Models.Handlers;

public interface ITextOverlayHandler : IHandler
{
    public void Show(string text, VecD position, double fontSize);
    public void Hide();
}
