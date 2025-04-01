using System.ComponentModel;
using Avalonia.Input;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Handlers.Tools;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.TransformOverlays;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.Tools.Tools;

[Command.Tool(Key = Key.O, Transient = Key.LeftAlt)]
internal class ColorPickerToolViewModel : ToolViewModel, IColorPickerHandler
{
    private readonly string defaultReferenceActionDisplay = "COLOR_PICKER_ACTION_DISPLAY_DEFAULT";

    private readonly string defaultActionDisplay = "COLOR_PICKER_ACTION_DISPLAY_CANVAS_ONLY";

    public override Type LayerTypeToCreateOnEmptyUse { get; } = null;
    public override bool HideHighlight => true;

    public override bool UsesColor => true;

    public override string ToolNameLocalizationKey => "COLOR_PICKER_TOOL";
    public override BrushShape FinalBrushShape => BrushShape.Pixel;

    public override string DefaultIcon => PixiPerfectIcons.Picker;

    public override Type[]? SupportedLayerTypes { get; } = null;  // all layer types are supported

    public override LocalizedString Tooltip => new("COLOR_PICKER_TOOLTIP", Shortcut);

    private bool pickFromCanvas = true;

    public bool PickFromCanvas
    {
        get => pickFromCanvas;
        private set
        {
            if (SetProperty(ref pickFromCanvas, value))
            {
                OnPropertyChanged(nameof(PickOnlyFromReferenceLayer));
            }
        }
    }

    private bool pickFromReferenceLayer = true;

    public bool PickFromReferenceLayer
    {
        get => pickFromReferenceLayer;
        private set
        {
            if (SetProperty(ref pickFromReferenceLayer, value))
            {
                OnPropertyChanged(nameof(PickOnlyFromReferenceLayer));
            }
        }
    }

    public bool PickOnlyFromReferenceLayer => !pickFromCanvas && pickFromReferenceLayer;

    [Settings.Enum("SCOPE_LABEL", DocumentScope.Canvas)]
    public DocumentScope Mode => GetValue<DocumentScope>();

    public ColorPickerToolViewModel()
    {
        ActionDisplay = defaultActionDisplay;
        Toolbar = ToolbarFactory.Create<ColorPickerToolViewModel, EmptyToolbar>(this);
        ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocumentChanged += DocumentChanged;
    }

    private void DocumentChanged(object sender, DocumentChangedEventArgs e)
    {
        if (e.OldDocument != null)
        {
            e.OldDocument.ReferenceLayerViewModel.PropertyChanged -= ReferenceLayerChanged;
            e.OldDocument.TransformViewModel.PropertyChanged -= TransformViewModelOnPropertyChanged;
        }

        if (e.NewDocument != null)
        {
            e.NewDocument.ReferenceLayerViewModel.PropertyChanged += ReferenceLayerChanged;
            e.NewDocument.TransformViewModel.PropertyChanged += TransformViewModelOnPropertyChanged;
        }

        UpdateActionDisplay();
    }

    private void TransformViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(DocumentTransformViewModel.TransformActive))
        {
            UpdateActionDisplay();
        }
    }

    private void ReferenceLayerChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ReferenceLayerViewModel.ReferenceTexture)
            or nameof(ReferenceLayerViewModel.ReferenceShapeBindable))
        {
            UpdateActionDisplay();
        }
    }

    private void UpdateActionDisplay()
    {
        // TODO: We probably need to create keyboard service to handle this
        /*bool ctrlDown = (Keyboard.Modifiers & KeyModifiers.Control) != 0;
        bool shiftDown = (Keyboard.Modifiers & KeyModifiers.Shift) != 0;*/

        UpdateActionDisplay(false, false);
    }

    private void UpdateActionDisplay(bool ctrlIsDown, bool shiftIsDown)
    {
        var document = ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument;

        if (document == null)
        {
            return;
        }

        var documentBounds = new RectD(default, document.SizeBindable);
        var referenceLayer = document.ReferenceLayerViewModel;

        if (referenceLayer.ReferenceTexture == null || document.TransformViewModel.TransformActive ||
            !referenceLayer.ReferenceShapeBindable.Intersects(documentBounds))
        {
            PickFromCanvas = true;
            PickFromReferenceLayer = true;
            ActionDisplay = defaultActionDisplay;
            return;
        }

        if (ctrlIsDown)
        {
            PickFromCanvas = false;
            PickFromReferenceLayer = true;
            ActionDisplay = "COLOR_PICKER_ACTION_DISPLAY_REFERENCE_ONLY";
        }
        else if (shiftIsDown)
        {
            PickFromCanvas = true;
            PickFromReferenceLayer = false;
            ActionDisplay = "COLOR_PICKER_ACTION_DISPLAY_CANVAS_ONLY";
            return;
        }
        else
        {
            PickFromCanvas = true;
            PickFromReferenceLayer = true;
            ActionDisplay = defaultReferenceActionDisplay;
        }

        referenceLayer.RaiseShowHighestChanged();
    }

    public override void UseTool(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseColorPickerTool();
    }

    public override void KeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown, Key argsKey) =>
        UpdateActionDisplay(ctrlIsDown, shiftIsDown);
}
