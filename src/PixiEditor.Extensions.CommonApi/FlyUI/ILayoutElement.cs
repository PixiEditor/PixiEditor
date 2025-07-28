using PixiEditor.Extensions.CommonApi.FlyUI.Events;

namespace PixiEditor.Extensions.CommonApi.FlyUI;

public interface ILayoutElement<out TBuildResult>
{
    public int UniqueId { get; set; }
    public TBuildResult BuildNative();
    public void AddEvent(string eventName, ElementEventHandler eventHandler);
    public void RemoveEvent(string eventName, ElementEventHandler eventHandler);
    public void RaiseEvent(string eventName, ElementEventArgs args);
}
