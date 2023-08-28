using System.Timers;
using Avalonia.Input;

namespace PixiEditor.AvaloniaUI.Models.Controllers.InputDevice;

public class MouseUpdateController : IDisposable
{
    private const int MouseUpdateIntervalMs = 7;  // 7ms ~= 142 Hz
    
    private readonly System.Timers.Timer _timer;
    
    private InputElement element;
    
    private Action<PointerEventArgs> mouseMoveHandler;
    
    public MouseUpdateController(InputElement uiElement, Action<PointerEventArgs> onMouseMove)
    {
        mouseMoveHandler = onMouseMove;
        element = uiElement;
        
        _timer = new System.Timers.Timer(MouseUpdateIntervalMs);
        _timer.AutoReset = true;
        _timer.Elapsed += TimerOnElapsed;
        
        element.PointerMoved += OnMouseMove;
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _timer.Stop();
        element.PointerMoved += OnMouseMove;
    }

    private void OnMouseMove(object sender, PointerEventArgs e)
    {
        element.PointerMoved -= OnMouseMove;
        _timer.Start();
        mouseMoveHandler(e);
    }

    public void Dispose()
    {
        _timer.Dispose();
        element.RemoveHandler(InputElement.PointerMovedEvent, OnMouseMove);
    }
}
