using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Drawie.Numerics;
using PixiEditor.Models.Handlers;
using PixiEditor.Views.Overlays.ContextualOptions;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

public class ContextualOptionsViewModel : ObservableObject, IContextualOptionsHandler
{
    private bool isVisible;
    private VecD position;
    public ObservableCollection<ContextualOption> Options { get; set; } = new();

    public VecD Position
    {
        get => position;
        set => SetProperty(ref position, value);
    }

    public bool IsVisible
    {
        get => isVisible;
        set => SetProperty(ref isVisible, value);
    }

    public void SetOptions(IEnumerable<ContextualOption> options)
    {
        Options.Clear();
        Options.AddRange(options);
    }

    public void Show(VecD position)
    {
        Position = position;
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }
}
