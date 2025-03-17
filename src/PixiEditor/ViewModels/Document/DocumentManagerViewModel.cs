using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Input;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Handlers;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.Tools.Tools;
using PixiEditor.Views;
using PixiEditor.Views.Overlays.SymmetryOverlay;

namespace PixiEditor.ViewModels.Document;
#nullable enable
[Command.Group("PixiEditor.Document", "IMAGE")]
internal class DocumentManagerViewModel : SubViewModel<ViewModelMain>, IDocumentManagerHandler
{
    public ObservableCollection<DocumentViewModel> Documents { get; } = new ObservableCollection<DocumentViewModel>();
    public ObservableCollection<LazyDocumentViewModel> LazyDocuments { get; } = new ObservableCollection<LazyDocumentViewModel>();
    public event EventHandler<DocumentChangedEventArgs>? ActiveDocumentChanged;

    private DocumentViewModel? activeDocument;

    public DocumentViewModel? ActiveDocument
    {
        get => activeDocument;
        // Use WindowSubViewModel.MakeDocumentViewportActive(document);
        private set
        {
            if (activeDocument == value)
                return;
            DocumentViewModel? prevDoc = activeDocument;
            activeDocument = value;
            OnPropertyChanged(nameof(ActiveDocument));
            ActiveDocumentChanged?.Invoke(this, new(value, prevDoc));

            if (ViewModelMain.Current.ToolsSubViewModel.ActiveTool == null)
            {
                var firstTool =
                    ViewModelMain.Current.ToolsSubViewModel.ActiveToolSet.Tools.FirstOrDefault(x =>
                        x.CanBeUsedOnActiveLayer);
                if (firstTool != null)
                {
                    ViewModelMain.Current.ToolsSubViewModel.SetActiveTool(firstTool.GetType(), false);
                }
            }
        }
    }

    IDocument? IDocumentManagerHandler.ActiveDocument
    {
        get => ActiveDocument;
        set => ActiveDocument = (DocumentViewModel)value;
    }

    public bool HasActiveDocument => ActiveDocument != null;

    public DocumentManagerViewModel(ViewModelMain owner) : base(owner)
    {
        owner.WindowSubViewModel.ActiveViewportChanged += (_, args) =>
        {
            ActiveDocument = args.Document;
        };
    }

    public void MakeActiveDocumentNull() => ActiveDocument = null;

    [Evaluator.CanExecute("PixiEditor.HasDocument", nameof(ActiveDocument))]
    public bool DocumentNotNull() => ActiveDocument != null;

    [Command.Basic("PixiEditor.Document.ClipCanvas", "CLIP_CANVAS", "CLIP_CANVAS",
        CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.Crop, MenuItemPath = "IMAGE/CLIP_CANVAS", MenuItemOrder = 2, AnalyticsTrack = true)]
    public void ClipCanvas() => ActiveDocument?.Operations.ClipCanvas();

    [Command.Basic("PixiEditor.Document.FlipImageHorizontal", FlipType.Horizontal, "FLIP_IMG_HORIZONTALLY",
        "FLIP_IMG_HORIZONTALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/FLIP/FLIP_IMG_HORIZONTALLY", MenuItemOrder = 14, Icon = PixiPerfectIcons.XFlip,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.FlipImageVertical", FlipType.Vertical, "FLIP_IMG_VERTICALLY",
        "FLIP_IMG_VERTICALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/FLIP/FLIP_IMG_VERTICALLY", MenuItemOrder = 15, Icon = PixiPerfectIcons.YFlip,
        AnalyticsTrack = true)]
    public void FlipImage(FlipType type) =>
        ActiveDocument?.Operations.FlipImage(type, activeDocument.AnimationDataViewModel.ActiveFrameBindable);

    [Command.Basic("PixiEditor.Document.FlipLayersHorizontal", FlipType.Horizontal, "FLIP_LAYERS_HORIZONTALLY",
        "FLIP_LAYERS_HORIZONTALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/FLIP/FLIP_LAYERS_HORIZONTALLY", MenuItemOrder = 16, Icon = PixiPerfectIcons.XSelectedFlip,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.FlipLayersVertical", FlipType.Vertical, "FLIP_LAYERS_VERTICALLY",
        "FLIP_LAYERS_VERTICALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/FLIP/FLIP_LAYERS_VERTICALLY", MenuItemOrder = 17, Icon = PixiPerfectIcons.YSelectedFlip,
        AnalyticsTrack = true)]
    public void FlipLayers(FlipType type)
    {
        if (ActiveDocument?.SelectedStructureMember == null)
            return;

        ActiveDocument?.Operations.FlipImage(type, ActiveDocument.GetSelectedMembers(),
            activeDocument.AnimationDataViewModel.ActiveFrameBindable);
    }

    [Command.Basic("PixiEditor.Document.Rotate90Deg", "ROT_IMG_90",
        "ROT_IMG_90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D90,
        MenuItemPath = "IMAGE/ROTATION/ROT_IMG_90_D", MenuItemOrder = 8, Icon = PixiPerfectIcons.RotateImage90,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.Rotate180Deg", "ROT_IMG_180",
        "ROT_IMG_180", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D180,
        MenuItemPath = "IMAGE/ROTATION/ROT_IMG_180_D", MenuItemOrder = 9, Icon = PixiPerfectIcons.RotateImage180,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.Rotate270Deg", "ROT_IMG_-90",
        "ROT_IMG_-90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D270,
        MenuItemPath = "IMAGE/ROTATION/ROT_IMG_-90_D", MenuItemOrder = 10, Icon = PixiPerfectIcons.RotateImageMinus90,
        AnalyticsTrack = true)]
    public void RotateImage(RotationAngle angle) => ActiveDocument?.Operations.RotateImage(angle);

    [Command.Basic("PixiEditor.Document.Rotate90DegLayers", "ROT_LAYERS_90",
        "ROT_LAYERS_90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D90,
        MenuItemPath = "IMAGE/ROTATION/ROT_LAYERS_90_D", MenuItemOrder = 11, Icon = PixiPerfectIcons.RotateFile90,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.Rotate180DegLayers", "ROT_LAYERS_180",
        "ROT_LAYERS_180", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D180,
        MenuItemPath = "IMAGE/ROTATION/ROT_LAYERS_180_D", MenuItemOrder = 12, Icon = PixiPerfectIcons.RotateFile180,
        AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.Rotate270DegLayers", "ROT_LAYERS_-90",
        "ROT_LAYERS_-90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D270,
        MenuItemPath = "IMAGE/ROTATION/ROT_LAYERS_-90_D", MenuItemOrder = 13, Icon = PixiPerfectIcons.RotateFileMinus90,
        AnalyticsTrack = true)]
    public void RotateLayers(RotationAngle angle)
    {
        if (ActiveDocument?.SelectedStructureMember == null)
            return;

        ActiveDocument?.Operations.RotateImage(angle, ActiveDocument.GetSelectedMembers(),
            activeDocument.AnimationDataViewModel.ActiveFrameBindable);
    }

    [Command.Basic("PixiEditor.Document.ToggleVerticalSymmetryAxis", "TOGGLE_VERT_SYMMETRY_AXIS",
        "TOGGLE_VERT_SYMMETRY_AXIS", CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.YSymmetry, AnalyticsTrack = true)]
    public void ToggleVerticalSymmetryAxis()
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.VerticalSymmetryAxisEnabledBindable ^= true;
    }

    [Command.Basic("PixiEditor.Document.ToggleHorizontalSymmetryAxis", "TOGGLE_HOR_SYMMETRY_AXIS",
        "TOGGLE_HOR_SYMMETRY_AXIS", CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.XSymmetry, AnalyticsTrack = true)]
    public void ToggleHorizontalSymmetryAxis()
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.HorizontalSymmetryAxisEnabledBindable ^= true;
    }

    [Command.Internal("PixiEditor.Document.DragSymmetry", CanExecute = "PixiEditor.HasDocument")]
    public void DragSymmetry(SymmetryAxisDragInfo info)
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.EventInlet.OnSymmetryDragged(info);
    }

    [Command.Internal("PixiEditor.Document.StartDragSymmetry", CanExecute = "PixiEditor.HasDocument",
        AnalyticsTrack = true)]
    public void StartDragSymmetry(SymmetryAxisDirection dir)
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.EventInlet.OnSymmetryDragStarted(dir);
        ActiveDocument.Tools.UseSymmetry(dir);
    }

    [Command.Internal("PixiEditor.Document.EndDragSymmetry", CanExecute = "PixiEditor.HasDocument")]
    public void EndDragSymmetry(SymmetryAxisDirection dir)
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.EventInlet.OnSymmetryDragEnded(dir);
    }

    [Command.Basic("PixiEditor.Document.DeletePixels", "DELETE_PIXELS", "DELETE_PIXELS_DESCRIPTIVE",
        CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.Delete,
        ShortcutContexts = [typeof(ViewportWindowViewModel)],
        Icon = PixiPerfectIcons.Eraser,
        MenuItemPath = "EDIT/DELETE_SELECTED_PIXELS", MenuItemOrder = 6, AnalyticsTrack = true)]
    public void DeletePixels()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.Operations.DeleteSelectedPixels(activeDocument
            .AnimationDataViewModel.ActiveFrameBindable);
    }


    [Command.Basic("PixiEditor.Document.ResizeDocument", false, "RESIZE_DOCUMENT", "RESIZE_DOCUMENT",
        CanExecute = "PixiEditor.HasDocument", Key = Key.I, Modifiers = KeyModifiers.Control | KeyModifiers.Shift,
        Icon = PixiPerfectIcons.Resize, MenuItemPath = "IMAGE/RESIZE_IMAGE", MenuItemOrder = 0, AnalyticsTrack = true)]
    [Command.Basic("PixiEditor.Document.ResizeCanvas", true, "RESIZE_CANVAS", "RESIZE_CANVAS",
        CanExecute = "PixiEditor.HasDocument", Key = Key.C, Modifiers = KeyModifiers.Control | KeyModifiers.Shift,
        Icon = PixiPerfectIcons.CanvasResize, MenuItemPath = "IMAGE/RESIZE_CANVAS", MenuItemOrder = 1,
        AnalyticsTrack = true)]
    public async Task OpenResizePopup(bool canvas)
    {
        DocumentViewModel? doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        ResizeDocumentDialog dialog = new ResizeDocumentDialog(
            doc.Width,
            doc.Height,
            MainWindow.Current!,
            canvas);
        if (await dialog.ShowDialog())
        {
            if (canvas)
            {
                doc.Operations.ResizeCanvas(new(dialog.Width, dialog.Height), dialog.ResizeAnchor);
            }
            else
            {
                doc.Operations.ResizeImage(new(dialog.Width, dialog.Height), ResamplingMethod.NearestNeighbor);
            }
        }
    }

    [Command.Basic("PixiEditor.Document.CenterContent", "CENTER_CONTENT", "CENTER_CONTENT",
        CanExecute = "PixiEditor.HasDocument",
        Icon = PixiPerfectIcons.Center, MenuItemPath = "IMAGE/CENTER_CONTENT", MenuItemOrder = 3,
        AnalyticsTrack = true)]
    public void CenterContent()
    {
        if (ActiveDocument?.SelectedStructureMember == null)
            return;

        ActiveDocument.Operations.CenterContent(ActiveDocument.GetSelectedMembers(),
            activeDocument.AnimationDataViewModel.ActiveFrameBindable);
    }

    [Command.Basic("PixiEditor.Document.UseLinearSrgbProcessing", "USE_LINEAR_SRGB_PROCESSING",
        "USE_LINEAR_SRGB_PROCESSING_DESC", CanExecute = "PixiEditor.DocumentUsesSrgbBlending",
        AnalyticsTrack = true)]
    public void UseLinearSrgbProcessing()
    {
        if (ActiveDocument is null)
            return;

        ActiveDocument.Operations.UseLinearSrgbProcessing();
    }

    [Command.Basic("PixiEditor.Document.UseSrgbProcessing", "USE_SRGB_PROCESSING",
        "USE_SRGB_PROCESSING_DESC", CanExecute = "PixiEditor.DocumentUsesLinearBlending",
        AnalyticsTrack = true)]
    public void UseSrgbProcessing()
    {
        if (ActiveDocument is null)
            return;

        ActiveDocument.Operations.UseSrgbProcessing();
    }

    [Command.Internal("PixiEditor.Document.LoadLazyDocument")]
    public void LoadLazyDocument(LazyDocumentViewModel lazyDocument)
    {
        Owner.FileSubViewModel.LoadLazyDocument(lazyDocument);
        LazyDocuments.Remove(lazyDocument);
        Owner.WindowSubViewModel.CloseViewportForLazyDocument(lazyDocument);
    }

    [Evaluator.CanExecute("PixiEditor.DocumentUsesSrgbBlending", nameof(ActiveDocument),
        nameof(ActiveDocument.UsesSrgbBlending))]
    public bool DocumentUsesSrgbBlending() => ActiveDocument?.UsesSrgbBlending ?? false;

    [Evaluator.CanExecute("PixiEditor.DocumentUsesLinearBlending", nameof(ActiveDocument),
        nameof(ActiveDocument.UsesSrgbBlending))]
    public bool DocumentUsesLinearBlending() => !ActiveDocument?.UsesSrgbBlending ?? true;
}
