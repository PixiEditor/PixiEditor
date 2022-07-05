using System.Collections.ObjectModel;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Events;
using PixiEditor.Models.Tools;

namespace PixiEditor.ViewModels.SubViewModels.Document;
#nullable enable
[Command.Group("PixiEditor.Document", "Image")]
internal class DocumentManagerViewModel : SubViewModel<ViewModelMain>
{
    public DocumentManagerViewModel(ViewModelMain owner) : base(owner)
    {
        /*ToolSessionController = new ToolSessionController();
        ToolSessionController.SessionStarted += OnSessionStart;
        ToolSessionController.SessionEnded += OnSessionEnd;
        ToolSessionController.PixelMousePositionChanged += OnPixelMousePositionChange;
        ToolSessionController.PreciseMousePositionChanged += OnPreciseMousePositionChange;
        ToolSessionController.KeyStateChanged += (_, _) => UpdateActionDisplay(_tools.ActiveTool);*/

        //undo.UndoRedoCalled += (_, _) => ToolSessionController.ForceStopActiveSessionIfAny();
    }

    //private ToolSessionController ToolSessionController { get; set; }
    //public ICanvasInputTarget InputTarget => ToolSessionController;

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
            var prevDoc = activeDocument;
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

    public event EventHandler? StopUsingTool;

    //private readonly ToolsViewModel _tools;

    private ToolSession? activeSession = null;

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

    public void UpdateActionDisplay(Tool tool)
    {
        //tool?.UpdateActionDisplay(ToolSessionController.IsCtrlDown, ToolSessionController.IsShiftDown, ToolSessionController.IsAltDown);
    }

    private void OnSessionStart(object sender, ToolSession e)
    {
        activeSession = e;

        ExecuteTool();
    }

    private void OnSessionEnd(object sender, ToolSession e)
    {
        activeSession = null;

        //HighlightPixels(ToolSessionController.LastPixelPosition);
        StopUsingTool?.Invoke(this, EventArgs.Empty);
    }

    private void OnPreciseMousePositionChange(object sender, (double, double) e)
    {
        if (activeSession == null || !activeSession.Tool.RequiresPreciseMouseData)
            return;
        ExecuteTool();
    }

    private void OnPixelMousePositionChange(object sender, MouseMovementEventArgs e)
    {
        if (activeSession != null)
        {
            if (activeSession.Tool.RequiresPreciseMouseData)
                return;
            ExecuteTool();
            return;
        }
        else
        {
            HighlightPixels(e.NewPosition);
        }
    }

    private void ExecuteTool()
    {
        if (activeSession == null)
            throw new Exception("Can't execute tool's Use outside a session");

        if (activeSession.Tool is BitmapOperationTool operationTool)
        {
            //BitmapOperations.UseTool(activeSession.MouseMovement, operationTool, PrimaryColor);
        }
        else if (activeSession.Tool is ReadonlyTool readonlyTool)
        {
            //readonlyTool.Use(activeSession.MouseMovement);
        }
        else
        {
            throw new InvalidOperationException($"'{activeSession.Tool.GetType().Name}' is either not a Tool or can't inherit '{nameof(Tool)}' directly.\nChanges the base type to either '{nameof(BitmapOperationTool)}' or '{nameof(ReadonlyTool)}'");
        }
    }

    private void BitmapManager_DocumentChanged(object sender)
    {
        /*
        e.NewDocument?.GeneratePreviewLayer();
        if (e.OldDocument != e.NewDocument)
            ToolSessionController.ForceStopActiveSessionIfAny();*/
    }

    public void UpdateHighlightIfNecessary(bool forceHide = false)
    {
        if (activeSession != null)
            return;

        //HighlightPixels(forceHide ? new(-1, -1) : ToolSessionController.LastPixelPosition);
    }

    private void HighlightPixels(VecI position)
    {

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

    [Command.Basic("PixiEditor.Document.DeletePixels", "Delete pixels", "Delete selected pixels", CanExecute = "PixiEditor.Selection.IsNotEmpty", Key = Key.Delete, IconPath = "Tools/EraserImage.png")]
    public void DeletePixels()
    {
        /*
        var doc = Owner.BitmapManager.ActiveDocument;
        Owner.BitmapManager.BitmapOperations.DeletePixels(
            doc.Layers.Where(x => x.IsActive && doc.GetFinalLayerIsVisible(x)).ToArray(),
            doc.ActiveSelection.SelectedPoints.ToArray());
        */
    }


    [Command.Basic("PixiEditor.Document.ResizeDocument", false, "Resize Document", "Resize Document", CanExecute = "PixiEditor.HasDocument", Key = Key.I, Modifiers = ModifierKeys.Control | ModifierKeys.Shift)]
    [Command.Basic("PixiEditor.Document.ResizeCanvas", true, "Resize Canvas", "Resize Canvas", CanExecute = "PixiEditor.HasDocument", Key = Key.C, Modifiers = ModifierKeys.Control | ModifierKeys.Shift)]
    public void OpenResizePopup(bool canvas)
    {
        /*
        ResizeDocumentDialog dialog = new ResizeDocumentDialog(
            Owner.BitmapManager.ActiveDocument.Width,
            Owner.BitmapManager.ActiveDocument.Height,
            canvas);
        if (dialog.ShowDialog())
        {
            if (canvas)
            {
                Owner.BitmapManager.ActiveDocument.ResizeCanvas(dialog.Width, dialog.Height, dialog.ResizeAnchor);
            }
            else
            {
                Owner.BitmapManager.ActiveDocument.Resize(dialog.Width, dialog.Height);
            }
        }
        
*/
    }

    [Command.Basic("PixiEditor.Document.CenterContent", "Center Content", "Center Content", CanExecute = "PixiEditor.HasDocument")]
    public void CenterContent()
    {
        //Owner.BitmapManager.ActiveDocument.CenterContent();
    }

}
