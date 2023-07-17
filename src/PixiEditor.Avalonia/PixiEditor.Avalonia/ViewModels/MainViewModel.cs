using System.Reactive;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Localization;
using PixiEditor.Platform;
using PixiEditor.ViewModels.SubViewModels;
using ReactiveUI;

namespace PixiEditor.Avalonia.ViewModels;

internal class MainViewModel : ViewModelBase
{
    public event Action OnStartupEvent;

    public IServiceProvider Services { get; set; }
    public CommandController CommandController { get; set; }
    public ReactiveCommand<Unit, Unit> OnStartupCommand { get; }
    public ToolsViewModel ToolsViewModel { get; set; }

    public IPreferences Preferences { get; set; }
    public ILocalizationProvider LocalizationProvider { get; set; }

    public MainViewModel()
    {
        OnStartupCommand = ReactiveCommand.Create(OnStartup);
    }

    public void Setup(IServiceProvider services)
    {
        Services = services;

        Preferences = services.GetRequiredService<IPreferences>();
        Preferences.Init();

        LocalizationProvider = services.GetRequiredService<ILocalizationProvider>();
        LocalizationProvider.LoadData();

        ToolsViewModel = services.GetService<ToolsViewModel>();

        CommandController = services.GetService<CommandController>();
        CommandController.Init(services);
    }

    private void OnStartup()
    {
        OnStartupEvent?.Invoke();
    }
}
