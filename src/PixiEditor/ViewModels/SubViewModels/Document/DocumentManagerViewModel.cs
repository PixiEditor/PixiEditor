using System.Collections.ObjectModel;
using System.Windows.Input;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.Views.UserControls.SymmetryOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Document;
#nullable enable
[Command.Group("PixiEditor.Document", "Image")]
internal class DocumentManagerViewModel : SubViewModel<ViewModelMain>
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
            RaisePropertyChanged(nameof(ActiveDocument));
            ActiveDocumentChanged?.Invoke(this, new(value, prevDoc));
            
            if (ViewModelMain.Current.ToolsSubViewModel.ActiveTool == null)
            {
                ViewModelMain.Current.ToolsSubViewModel.SetActiveTool<PenToolViewModel>(false);
            }
        }
    }

    public DocumentManagerViewModel(ViewModelMain owner) : base(owner)
    {
        owner.WindowSubViewModel.ActiveViewportChanged += (_, args) => ActiveDocument = args.Document;
    }

    public void MakeActiveDocumentNull() => ActiveDocument = null;

    [Evaluator.CanExecute("PixiEditor.HasDocument")]
    public bool DocumentNotNull() => ActiveDocument != null;

    [Command.Basic("PixiEditor.Document.ClipCanvas", "Clip Canvas", "Clip Canvas", CanExecute = "PixiEditor.HasDocument")]
    public void ClipCanvas()
    {
        if (ActiveDocument is null)
            return;
        
        ActiveDocument?.Operations.ClipCanvas();
    }
    
    [Command.Basic("PixiEditor.Document.FlipImageHorizontal", "Flip Image Horizontally", "Flip Image Horizontally", CanExecute = "PixiEditor.HasDocument")]
    public void FlipImageHorizontally()
    {
        if (ActiveDocument is null)
            return;
        
        ActiveDocument?.Operations.FlipImage(FlipType.Horizontal);
    }
    
    [Command.Basic("PixiEditor.Document.FlipLayersHorizontal", "Flip Selected Layers Horizontally", "Flip Selected Layers Horizontally", CanExecute = "PixiEditor.HasDocument")]
    public void FlipLayersHorizontally()
    {
        if (ActiveDocument?.SelectedStructureMember == null)
            return;
        
        ActiveDocument?.Operations.FlipImage(FlipType.Horizontal, ActiveDocument.GetSelectedMembers());
    }
    
    [Command.Basic("PixiEditor.Document.FlipImageVertical", "Flip Image Vertically", "Flip Image Vertically", CanExecute = "PixiEditor.HasDocument")]
    public void FlipImageVertically()
    {
        if (ActiveDocument is null)
            return;
        
        ActiveDocument?.Operations.FlipImage(FlipType.Vertical);
    }
    
    [Command.Basic("PixiEditor.Document.FlipLayersVertical", "Flip Selected Layers Vertically", "Flip Selected Layers Vertically", CanExecute = "PixiEditor.HasDocument")]
    public void FlipLayersVertically()
    {
        if (ActiveDocument?.SelectedStructureMember == null)
            return;
        
        ActiveDocument?.Operations.FlipImage(FlipType.Vertical, ActiveDocument.GetSelectedMembers());
    }
    
    [Command.Basic("PixiEditor.Document.Rotate90Deg", "Rotate Image 90 deg", "Rotate Image 90 deg", CanExecute = "PixiEditor.HasDocument")]
    public void Rotate90Deg()
    {
        if (ActiveDocument == null)
            return;
        
        ActiveDocument?.Operations.RotateImage(RotationAngle.D90, ActiveDocument.GetSelectedMembers());
    }
    
    [Command.Basic("PixiEditor.Document.Rotate180Deg", "Rotate Image 180 deg", "Rotate Image 180 deg", CanExecute = "PixiEditor.HasDocument")]
    public void Rotate180Deg()
    {
        if (ActiveDocument == null)
            return;
        
        ActiveDocument?.Operations.RotateImage(RotationAngle.D180, ActiveDocument.GetSelectedMembers());
    }
    
    [Command.Basic("PixiEditor.Document.Rotate270Deg", "Rotate Image 270 deg", "Rotate Image 270 deg", CanExecute = "PixiEditor.HasDocument")]
    public void Rotate270Deg()
    {
        if (ActiveDocument == null)
            return;
        
        ActiveDocument?.Operations.RotateImage(RotationAngle.D270, ActiveDocument.GetSelectedMembers());
    }


    [Command.Basic("PixiEditor.Document.ToggleVerticalSymmetryAxis", "Toggle vertical symmetry axis", "Toggle vertical symmetry axis", CanExecute = "PixiEditor.HasDocument", IconPath = "SymmetryVertical.png")]
    public void ToggleVerticalSymmetryAxis()
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.VerticalSymmetryAxisEnabledBindable ^= true;
    }

    [Command.Basic("PixiEditor.Document.ToggleHorizontalSymmetryAxis", "Toggle horizontal symmetry axis", "Toggle horizontal symmetry axis", CanExecute = "PixiEditor.HasDocument", IconPath = "SymmetryHorizontal.png")]
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

    [Command.Basic("PixiEditor.Document.DeletePixels", "Delete pixels", "Delete selected pixels", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.Delete, IconPath = "Tools/EraserImage.png")]
    public void DeletePixels()
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.Operations.DeleteSelectedPixels();
    }


    [Command.Basic("PixiEditor.Document.ResizeDocument", false, "Resize Document", "Resize Document", CanExecute = "PixiEditor.HasDocument", Key = Key.I, Modifiers = ModifierKeys.Control | ModifierKeys.Shift)]
    [Command.Basic("PixiEditor.Document.ResizeCanvas", true, "Resize Canvas", "Resize Canvas", CanExecute = "PixiEditor.HasDocument", Key = Key.C, Modifiers = ModifierKeys.Control | ModifierKeys.Shift)]
    public void OpenResizePopup(bool canvas)
    {
        DocumentViewModel? doc = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (doc is null)
            return;

        ResizeDocumentDialog dialog = new ResizeDocumentDialog(
            doc.Width,
            doc.Height,
            canvas);
        if (dialog.ShowDialog())
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

    [Command.Basic("PixiEditor.Document.CenterContent", "Center Content", "Center Content", CanExecute = "PixiEditor.HasDocument")]
    public void CenterContent()
    {
        if(ActiveDocument?.SelectedStructureMember == null)
            return;
        
        ActiveDocument.Operations.CenterContent(ActiveDocument.GetSelectedMembers());
    }
}
