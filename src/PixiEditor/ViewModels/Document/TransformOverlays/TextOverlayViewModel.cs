using CommunityToolkit.Mvvm.ComponentModel;
using Drawie.Backend.Core.Numerics;
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
    private Matrix3X3 matrix = Matrix3X3.Identity;

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
    
    public Matrix3X3 Matrix
    {
        get => matrix;
        set => SetProperty(ref matrix, value);
    }

    public TextOverlayViewModel()
    {
        RequestEditTextTrigger = new ExecutionTrigger<string>();
    }


    public void Show(string text, VecD position, Font font, Matrix3X3 matrix)
    {
        Font = font;
        Position = position;
        Text = text;
        Matrix = matrix;
        IsActive = true;
        RequestEditTextTrigger.Execute(this, text);
    }

    public void Hide()
    {
        IsActive = false;
        Font = null!;
        Position = default;
        Text = string.Empty;
    }
}
