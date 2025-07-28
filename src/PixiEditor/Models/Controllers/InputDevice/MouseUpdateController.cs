using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace PixiEditor.Models.Controllers.InputDevice;

#nullable enable
public class MouseUpdateController : IDisposable
{
    private bool isDisposed = false;

    private readonly Control element;
    private readonly Action<PointerEventArgs> mouseMoveHandler;
    private MouseUpdateControllerSession? session;

    public MouseUpdateController(Control uiElement, Action<PointerEventArgs> onMouseMove)
    {
        mouseMoveHandler = onMouseMove;
        element = uiElement;
        
        element.Loaded += OnElementLoaded;
        element.Unloaded += OnElementUnloaded;

        session ??= new MouseUpdateControllerSession(StartListening, StopListening, mouseMoveHandler); 

        element.PointerMoved += CallMouseMoveInput;
    }

    void OnElementLoaded(object? o, RoutedEventArgs routedEventArgs)
    {
        session ??= new MouseUpdateControllerSession(StartListening, StopListening, mouseMoveHandler);
    }

    private void OnElementUnloaded(object? o, RoutedEventArgs routedEventArgs)
    {
        session.Dispose();
        session = null;
    }

    private void StartListening()
    {
        if (isDisposed)
            return;
        element.PointerMoved -= CallMouseMoveInput;
        element.PointerMoved += CallMouseMoveInput;
    }

    private void CallMouseMoveInput(object? sender, PointerEventArgs e)
    {
        if (isDisposed)
            return;
        session?.MouseMoveInput(e);
    }

    private void StopListening()
    {
        if (isDisposed)
            return;
        element.PointerMoved -= CallMouseMoveInput;
    }

    public void Dispose()
    {
        element.RemoveHandler(InputElement.PointerMovedEvent, CallMouseMoveInput);
        element.RemoveHandler(Control.LoadedEvent, OnElementLoaded);
        element.RemoveHandler(Control.UnloadedEvent, OnElementUnloaded);
        session?.Dispose();
        isDisposed = true;
    }
}
