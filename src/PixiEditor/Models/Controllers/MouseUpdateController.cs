using System.Windows;
using System.Windows.Input;

namespace PixiEditor.Models.Controllers;

#nullable enable
public class MouseUpdateController : IDisposable
{
    private bool isDisposed = false;

    private readonly FrameworkElement element;
    private readonly MouseEventHandler mouseMoveHandler;
    private MouseUpdateControllerSession? session;
    
    public MouseUpdateController(FrameworkElement uiElement, MouseEventHandler onMouseMove)
    {
        mouseMoveHandler = onMouseMove;
        element = uiElement;
        
        element.Loaded += OnElementLoaded;
        element.Unloaded += OnElementUnloaded;
        
        session ??= new MouseUpdateControllerSession(StartListening, StopListening, mouseMoveHandler); 
        
        element.MouseMove += CallMouseMoveInput;
    }
    
    void OnElementLoaded(object o, RoutedEventArgs routedEventArgs)
    {
        session ??= new MouseUpdateControllerSession(StartListening, StopListening, mouseMoveHandler);
    }
    
    private void OnElementUnloaded(object o, RoutedEventArgs routedEventArgs)
    {
        session.Dispose();
        session = null;
    }

    private void StartListening()
    {
        if (isDisposed)
            return;
        element.MouseMove -= CallMouseMoveInput;
        element.MouseMove += CallMouseMoveInput;
    }

    private void CallMouseMoveInput(object sender, MouseEventArgs e)
    {
        if (isDisposed)
            return;
        session?.MouseMoveInput(sender, e);
    }
    
    private void StopListening()
    {
        if (isDisposed)
            return;
        element.MouseMove -= CallMouseMoveInput;
    }

    public void Dispose()
    {
        element.MouseMove -= CallMouseMoveInput;
        element.Loaded -= OnElementLoaded;
        element.Unloaded -= OnElementUnloaded;
        session?.Dispose();
        isDisposed = true;
    }
}
