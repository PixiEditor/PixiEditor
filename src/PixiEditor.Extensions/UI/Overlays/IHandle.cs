using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace PixiEditor.Extensions.UI.Overlays;

public interface IHandle
{
    public Control Owner { get; }
    public IBrush HandleBrush { get; set; }
    public IPen? HandlePen { get; set; }
    public double ZoomboxScale { get; set; }

    public void Draw(DrawingContext context);
    protected void OnPressed(PointerPressedEventArgs args);
}
