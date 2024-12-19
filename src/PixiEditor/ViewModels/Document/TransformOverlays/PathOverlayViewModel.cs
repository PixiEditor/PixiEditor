using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core.Vector;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

internal class PathOverlayViewModel : ObservableObject, IPathOverlayHandler
{
    private DocumentViewModel documentViewModel;
    private DocumentInternalParts internals;

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

    private RelayCommand<VectorPath> addToUndoCommand;

    public RelayCommand<VectorPath> AddToUndoCommand
    {
        get => addToUndoCommand;
        set => SetProperty(ref addToUndoCommand, value);
    }
    
    private bool showApplyButton;

    public bool ShowApplyButton
    {
        get => showApplyButton;
        set => SetProperty(ref showApplyButton, value);
    }

    private bool suppressUndo = false;

    public PathOverlayViewModel(DocumentViewModel documentViewModel, DocumentInternalParts internals)
    {
        this.documentViewModel = documentViewModel;
        this.internals = internals;
    }

    public void Show(VectorPath newPath, bool showApplyButton, Action<VectorPath>? customAddToUndo)
    {
        if (IsActive)
        {
            return;
        }

        Path = newPath;
        IsActive = true;
        ShowApplyButton = showApplyButton;
        AddToUndoCommand = new RelayCommand<VectorPath>(customAddToUndo);
    }

    public void Hide()
    {
        IsActive = false;
        Path = null;
        ShowApplyButton = false;
    }

    private void PathDataChanged(VectorPath path)
    {
        AddToUndoCommand.Execute(path);
    }
}
