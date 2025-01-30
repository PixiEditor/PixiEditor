using CommunityToolkit.Mvvm.ComponentModel;
using Drawie.Backend.Core.Text;
using Drawie.Numerics;
using PixiEditor.Helpers.UI;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

internal class TextOverlayViewModel : ObservableObject, ITextOverlayHandler
{
    private bool isActive;
    private string text;
    private VecD position;
    private Font font;
    private ExecutionTrigger<string> requestEditTextTrigger;

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

    public Font Font
    {
        get => font;
        set => SetProperty(ref font, value);
    }

    public ExecutionTrigger<string> RequestEditTextTrigger
    {
        get => requestEditTextTrigger;
        set => SetProperty(ref requestEditTextTrigger, value);
    }

    public TextOverlayViewModel()
    {
        RequestEditTextTrigger = new ExecutionTrigger<string>();
    }


    public void Show(string text, VecD position, Font font)
    {
        IsActive = true;
        Font = font;
        Position = position;
        Text = text;
        RequestEditTextTrigger.Execute(this, text);
    }

    public void Hide()
    {
        IsActive = false;
    }
}
