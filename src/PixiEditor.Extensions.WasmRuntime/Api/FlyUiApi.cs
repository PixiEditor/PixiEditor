using Avalonia.Threading;
using PixiEditor.Extensions.FlyUI.Elements;

namespace PixiEditor.Extensions.WasmRuntime.Api;

internal class FlyUIApi : ApiGroupHandler
{
    [ApiFunction("subscribe_to_event")]
    public void SubscribeToEvent(int controlId, string eventName)
    {
        if (!LayoutBuilder.ManagedElements.ContainsKey(controlId))
            return;

        LayoutBuilder.ManagedElements[controlId].AddEvent(eventName, (args) =>
        {
            var action = Instance.GetAction<int, int>("raise_element_event");
            var ptr = WasmMemoryUtility.WriteString(eventName);

            action.Invoke(controlId, ptr);
            WasmMemoryUtility.Free(ptr);
        });
    }

    [ApiFunction("state_changed")]
    public void StateChanged(int controlId, Span<byte> bodySpan)
    {
        if (!LayoutBuilder.ManagedElements.TryGetValue(controlId, out var element))
            return;

        var body = LayoutBuilder.Deserialize(bodySpan, DuplicateResolutionTactic.ReplaceRemoveChildren);

        Dispatcher.UIThread.InvokeAsync(() =>
        {
            LayoutBuilder.ManagedElements[controlId] = element;
            if (element is StatefulContainer statefulElement && body is StatefulContainer statefulBodyElement)
            {
                statefulElement.State.SetState(() => statefulElement.State.Content = statefulBodyElement.State.Content);
            }
        });
    }
}
