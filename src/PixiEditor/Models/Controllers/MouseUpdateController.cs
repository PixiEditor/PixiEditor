using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace PixiEditor.Models.Controllers;

public class MouseUpdateController : IDisposable
{
    private const int MouseUpdateIntervalMs = 7;  // 7ms ~= 142 Hz
    
    private readonly System.Timers.Timer _timer;
    
    private UIElement element;
    
    private MouseEventHandler mouseMoveHandler;
    
    public MouseUpdateController(UIElement uiElement, MouseEventHandler onMouseMove)
    {
        mouseMoveHandler = onMouseMove;
        element = uiElement;
        
        _timer = new System.Timers.Timer(MouseUpdateIntervalMs);
        _timer.AutoReset = true;
        _timer.Elapsed += TimerOnElapsed;
        
        element.MouseMove += OnMouseMove;
    }

    private void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
        _timer.Stop();
        element.MouseMove += OnMouseMove;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        element.MouseMove -= OnMouseMove;
        _timer.Start();
        mouseMoveHandler(sender, e);
    }

    public void Dispose()
    {
        element.MouseMove -= OnMouseMove;
        _timer.Dispose();
    }
}
