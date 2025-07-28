using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

internal class LineToolOverlayViewModel : ObservableObject, ILineOverlayHandler
{
    public event EventHandler<(VecD, VecD)>? LineMoved;

    private VecD lineStart;

    public VecD LineStart
    {
        get => lineStart;
        set
        {
            if (SetProperty(ref lineStart, value) && isInitialized)
                LineMoved?.Invoke(this, (lineStart, lineEnd));
        }
    }

    private VecD lineEnd;

    public VecD LineEnd
    {
        get => lineEnd;
        set
        {
            if (SetProperty(ref lineEnd, value) && isInitialized)
                LineMoved?.Invoke(this, (lineStart, lineEnd));
        }
    }

    private bool showHandles;
    public bool ShowHandles
    {
        get => showHandles;
        set => SetProperty(ref showHandles, value);
    }

    private bool isSizeBoxEnabled;
    public bool IsSizeBoxEnabled
    {
        get => isSizeBoxEnabled;
        set => SetProperty(ref isSizeBoxEnabled, value);
    }

    private bool isEnabled;

    public bool IsEnabled
    {
        get => isEnabled;
        set => SetProperty(ref isEnabled, value);
    }

    private ICommand? actionCompletedCommand = null;

    public ICommand? ActionCompletedCommand
    {
        get => actionCompletedCommand;
        set => SetProperty(ref actionCompletedCommand, value);
    }

    private ICommand addToUndoCommand = null;
    public ICommand AddToUndoCommand
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

    private bool isInitialized;
    
    public LineToolOverlayViewModel()
    {
    }

    public void Show(VecD lineStart, VecD endPos, bool showApplyButton, Action<(VecD, VecD)> addToUndo)
    {
        isInitialized = false;
        LineStart = lineStart;
        LineEnd = endPos; 
        IsEnabled = true;
        ShowApplyButton = showApplyButton;
        ShowHandles = true;
        IsSizeBoxEnabled = false;
        AddToUndoCommand = new RelayCommand(() => addToUndo((LineStart, LineEnd)));
        
        isInitialized = true;
    }

    public void Hide()
    {
        IsEnabled = false;
        ShowApplyButton = false;
        IsSizeBoxEnabled = false;
        isInitialized = false;
    }

    public bool Nudge(VecD distance)
    {
        LineStart = LineStart + distance;
        LineEnd = LineEnd + distance;
        return true;
    }
}
