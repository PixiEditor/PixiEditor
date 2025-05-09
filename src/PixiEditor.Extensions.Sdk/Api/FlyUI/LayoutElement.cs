using PixiEditor.Extensions.CommonApi.FlyUI;
using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public abstract class LayoutElement : ILayoutElement<ControlDefinition>
{
    private Dictionary<string, List<ElementEventHandler>> _events;
    public List<string> BuildQueuedEvents = new List<string>();
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

    public LayoutElement(Cursor? cursor)
    {
        Cursor = cursor;
        UniqueId = LayoutElementIdGenerator.GetNextId();
        LayoutElementsStore.AddElement(UniqueId, this);
    }

    public virtual ControlDefinition BuildNative()
    {
        ControlDefinition control = CreateControl();

        control.InsertProperty(0, Cursor);
        BuildPendingEvents(control);
        return control;
    }

    protected abstract ControlDefinition CreateControl();

    ~LayoutElement()
    {
        LayoutElementsStore.RemoveElement(UniqueId);
    }

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
        BuildQueuedEvents.Add(eventName);
    }

    /*public void AddEvent<TEventArgs>(string eventName, ElementEventHandler<TEventArgs> eventHandler) where TEventArgs : ElementEventArgs<TEventArgs>
    {
        if (_events == null)
        {
            _events = new Dictionary<string, List<ElementEventHandler>>();
        }

        if (!_events.ContainsKey(eventName))
        {
            _events.Add(eventName, new List<ElementEventHandler>());
        }

        _events[eventName].Add((args => eventHandler((TEventArgs)args)));
        BuildQueuedEvents.Add(eventName);
    }*/

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

    /*public void RemoveEvent<TEventArgs>(string eventName, ElementEventHandler<TEventArgs> eventHandler) where TEventArgs : ElementEventArgs<TEventArgs>
    {
        if (_events == null)
        {
            return;
        }

        if (!_events.ContainsKey(eventName))
        {
            return;
        }

        _events[eventName].Remove((args => eventHandler((TEventArgs)args)));
    }*/

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

    protected void BuildPendingEvents(ControlDefinition controlDefinition)
    {
        foreach (string eventName in BuildQueuedEvents)
        {
            controlDefinition.QueuedEvents.Add(eventName);
        }
    }
}
