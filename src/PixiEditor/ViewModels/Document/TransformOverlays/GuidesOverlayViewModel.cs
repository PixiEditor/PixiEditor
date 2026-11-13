using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Models.Controllers.InputDevice;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

public partial class GuidesOverlayViewModel : ViewModelBase
{
    private SnappingController snappingController;
    private bool isEnabled;

    public bool IsEnabled
    {
        get => isEnabled;
        set
        {
            if (SetProperty(ref isEnabled, value))
            {
                if (value)
                {
                    snappingController.EnableHorizontalStartingWith("guide_");
                    snappingController.EnableVerticalStartingWith("guide_");
                }
                else
                {
                    snappingController.DisableHorizontalStartingWith("guide_");
                    snappingController.DisableVerticalStartingWith("guide_");
                }
            }
        }
    }


    [ObservableProperty]
    public partial bool CanEdit { get; set; }

    public GuidesOverlayViewModel(SnappingController controller)
    {
        snappingController = controller;
    }
}
