using System.Collections.ObjectModel;
using System.Windows.Input;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Events;
using PixiEditor.ViewModels.SubViewModels.Tools;
using PixiEditor.Views.UserControls.SymmetryOverlay;

namespace PixiEditor.ViewModels.SubViewModels.Document;
#nullable enable
[Command.Group("PixiEditor.Document", "Image")]
internal class DocumentManagerViewModel : SubViewModel<ViewModelMain>
{
    public DocumentManagerViewModel(ViewModelMain owner) : base(owner) { }

    public ObservableCollection<DocumentViewModel> Documents { get; set; } = new ObservableCollection<DocumentViewModel>();
    public event EventHandler<DocumentChangedEventArgs>? ActiveDocumentChanged;


    private DocumentViewModel? activeDocument;
    public DocumentViewModel? ActiveDocument
    {
        get => activeDocument;
        set
        {
            if (activeDocument == value)
                return;
            DocumentViewModel? prevDoc = activeDocument;
            activeDocument = value;
            RaisePropertyChanged(nameof(ActiveDocument));
            ActiveDocumentChanged?.Invoke(this, new(value, prevDoc));
            ActiveWindow = value;
        }
    }

    private object? activeWindow;
    public object? ActiveWindow
    {
        get => activeWindow;
        set
        {
            if (activeWindow == value)
                return;
            activeWindow = value;
            RaisePropertyChanged(nameof(ActiveWindow));
            if (activeWindow is DocumentViewModel doc)
                ActiveDocument = doc;
        }
    }

    [Evaluator.CanExecute("PixiEditor.HasDocument")]
    public bool DocumentNotNull() => ActiveDocument != null;

    public void CloseDocument(Guid documentGuid)
    {
        /*
        int nextIndex = 0;
        if (document == ActiveDocument)
        {
            nextIndex = Documents.Count > 1 ? Documents.IndexOf(document) : -1;
            nextIndex += nextIndex > 0 ? -1 : 0;
        }

        Documents.Remove(document);
        ActiveDocument = nextIndex >= 0 ? Documents[nextIndex] : null;
        */
    }

    public void UpdateActionDisplay(ToolViewModel tool)
    {
        //tool?.UpdateActionDisplay(ToolSessionController.IsCtrlDown, ToolSessionController.IsShiftDown, ToolSessionController.IsAltDown);
    }

    [Command.Basic("PixiEditor.Document.ClipCanvas", "Clip Canvas", "Clip Canvas", CanExecute = "PixiEditor.HasDocument")]
    public void ClipCanvas()
    {
        //Owner.BitmapManager.ActiveDocument?.ClipCanvas();
    }

    /*
    public void RequestCloseDocument(Document document)
    {
        /*
        if (!document.ChangesSaved)
        {
            ConfirmationType result = ConfirmationDialog.Show(ConfirmationDialogMessage, ConfirmationDialogTitle);
            if (result == ConfirmationType.Yes)
            {
                Owner.FileSubViewModel.SaveDocument(false);
                if (!document.ChangesSaved)
                    return;
            }
            else if (result == ConfirmationType.Canceled)
            {
                return;
            }
        }

        Owner.BitmapManager.CloseDocument(document);
        
    }
*/
    [Command.Basic("PixiEditor.Document.ToggleVerticalSymmetryAxis", "Toggle vertical symmetry axis", "Toggle vertical symmetry axis", CanExecute = "PixiEditor.HasDocument")]
    public void ToggleVerticalSymmetryAxis()
    {
        if (ActiveDocument is null)
            return;
        ActiveDocument.VerticalSymmetryAxisEnabledBindable ^= true;
    }

    [Command.Basic("PixiEditor.Document.ToggleHorizontalSymmetryAxis", "Toggle horizontal symmetry axis", "Toggle horizontal symmetry axis", CanExecute = "PixiEditor.HasDocument")]
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
        //Owner.BitmapManager.ActiveDocument.CenterContent();
    }

}
