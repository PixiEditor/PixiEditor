namespace PixiEditor.Extensions.CommonApi.FlyUI.Events;

public class ToggleEventArgs : ElementEventArgs<ToggleEventArgs>
{
    public bool IsToggled { get; }

    public ToggleEventArgs(bool isToggled)
    {
        IsToggled = isToggled;
    }
}
