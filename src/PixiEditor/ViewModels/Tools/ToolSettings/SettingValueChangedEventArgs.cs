namespace PixiEditor.ViewModels.Tools.ToolSettings;

internal class SettingValueChangedEventArgs<T> : EventArgs
{
    public T OldValue { get; set; }

    public T NewValue { get; set; }

    public SettingValueChangedEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
