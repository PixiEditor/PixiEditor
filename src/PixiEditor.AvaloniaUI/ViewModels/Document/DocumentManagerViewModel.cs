using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Evaluators;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.ViewModels.SubViewModels;
using PixiEditor.AvaloniaUI.ViewModels.Tools.Tools;
using PixiEditor.AvaloniaUI.Views;
using PixiEditor.AvaloniaUI.Views.Overlays.SymmetryOverlay;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;
#nullable enable
[Command.Group("PixiEditor.Document", "IMAGE")]
internal class DocumentManagerViewModel : SubViewModel<ViewModelMain>, IDocumentManagerHandler
{
    public ObservableCollection<DocumentViewModel> Documents { get; } = new ObservableCollection<DocumentViewModel>();
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
                ViewModelMain.Current.ToolsSubViewModel.SetActiveTool<PenToolViewModel>(false);
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

    [Command.Basic("PixiEditor.Document.ClipCanvas", "CLIP_CANVAS", "CLIP_CANVAS", CanExecute = "PixiEditor.HasDocument", IconPath = "crop.png",
        MenuItemPath = "IMAGE/CLIP_CANVAS", MenuItemOrder = 2)]
    public void ClipCanvas() => ActiveDocument?.Operations.ClipCanvas();

    [Command.Basic("PixiEditor.Document.FlipImageHorizontal", FlipType.Horizontal, "FLIP_IMG_HORIZONTALLY", "FLIP_IMG_HORIZONTALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/FLIP/FLIP_IMG_HORIZONTALLY", MenuItemOrder = 14)]
    [Command.Basic("PixiEditor.Document.FlipImageVertical", FlipType.Vertical, "FLIP_IMG_VERTICALLY", "FLIP_IMG_VERTICALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/FLIP/FLIP_IMG_VERTICALLY", MenuItemOrder = 15)]
    public void FlipImage(FlipType type) => ActiveDocument?.Operations.FlipImage(type);

    [Command.Basic("PixiEditor.Document.FlipLayersHorizontal", FlipType.Horizontal, "FLIP_LAYERS_HORIZONTALLY", "FLIP_LAYERS_HORIZONTALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/FLIP/FLIP_LAYERS_HORIZONTALLY", MenuItemOrder = 16)]
    [Command.Basic("PixiEditor.Document.FlipLayersVertical", FlipType.Vertical, "FLIP_LAYERS_VERTICALLY", "FLIP_LAYERS_VERTICALLY", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/FLIP/FLIP_LAYERS_VERTICALLY", MenuItemOrder = 17)]
    public void FlipLayers(FlipType type)
    {
        if (ActiveDocument?.SelectedStructureMember == null)
            return;

        ActiveDocument?.Operations.FlipImage(type, ActiveDocument.GetSelectedMembers());
    }

    [Command.Basic("PixiEditor.Document.Rotate90Deg", "ROT_IMG_90",
        "ROT_IMG_90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D90,
        MenuItemPath = "IMAGE/ROTATION/ROT_IMG_90_D", MenuItemOrder = 8)]
    [Command.Basic("PixiEditor.Document.Rotate180Deg", "ROT_IMG_180",
        "ROT_IMG_180", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D180,
        MenuItemPath = "IMAGE/ROTATION/ROT_IMG_180_D", MenuItemOrder = 9)]
    [Command.Basic("PixiEditor.Document.Rotate270Deg", "ROT_IMG_-90",
        "ROT_IMG_-90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D270,
        MenuItemPath = "IMAGE/ROTATION/ROT_IMG_-90_D", MenuItemOrder = 10)]
    public void RotateImage(RotationAngle angle) => ActiveDocument?.Operations.RotateImage(angle);

    [Command.Basic("PixiEditor.Document.Rotate90DegLayers", "ROT_LAYERS_90",
        "ROT_LAYERS_90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D90,
        MenuItemPath = "IMAGE/ROTATION/ROT_LAYERS_90_D", MenuItemOrder = 11)]
    [Command.Basic("PixiEditor.Document.Rotate180DegLayers", "ROT_LAYERS_180",
        "ROT_LAYERS_180", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D180,
        MenuItemPath = "IMAGE/ROTATION/ROT_LAYERS_180_D", MenuItemOrder = 12)]
    [Command.Basic("PixiEditor.Document.Rotate270DegLayers", "ROT_LAYERS_-90",
        "ROT_LAYERS_-90", CanExecute = "PixiEditor.HasDocument", Parameter = RotationAngle.D270,
        MenuItemPath = "IMAGE/ROTATION/ROT_LAYERS_-90_D", MenuItemOrder = 13)]
    public void RotateLayers(RotationAngle angle)
    {
        if (ActiveDocument?.SelectedStructureMember == null)
            return;
        
        ActiveDocument?.Operations.RotateImage(angle, ActiveDocument.GetSelectedMembers());
    }

    [Command.Basic("PixiEditor.Document.ToggleVerticalSymmetryAxis", "TOGGLE_VERT_SYMMETRY_AXIS", "TOGGLE_VERT_SYMMETRY_AXIS", CanExecute = "PixiEditor.HasDocument", IconPath = "SymmetryVertical.png")]
    public void ToggleVerticalSymmetryAxis()
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.VerticalSymmetryAxisEnabledBindable ^= true;
    }

    [Command.Basic("PixiEditor.Document.ToggleHorizontalSymmetryAxis", "TOGGLE_HOR_SYMMETRY_AXIS", "TOGGLE_HOR_SYMMETRY_AXIS", CanExecute = "PixiEditor.HasDocument", IconPath = "SymmetryHorizontal.png")]
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

    [Command.Internal("PixiEditor.Document.StartDragSymmetry", CanExecute = "PixiEditor.HasDocument")]
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

    [Command.Basic("PixiEditor.Document.DeletePixels", "DELETE_PIXELS", "DELETE_PIXELS_DESCRIPTIVE", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.Delete, IconPath = "Tools/EraserImage.png",
        MenuItemPath = "EDIT/DELETE_SELECTED_PIXELS", MenuItemOrder = 6)]
    public void DeletePixels()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.Operations.DeleteSelectedPixels();
    }


    [Command.Basic("PixiEditor.Document.ResizeDocument", false, "RESIZE_DOCUMENT", "RESIZE_DOCUMENT", CanExecute = "PixiEditor.HasDocument", Key = Key.I, Modifiers = KeyModifiers.Control | KeyModifiers.Shift,
        MenuItemPath = "IMAGE/RESIZE_IMAGE", MenuItemOrder = 0)]
    [Command.Basic("PixiEditor.Document.ResizeCanvas", true, "RESIZE_CANVAS", "RESIZE_CANVAS", CanExecute = "PixiEditor.HasDocument", Key = Key.C, Modifiers = KeyModifiers.Control | KeyModifiers.Shift,
        MenuItemPath = "IMAGE/RESIZE_CANVAS", MenuItemOrder = 1)]
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

    [Command.Basic("PixiEditor.Document.CenterContent", "CENTER_CONTENT", "CENTER_CONTENT", CanExecute = "PixiEditor.HasDocument",
        MenuItemPath = "IMAGE/CLIP_CANVAS", MenuItemOrder = 3)]
    public void CenterContent()
    {
        if(ActiveDocument?.SelectedStructureMember == null)
            return;
        
        ActiveDocument.Operations.CenterContent(ActiveDocument.GetSelectedMembers());
    }
}
