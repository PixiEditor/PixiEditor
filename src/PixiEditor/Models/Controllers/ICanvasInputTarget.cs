using System.Windows.Input;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Controllers;

internal interface ICanvasInputTarget
{
    void OnToolChange(Tool tool);
    void OnKeyDown(Key key);
    void OnKeyUp(Key key);
    void OnLeftMouseButtonDown(double canvasPosX, double canvasPosY);
    void OnLeftMouseButtonUp();
    void OnMouseMove(double newCanvasX, double newCanvasY);
}
