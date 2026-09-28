using CommunityToolkit.Mvvm.ComponentModel;

namespace PixiEditor.ViewModels.ExtensionManager;

public class ExtensionsTab : ObservableObject
{
    private bool showStatusIndicator;
    public string Id { get; }
    public string Name { get; }
    public bool ShowStatusIndicator
    {
        get => showStatusIndicator;
        set => SetProperty(ref showStatusIndicator, value);
    }

    public ExtensionsTab(string id, string name, bool showStatusIndicator = false)
    {
        Id = id;
        Name = name;
        ShowStatusIndicator = showStatusIndicator;
    }
}
