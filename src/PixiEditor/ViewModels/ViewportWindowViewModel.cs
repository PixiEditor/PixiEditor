using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.ViewModels;
#nullable enable
internal class ViewportWindowViewModel : NotifyableObject
{
    public DocumentViewModel Document { get; }

    public ExecutionTrigger<VecI> CenterViewportTrigger { get; } = new ExecutionTrigger<VecI>();
    public ExecutionTrigger<double> ZoomViewportTrigger { get; } = new ExecutionTrigger<double>();

    public string Index => ViewModelMain.Current?.WindowSubViewModel.CalculateViewportIndex(this) ?? "";

    public RelayCommand RequestCloseCommand { get; }

    public ViewportWindowViewModel(DocumentViewModel document)
    {
        Document = document;
        RequestCloseCommand = new RelayCommand(_ => ViewModelMain.Current?.WindowSubViewModel.OnViewportWindowCloseButtonPressed(this));
    }
}
