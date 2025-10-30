using System.ComponentModel;
using Avalonia.Media;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using PixiDocks.Core.Docking;
using PixiDocks.Core.Docking.Events;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.DocumentModels;
using Drawie.Numerics;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Document;
using PixiEditor.Views.Visuals;
using Color = Drawie.Backend.Core.ColorsImpl.Color;

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
    public string Title => Document.IsNestedDocument ? new LocalizedString("NESTED_DOCUMENT") :$"{Document.FileName}{Index}";
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

    private bool autoScaleBackground = true;

    public bool AutoScaleBackground
    {
        get => autoScaleBackground;
        set
        {
            autoScaleBackground = value;
            OnPropertyChanged(nameof(AutoScaleBackground));
        }
    }

    private double customBackgroundScaleX = 16;

    public double CustomBackgroundScaleX
    {
        get => customBackgroundScaleX;
        set
        {
            customBackgroundScaleX = value;
            OnPropertyChanged(nameof(CustomBackgroundScaleX));
        }
    }

    private double customBackgroundScaleY = 16;

    public double CustomBackgroundScaleY
    {
        get => customBackgroundScaleY;
        set
        {
            customBackgroundScaleY = value;
            OnPropertyChanged(nameof(CustomBackgroundScaleY));
        }
    }

    private Bitmap backgroundBitmap;

    public Bitmap BackgroundBitmap
    {
        get => backgroundBitmap;
        set
        {
            backgroundBitmap = value;
            OnPropertyChanged(nameof(BackgroundBitmap));
        }
    }

    private TextureControl previewPainterControl;

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

        AutoScaleBackground = PixiEditorSettings.Scene.AutoScaleBackground.Value;
        CustomBackgroundScaleX = PixiEditorSettings.Scene.CustomBackgroundScaleX.Value;
        CustomBackgroundScaleY = PixiEditorSettings.Scene.CustomBackgroundScaleY.Value;
        BackgroundBitmap = BitmapFromColors(
            PixiEditorSettings.Scene.PrimaryBackgroundColor.Value,
            PixiEditorSettings.Scene.SecondaryBackgroundColor.Value);

        PixiEditorSettings.Scene.AutoScaleBackground.ValueChanged += UpdateAutoScaleBackground;
        PixiEditorSettings.Scene.CustomBackgroundScaleX.ValueChanged += UpdateCustomBackgroundScaleX;
        PixiEditorSettings.Scene.CustomBackgroundScaleY.ValueChanged += UpdateCustomBackgroundScaleY;
        PixiEditorSettings.Scene.PrimaryBackgroundColor.ValueChanged += UpdateBackgroundBitmap;
        PixiEditorSettings.Scene.SecondaryBackgroundColor.ValueChanged += UpdateBackgroundBitmap;

        previewPainterControl = new TextureControl();
        var nonZoomed = Document.SceneTextures.Where(x =>
            x.Value is { DrawingSurface.Canvas.TotalMatrix: { TransX: 0, TransY: 0, SkewX: 0, SkewY: 0 } }).ToArray();
        if (nonZoomed.Length > 0)
        {
            var minSize = nonZoomed.MinBy(x => x.Value.Size);
            if (minSize.Value != null)
            {
                previewPainterControl.Texture = minSize.Value;
            }
        }

        TabCustomizationSettings.Icon = previewPainterControl;
        TabCustomizationSettings.FontStyle = Document.IsNestedDocument ? FontStyle.Italic : FontStyle.Normal;
    }


    private void DocumentOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DocumentViewModel.FileName))
        {
            OnPropertyChanged(nameof(Title));
        }
        else if (e.PropertyName == nameof(DocumentViewModel.AllChangesSaved))
        {
            var nonZoomed = Document.SceneTextures.Where(x =>
                    x.Value is { DrawingSurface.Canvas.TotalMatrix: { TransX: 0, TransY: 0, SkewX: 0, SkewY: 0 } })
                .ToArray();
            if (nonZoomed.Length > 0)
            {
                var minSize = nonZoomed.MinBy(x => x.Value.Size);
                if (minSize.Value != null)
                {
                    previewPainterControl.Texture = minSize.Value;
                }
            }
            TabCustomizationSettings.SavedState = GetSaveState(Document);
        }
        else if (e.PropertyName == nameof(DocumentViewModel.AllChangesAutosaved))
        {
            TabCustomizationSettings.SavedState = GetSaveState(Document);
        }
        else if (e.PropertyName == nameof(DocumentViewModel.IsNestedDocument))
        {
            TabCustomizationSettings.FontStyle = Document.IsNestedDocument ? FontStyle.Italic : FontStyle.Normal;
            OnPropertyChanged(nameof(Title));
        }
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
                    if (_closeRequested)
                    {
                        Document.SizeChanged -= DocumentOnSizeChanged;
                        Document.PropertyChanged -= DocumentOnPropertyChanged;

                        PixiEditorSettings.Scene.AutoScaleBackground.ValueChanged -= UpdateAutoScaleBackground;
                        PixiEditorSettings.Scene.CustomBackgroundScaleX.ValueChanged -= UpdateCustomBackgroundScaleX;
                        PixiEditorSettings.Scene.CustomBackgroundScaleY.ValueChanged -= UpdateCustomBackgroundScaleY;
                        PixiEditorSettings.Scene.PrimaryBackgroundColor.ValueChanged -= UpdateBackgroundBitmap;
                        PixiEditorSettings.Scene.SecondaryBackgroundColor.ValueChanged -= UpdateBackgroundBitmap;
                    }
                });
            });
        }

        return _closeRequested;
    }

    private void UpdateAutoScaleBackground(Setting<bool> setting, bool newValue)
    {
        AutoScaleBackground = newValue;
    }

    private void UpdateCustomBackgroundScaleX(Setting<double> setting, double newValue)
    {
        CustomBackgroundScaleX = newValue;
    }

    private void UpdateCustomBackgroundScaleY(Setting<double> setting, double newValue)
    {
        CustomBackgroundScaleY = newValue;
    }

    public VecI GetRenderOutputSize()
    {
        return Document.GetRenderOutputSize(RenderOutputName);
    }

    private void UpdateBackgroundBitmap(Setting<string> setting, string newValue)
    {
        BackgroundBitmap?.Dispose();
        BackgroundBitmap = BitmapFromColors(
            PixiEditorSettings.Scene.PrimaryBackgroundColor.Value,
            PixiEditorSettings.Scene.SecondaryBackgroundColor.Value);
    }

    private static Bitmap BitmapFromColors(string primaryHex, string secondaryHex)
    {
        Color primary = Color.FromHex(primaryHex);
        Color secondary = Color.FromHex(secondaryHex);

        Surface surface = Surface.ForDisplay(new VecI(2, 2));
        surface.DrawingSurface.Canvas.Clear(primary);
        using Paint secondaryPaint = new Paint { Color = secondary, Style = PaintStyle.Fill };
        surface.DrawingSurface.Canvas.DrawRect(1, 0, 1, 1, secondaryPaint);
        surface.DrawingSurface.Canvas.DrawRect(0, 1, 1, 1, secondaryPaint);

        using var snapshot = surface.DrawingSurface.Snapshot();
        return Bitmap.FromImage(snapshot);
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
