using System.Reactive;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Commands;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.Avalonia.ViewModels;

internal partial class MainViewModel : ViewModelBase
{
    public event Action OnStartupEvent;

    public IServiceProvider Services { get; set; }
    public CommandController CommandController { get; set; }
    public ToolsViewModel ToolsViewModel { get; set; }

    public IPreferences Preferences { get; set; }
    public ILocalizationProvider LocalizationProvider { get; set; }

    public MainViewModel()
    {

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

    [RelayCommand]
    private void OnStartup()
    {
        OnStartupEvent?.Invoke();
    }
}
