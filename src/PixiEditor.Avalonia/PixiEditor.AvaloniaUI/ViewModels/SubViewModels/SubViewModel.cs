namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

internal class SubViewModel<T> : ViewModelBase
    where T : ViewModelBase
{
    public T Owner { get; protected set; }

    public SubViewModel(T owner)
    {
        Owner = owner;
    }
}
