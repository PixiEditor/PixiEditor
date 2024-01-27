using PixiEditor.Extensions.CommonApi.LayoutBuilding;
using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;

namespace PixiEditor.Extensions.Wasm.Api.LayoutBuilding;

public abstract class LayoutElement : ILayoutElement<CompiledControl>
{
    private Dictionary<string, List<ElementEventHandler>> _events;
    public List<string> BuildQueuedEvents = new List<string>();
    public int UniqueId { get; set; }

    public abstract CompiledControl Build();

    public LayoutElement()
    {
        UniqueId = LayoutElementIdGenerator.GetNextId();
        LayoutElementsStore.AddElement(UniqueId, this);
    }

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

    protected void BuildPendingEvents(CompiledControl control)
    {
        foreach (string eventName in BuildQueuedEvents)
        {
            control.QueuedEvents.Add(eventName);
        }
    }
}
