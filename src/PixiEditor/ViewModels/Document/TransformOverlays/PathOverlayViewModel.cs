using CommunityToolkit.Mvvm.ComponentModel;
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
            if (SetProperty(ref path, value))
            {
                PathChanged?.Invoke(value);
            }
        }
    }

    public event Action<VectorPath>? PathChanged;
    public bool IsActive { get; set; }

    public PathOverlayViewModel(DocumentViewModel documentViewModel, DocumentInternalParts internals)
    {
        this.documentViewModel = documentViewModel;
        this.internals = internals;
    }

    public void Show(VectorPath path)
    {
        Path = path;
        IsActive = true;
    }
    
    public void Hide()
    {
        IsActive = false;
        Path = null;
    }
}
