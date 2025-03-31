using System.ComponentModel;
using Avalonia.Threading;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.DocumentModels;
using Drawie.Numerics;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Document;
using PixiEditor.Views.Visuals;

namespace PixiEditor.ViewModels.SubViewModels;
#nullable enable
internal class ViewportWindowViewModel : SubViewModel<WindowViewModel>, IDockableContent, IDockableCloseEvents,
    IDockableSelectionEvents, IViewport
{
    public DocumentViewModel Document { get; }
    public ExecutionTrigger<VecI> CenterViewportTrigger { get; } = new ExecutionTrigger<VecI>();
    public ExecutionTrigger<double> ZoomViewportTrigger { get; } = new ExecutionTrigger<double>();


    public string Index => _index;

    public string Id => id;
    public string Title => $"{Document.FileName}{Index}";
    public bool CanFloat => true;
    public bool CanClose => true;

    public DocumentTabCustomizationSettings TabCustomizationSettings { get; } =
        new DocumentTabCustomizationSettings(showCloseButton: true);

    TabCustomizationSettings IDockableContent.TabCustomizationSettings => TabCustomizationSettings;

    private bool _closeRequested;
    private string _index = "";

    private bool _flipX;
    private string renderOutputName = "DEFAULT";
    private string id = Guid.NewGuid().ToString();

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

    public string RenderOutputName
    {
        get => renderOutputName;
        set
        {
            renderOutputName = value;
            OnPropertyChanged(nameof(RenderOutputName));
        }
    }

    private ViewportColorChannels _channels = ViewportColorChannels.Default;

    public ViewportColorChannels Channels
    {
        get => _channels;
        set => SetProperty(ref _channels, value);
    }

    private bool hudVisible = true;

    public bool HudVisible
    {
        get => hudVisible;
        set
        {
            hudVisible = value;
            OnPropertyChanged(nameof(HudVisible));
        }
    }

    private PreviewPainterControl previewPainterControl;

    public void IndexChanged()
    {
        _index = Owner.CalculateViewportIndex(this) ?? "";
        OnPropertyChanged(nameof(Index));
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(Id));
    }

    public ViewportWindowViewModel(WindowViewModel owner, DocumentViewModel document) : base(owner)
    {
        Document = document;
        Document.SizeChanged += DocumentOnSizeChanged;
        Document.PropertyChanged += DocumentOnPropertyChanged;
        previewPainterControl = new PreviewPainterControl(Document.PreviewPainter,
            Document.AnimationDataViewModel.ActiveFrameTime.Frame);
        TabCustomizationSettings.Icon = previewPainterControl;
    }

    private void DocumentOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DocumentViewModel.FileName))
        {
            OnPropertyChanged(nameof(Title));
        }
        else if (e.PropertyName == nameof(DocumentViewModel.PreviewPainter))
        {
            previewPainterControl.PreviewPainter = Document.PreviewPainter;
            previewPainterControl.FrameToRender = Document.AnimationDataViewModel.ActiveFrameTime.Frame;
        }
        else if (e.PropertyName == nameof(DocumentViewModel.AllChangesSaved))
        {
            TabCustomizationSettings.SavedState = GetSaveState(Document);
        }
        else if (e.PropertyName == nameof(DocumentViewModel.AllChangesAutosaved))
        {
            TabCustomizationSettings.SavedState = GetSaveState(Document);
        }
    }

    ~ViewportWindowViewModel()
    {
        Document.SizeChanged -= DocumentOnSizeChanged;
        Document.PropertyChanged -= DocumentOnPropertyChanged;
    }

    private void DocumentOnSizeChanged(object? sender, DocumentSizeChangedEventArgs e)
    {
        previewPainterControl.QueueNextFrame();
        OnPropertyChanged(nameof(TabCustomizationSettings));
    }

    async Task<bool> IDockableCloseEvents.OnClose()
    {
        if (!_closeRequested)
        {
            await Task.Run(async () =>
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    _closeRequested =
                        await Owner.OnViewportWindowCloseButtonPressed(this);
                });
            });
        }

        return _closeRequested;
    }

    private static SavedState GetSaveState(DocumentViewModel document)
    {
        if (document.AllChangesSaved)
        {
            return SavedState.Saved;
        }

        if (document.AllChangesAutosaved)
        {
            return SavedState.Autosaved;
        }

        return SavedState.Unsaved;
    }

    void IDockableSelectionEvents.OnSelected()
    {
        Owner.ActiveWindow = this;
        Owner.Owner.ShortcutController.OverwriteContext(this.GetType());
    }

    void IDockableSelectionEvents.OnDeselected()
    {
        Owner.Owner.ShortcutController.ClearContext(GetType());
    }
}
