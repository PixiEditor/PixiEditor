using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;
using PixiEditor.Extensions.FlyUI.Converters;
using Cursor = PixiEditor.Extensions.CommonApi.FlyUI.Cursor;

namespace PixiEditor.Extensions.FlyUI.Elements;

public abstract class LayoutElement : ILayoutElement<Control>, INotifyPropertyChanged, IPropertyDeserializable
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

    public Cursor? Cursor { get; set; }

    private Dictionary<string, List<ElementEventHandler>>? _events;

    public virtual Control BuildNative()
    {
        Control control = CreateNativeControl();

        BuildCore(control);
        return control;
    }

    protected void BuildCore(Control control)
    {
        if (Cursor != null)
        {
            control.Cursor =
                new Avalonia.Input.Cursor((StandardCursorType)(Cursor.Value.BuiltInCursor ?? BuiltInCursor.None));
        }

        SubscribeBasicEvents(control);
    }

    private void SubscribeBasicEvents(Control control)
    {
        control.PointerEntered += (sender, args) =>
            RaiseEvent(nameof(PointerEnter), new ElementEventArgs() { Sender = this });
        control.PointerExited += (sender, args) =>
            RaiseEvent(nameof(PointerLeave), new ElementEventArgs() { Sender = this });
        control.PointerPressed += (sender, args) =>
            RaiseEvent(nameof(PointerPressed), new ElementEventArgs() { Sender = this });
        control.PointerReleased += (sender, args) =>
            RaiseEvent(nameof(PointerReleased), new ElementEventArgs() { Sender = this });
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

        // I'm unsure if it's a correct solution, it prevents resubscription of the same event during
        // state change. If event count for the same name is bigger than 1, the same event will be called
        // twice in extension, it won't resolve correct handle within the extension.
        // TODO: Research if it's a correct solution
        if (_events[eventName].Count > 0)
        {
            _events[eventName].Clear();
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

        // I'm unsure if it's a correct solution, it prevents resubscription of the same event during
        // state change. If event count for the same name is bigger than 1, the same event will be called
        // twice in extension, it won't resolve correct handle within the extension.
        // TODO: Research if it's a correct solution
        if (_events[eventName].Count > 0)
        {
            _events[eventName].Clear();
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

    public IEnumerable<object> GetProperties()
    {
        yield return Cursor;
        foreach (var property in GetControlProperties())
        {
            yield return property;
        }
    }

    protected virtual IEnumerable<object> GetControlProperties()
    {
        yield break;
    }

    public void DeserializeProperties(List<object> values)
    {
        Cursor = (Cursor?)values.ElementAtOrDefault(0);
        var subValues = values[1..];
        DeserializeControlProperties(subValues);
    }

    protected virtual void DeserializeControlProperties(List<object> values) { }
}
