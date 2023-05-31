using System.ComponentModel;
using System.Windows.Input;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using PixiEditor.Models.Localization;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Document.TransformOverlays;
using PixiEditor.ViewModels.SubViewModels.Tools.ToolSettings.Toolbars;
using PixiEditor.Views.UserControls.Overlays.BrushShapeOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Tools.Tools;

[Command.Tool(Key = Key.O, Transient = Key.LeftAlt)]
internal class ColorPickerToolViewModel : ToolViewModel
{
    private readonly string defaultReferenceActionDisplay = "COLOR_PICKER_ACTION_DISPLAY_DEFAULT";
    
    private readonly string defaultActionDisplay = "COLOR_PICKER_ACTION_DISPLAY_CANVAS_ONLY";
    
    public override bool HideHighlight => true;

    public override string ToolNameLocalizationKey => "COLOR_PICKER_TOOL";
    public override BrushShape BrushShape => BrushShape.Pixel;

    public override LocalizedString Tooltip => new("COLOR_PICKER_TOOLTIP", Shortcut);

    private bool pickFromCanvas = true;
    public bool PickFromCanvas
    {
        get => pickFromCanvas;
        private set
        {
            if (SetProperty(ref pickFromCanvas, value))
            {
                RaisePropertyChanged(nameof(PickOnlyFromReferenceLayer));
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
                RaisePropertyChanged(nameof(PickOnlyFromReferenceLayer));
            }
        }
    }

    public bool PickOnlyFromReferenceLayer => !pickFromCanvas && pickFromReferenceLayer;

    [Settings.Enum("SCOPE_LABEL", DocumentScope.AllLayers)]
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
        if (e.PropertyName is nameof(ReferenceLayerViewModel.ReferenceBitmap) or nameof(ReferenceLayerViewModel.ReferenceShapeBindable))
        {
            UpdateActionDisplay();
        }
    }

    private void UpdateActionDisplay()
    {
        bool ctrlDown = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
        bool shiftDown = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
        
        UpdateActionDisplay(ctrlDown, shiftDown);
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
        
        if (referenceLayer.ReferenceBitmap == null || document.TransformViewModel.TransformActive || !referenceLayer.ReferenceShapeBindable.Intersects(documentBounds))
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

    public override void OnLeftMouseButtonDown(VecD pos)
    {
        ViewModelMain.Current?.DocumentManagerSubViewModel.ActiveDocument?.Tools.UseColorPickerTool();
    }

    public override void ModifierKeyChanged(bool ctrlIsDown, bool shiftIsDown, bool altIsDown) =>
        UpdateActionDisplay(ctrlIsDown, shiftIsDown);
}
