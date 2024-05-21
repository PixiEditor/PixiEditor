using PixiDocks.Core.Docking;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

public class DocumentTabCustomizationSettings : TabCustomizationSettings
{
    private bool isSaved;
    public bool IsSaved
    {
        get => isSaved;
        set => SetField(ref isSaved, value);
    }

    public DocumentTabCustomizationSettings(object? icon = null, bool showCloseButton = false, bool isSaved = true) : base(icon, showCloseButton)
    {
        IsSaved = isSaved;
    }
}
