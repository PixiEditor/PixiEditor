using System.Timers;
using System.Windows;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace PixiEditor.Models.Controllers;

public class MouseUpdateController : IDisposable
{
    private const int MouseUpdateIntervalMs = 7;  // 7ms ~= 142 Hz
    
    private readonly System.Timers.Timer _timer;
    
    private InputElement element;
    
    private Action mouseMoveHandler;
    
    public MouseUpdateController(InputElement uiElement, Action onMouseMove)
    {
        mouseMoveHandler = onMouseMove;
        element = uiElement;
        
        _timer = new System.Timers.Timer(MouseUpdateIntervalMs);
        _timer.AutoReset = true;
        _timer.Elapsed += TimerOnElapsed;
        
        element.AddHandler(InputElement.PointerMovedEvent, OnMouseMove);
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _timer.Stop();
        element.AddHandler(InputElement.PointerMovedEvent, OnMouseMove);
    }

    private void OnMouseMove(object sender, PointerEventArgs e)
    {
        element.RemoveHandler(InputElement.PointerMovedEvent, OnMouseMove);
        _timer.Start();
        mouseMoveHandler();
    }

    public void Dispose()
    {
        _timer.Dispose();
        element.RemoveHandler(InputElement.PointerMovedEvent, OnMouseMove);
    }
}
