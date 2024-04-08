using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiEditor.AvaloniaUI.Helpers.UI;
using PixiEditor.AvaloniaUI.ViewModels.Document;
using PixiEditor.AvaloniaUI.Views.Visuals;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
#nullable enable
internal class ViewportWindowViewModel : SubViewModel<WindowViewModel>, IDockableContent, IDockableCloseEvents
{
    public DocumentViewModel Document { get; }
    public ExecutionTrigger<VecI> CenterViewportTrigger { get; } = new ExecutionTrigger<VecI>();
    public ExecutionTrigger<double> ZoomViewportTrigger { get; } = new ExecutionTrigger<double>();

    public string Index => Owner.CalculateViewportIndex(this) ?? "";

    public RelayCommand RequestCloseCommand { get; }

    public string Id => $"{Document.FileName}{Index}";
    public string Title => $"{Document.FileName}{Index}";
    public bool CanFloat => true;
    public bool CanClose => true;
    public TabCustomizationSettings TabCustomizationSettings { get; } = new(showCloseButton: true);

    private bool _closeRequested;

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
        TabCustomizationSettings.Icon = new SurfaceImage(Document.PreviewSurface);
        RequestCloseCommand = new RelayCommand(() => Owner.OnViewportWindowCloseButtonPressed(this));
    }

    bool IDockableCloseEvents.OnClose()
    {
        if (!_closeRequested)
        {
            Task.Run(async () =>
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    _closeRequested =
                        await Owner.Owner.DisposeDocumentWithSaveConfirmation(Document);
                });
            });
        }

        return _closeRequested;
    }
}
