using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;

namespace PixiEditor.AvaloniaUI.ViewModels.Dock;

internal class DockDocumentViewModel : global::Dock.Model.Avalonia.Controls.Document
{
    public static readonly StyledProperty<ViewportWindowViewModel> ViewModelProperty =
        AvaloniaProperty.Register<DockDocumentViewModel, ViewportWindowViewModel>(nameof(ViewModel));

    public ViewportWindowViewModel ViewModel
    {
        get { return (ViewportWindowViewModel)GetValue(ViewModelProperty); }
        set { SetValue(ViewModelProperty, value); }
    }

    private bool _closeRequested;
    public DockDocumentViewModel(ViewportWindowViewModel viewportViewModel)
    {
        ViewModel = viewportViewModel;
    }


    public override bool OnClose()
    {
        if (!_closeRequested)
        {
            Task.Run(async () =>
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    _closeRequested =
                        await ViewModel.Owner.Owner.DisposeDocumentWithSaveConfirmation(ViewModel.Document);
                });
            });
        }

        return _closeRequested;
    }
}
