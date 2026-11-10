using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

public partial class RulerOverlayViewModel : ViewModelBase, IRulerOverlayHandler
{
    [ObservableProperty]
    public partial bool IsEnabled { get; set; }
}
