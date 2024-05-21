using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.ViewModels;
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
            RaisePropertyChanged(nameof(FlipX));
        }
    }
    
    private bool _flipY;

    public bool FlipY
    {
        get => _flipY;
        set
        {
            _flipY = value;
            RaisePropertyChanged(nameof(FlipY));
        }
    }

    public ViewportWindowViewModel(WindowViewModel owner, DocumentViewModel document) : base(owner)
    {
        Document = document;
        RequestCloseCommand = new RelayCommand(_ => ViewModelMain.Current?.WindowSubViewModel.OnViewportWindowCloseButtonPressed(this));
    }
}
