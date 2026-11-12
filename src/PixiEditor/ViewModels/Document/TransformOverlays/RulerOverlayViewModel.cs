using CommunityToolkit.Mvvm.ComponentModel;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

public partial class RulerOverlayViewModel : ViewModelBase, IRulerOverlayHandler
{
    [ObservableProperty] public partial float Dpi { get; set; }

    [ObservableProperty] public partial bool IsEnabled { get; set; }

    public RulerOverlayViewModel(float dpi)
    {
        Dpi = dpi;
    }
}
