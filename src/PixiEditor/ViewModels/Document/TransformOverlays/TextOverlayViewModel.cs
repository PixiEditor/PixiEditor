using CommunityToolkit.Mvvm.ComponentModel;
using Drawie.Numerics;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

public class TextOverlayViewModel : ObservableObject, ITextOverlayHandler
{
    private bool isActive;
    private string text;
    private VecD position;
    private double fontSize;
    
    public event Action<string>? TextChanged;

    public bool IsActive
    {
        get => isActive;
        set => SetProperty(ref isActive, value);
    }

    public string Text
    {
        get => text;
        set
        {
            SetProperty(ref text, value);
            if (IsActive)
            {
                TextChanged?.Invoke(value);
            }
        }
    }

    public VecD Position
    {
        get => position;
        set => SetProperty(ref position, value);
    }

    public double FontSize
    {
        get => fontSize;
        set => SetProperty(ref fontSize, value);
    }

    public void Show(string text, VecD position, double fontSize)
    {
        IsActive = true;
        Text = text;
        Position = position;
        FontSize = fontSize;
    }
    
    public void Hide()
    {
        IsActive = false;
    }
}
