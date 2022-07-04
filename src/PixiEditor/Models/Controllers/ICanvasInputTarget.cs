using PixiEditor.Models.Tools;
using System.Windows.Input;

namespace PixiEditor.Models.Controllers;

public interface ICanvasInputTarget
{
    void OnToolChange(Tool tool);
    void OnKeyDown(Key key);
    void OnKeyUp(Key key);
    void OnLeftMouseButtonDown(double canvasPosX, double canvasPosY);
    void OnLeftMouseButtonUp();
    void OnMouseMove(double newCanvasX, double newCanvasY);
}