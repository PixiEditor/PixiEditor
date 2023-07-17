using System.Reactive;
using ReactiveUI;

namespace PixiEditor.Avalonia.ViewModels;

public class MainViewModel : ViewModelBase
{
    public event Action OnStartupEvent;
    public ReactiveCommand<Unit, Unit> OnStartupCommand { get; }

    public MainViewModel()
    {
        OnStartupCommand = ReactiveCommand.Create(OnStartup);
    }

    private void OnStartup()
    {
        OnStartupEvent?.Invoke();
    }
}
