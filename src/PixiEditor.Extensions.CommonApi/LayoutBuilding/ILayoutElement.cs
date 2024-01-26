using PixiEditor.Extensions.CommonApi.LayoutBuilding.Events;

namespace PixiEditor.Extensions.CommonApi.LayoutBuilding;

public interface ILayoutElement<out TBuildResult>
{
    public TBuildResult Build();
    public void AddEvent(string eventName, ElementEventHandler eventHandler);
    public void RemoveEvent(string eventName, ElementEventHandler eventHandler);
    public void RaiseEvent(string eventName, ElementEventArgs args);
}
