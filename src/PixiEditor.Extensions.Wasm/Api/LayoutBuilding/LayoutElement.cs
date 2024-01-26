using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public abstract class LayoutElement : ILayoutElement<NativeControl>
{
    private Dictionary<string, List<ElementEventHandler>> _events;

    private int InternalControlId { get; set; }

    public abstract NativeControl Build();

    public LayoutElement()
    {
        InternalControlId = LayoutElementIdGenerator.GetNextId();
        LayoutElementsStore.AddElement(InternalControlId, this);
    }

    ~LayoutElement()
    {
        LayoutElementsStore.RemoveElement(InternalControlId);
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
}
