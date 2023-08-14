using CommunityToolkit.Mvvm.Input;
using PixiEditor.AvaloniaUI.Helpers.UI;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
#nullable enable
internal class ViewportWindowViewModel : SubViewModel<WindowViewModel>
{
    public DocumentViewModel Document { get; }

    public ExecutionTrigger<VecI> CenterViewportTrigger { get; } = new ExecutionTrigger<VecI>();
    public ExecutionTrigger<double> ZoomViewportTrigger { get; } = new ExecutionTrigger<double>();

    public string Index => Owner.CalculateViewportIndex(this) ?? "";

    public RelayCommand RequestCloseCommand { get; }

    private bool _flipX;

    public bool FlipX
    {
        get => _flipX;
        set
        {
            _flipX = value;
            OnPropertyChanged(nameof(FlipX));
        }
    }
    
    private bool _flipY;

    public bool FlipY
    {
        get => _flipY;
        set
        {
            _flipY = value;
            OnPropertyChanged(nameof(FlipY));
        }
    }

    public void IndexChanged()
    {
        OnPropertyChanged(nameof(Index));
    }

    public ViewportWindowViewModel(WindowViewModel owner, DocumentViewModel document) : base(owner)
    {
        Document = document;
        RequestCloseCommand = new RelayCommand(() => ViewModelMain.Current?.WindowSubViewModel.OnViewportWindowCloseButtonPressed(this));
    }
}
