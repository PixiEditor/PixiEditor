using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.IdentityProvider;

namespace PixiEditor.ViewModels.User;

public class OwnedProductViewModel : ObservableObject
{
    public ProductData ProductData { get; }

    private bool isInstalled;

    public bool IsInstalled
    {
        get => isInstalled;
        set => SetProperty(ref isInstalled, value);
    }

    private bool isInstalling;

    public bool IsInstalling
    {
        get => isInstalling;
        set => SetProperty(ref isInstalling, value);
    }

    public IAsyncRelayCommand InstallCommand { get; }

    public OwnedProductViewModel(ProductData productData, bool isInstalled,
        IAsyncRelayCommand<string> installContentCommand, Func<string, bool> isInstalledFunc)
    {
        ProductData = productData;
        IsInstalled = isInstalled;
        InstallCommand = new AsyncRelayCommand(
            async () =>
        {
            IsInstalling = true;
            await installContentCommand.ExecuteAsync(ProductData.Id);
            IsInstalling = false;
            IsInstalled = isInstalledFunc(ProductData.Id);
        }, () => !IsInstalled && !IsInstalling);
    }
}
