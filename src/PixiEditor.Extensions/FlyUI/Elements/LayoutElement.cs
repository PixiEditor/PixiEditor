using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.FlyUI.Elements;

public abstract class LayoutElement : ILayoutElement<Control>, INotifyPropertyChanged
{
    public int UniqueId { get; set; }
    public event ElementEventHandler PointerEnter
    {
        add => AddEvent(nameof(PointerEnter), value);
        remove => RemoveEvent(nameof(PointerEnter), value);
    }

    public event ElementEventHandler PointerLeave
    {
        add => AddEvent(nameof(PointerLeave), value);
        remove => RemoveEvent(nameof(PointerLeave), value);
    }

    public event ElementEventHandler PointerPressed
    {
        add => AddEvent(nameof(PointerPressed), value);
        remove => RemoveEvent(nameof(PointerPressed), value);
    }

    public event ElementEventHandler PointerReleased
    {
        add => AddEvent(nameof(PointerReleased), value);
        remove => RemoveEvent(nameof(PointerReleased), value);
    }

    private Dictionary<string, List<ElementEventHandler>>? _events;

    public virtual Control BuildNative()
    {
        Control control = CreateNativeControl();

        SubscribeBasicEvents(control);
        return control;
    }

    protected void SubscribeBasicEvents(Control control)
    {
        control.PointerEntered += (sender, args) => RaiseEvent(nameof(PointerEnter), new ElementEventArgs() { Sender = this });
        control.PointerExited += (sender, args) => RaiseEvent(nameof(PointerLeave), new ElementEventArgs() { Sender = this });
        control.PointerPressed += (sender, args) => RaiseEvent(nameof(PointerPressed), new ElementEventArgs() { Sender = this });
        control.PointerReleased += (sender, args) => RaiseEvent(nameof(PointerReleased), new ElementEventArgs() { Sender = this });
    }

    protected abstract Control CreateNativeControl();

    public void AddEvent(string eventName, ElementEventHandler eventHandler)
    {
        if (_events == null)
        {
            _events = new Dictionary<string, List<ElementEventHandler>>();
        }

        if (!_events.ContainsKey(eventName))
        {
            _events.Add(eventName, new List<ElementEventHandler>());
        }

        _events[eventName].Add(eventHandler);
    }
    
    public void AddEvent<T>(string eventName, ElementEventHandler<T> eventHandler) where T : ElementEventArgs<T>
    {
        if (_events == null)
        {
            _events = new Dictionary<string, List<ElementEventHandler>>();
        }

        if (!_events.ContainsKey(eventName))
        {
            _events.Add(eventName, new List<ElementEventHandler>());
        }

        _events[eventName].Add((args => eventHandler((T)args)));
    }

    public void RemoveEvent(string eventName, ElementEventHandler eventHandler)
    {
        if (_events == null)
        {
            return;
        }

        if (!_events.ContainsKey(eventName))
        {
            return;
        }

        _events[eventName].Remove(eventHandler);
    }
    
    public void RemoveEvent<T>(string eventName, ElementEventHandler<T> eventHandler) where T : ElementEventArgs<T>
    {
        if (_events == null)
        {
            return;
        }

        if (!_events.ContainsKey(eventName))
        {
            return;
        }

        _events[eventName].Remove((args => eventHandler((T)args)));
    }

    public void RaiseEvent(string eventName, ElementEventArgs args)
    {
        if (_events == null)
        {
            return;
        }

        if (!_events.ContainsKey(eventName))
        {
            return;
        }

        foreach (ElementEventHandler eventHandler in _events[eventName])
        {
            eventHandler.Invoke(args);
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
