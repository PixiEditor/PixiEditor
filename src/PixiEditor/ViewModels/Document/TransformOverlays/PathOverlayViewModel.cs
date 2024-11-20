using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.Vector;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

internal class PathOverlayViewModel : ObservableObject, IPathOverlayHandler
{
    private DocumentViewModel documentViewModel;
    private DocumentInternalParts internals;

    private PathOverlayUndoStack<VectorPath>? undoStack = null;

    private VectorPath path;

    public VectorPath Path
    {
        get => path;
        set
        {
            var old = path;
            if (SetProperty(ref path, value))
            {
                if (old != null)
                {
                    old.Changed -= PathDataChanged;
                }

                if (value != null)
                {
                    value.Changed += PathDataChanged;
                }

                PathChanged?.Invoke(value);
            }
        }
    }

    public event Action<VectorPath>? PathChanged;
    public bool IsActive { get; set; }
    public bool HasUndo => undoStack.UndoCount > 0;
    public bool HasRedo => undoStack.RedoCount > 0;

    public RelayCommand<VectorPath> AddToUndoCommand { get; }

    private bool suppressUndo = false;

    public PathOverlayViewModel(DocumentViewModel documentViewModel, DocumentInternalParts internals)
    {
        this.documentViewModel = documentViewModel;
        this.internals = internals;

        AddToUndoCommand = new RelayCommand<VectorPath>(AddToUndo);
        undoStack = new PathOverlayUndoStack<VectorPath>();
    }

    public void Show(VectorPath newPath)
    {
        if (IsActive)
        {
            return;
        }

        undoStack?.Dispose();
        undoStack = new PathOverlayUndoStack<VectorPath>();
        undoStack.AddState(new VectorPath(newPath));
        Path = newPath;
        IsActive = true;
    }

    public void Hide()
    {
        IsActive = false;
        Path = null;
    }

    public void Undo()
    {
        suppressUndo = true;
        Path = new VectorPath(undoStack?.Undo());
        suppressUndo = false;
    }

    public void Redo()
    {
        suppressUndo = true;
        Path = new VectorPath(undoStack?.Redo());
        suppressUndo = false;
    }

    private void AddToUndo(VectorPath toAdd)
    {
        if (suppressUndo)
        {
            return;
        }

        undoStack?.AddState(new VectorPath(path));
    }

    private void PathDataChanged(VectorPath path)
    {
        AddToUndo(path);
    }
}
