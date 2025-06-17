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
    private bool previewSize = false;
    private ExecutionTrigger<string> requestEditTextTrigger;
    private Matrix3X3 matrix = Matrix3X3.Identity;
    private double? spacing;
    private int cursorPosition;
    private int selectionEnd;

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

    public double? Spacing
    {
        get => spacing;
        set => SetProperty(ref spacing, value);
    }

    public int CursorPosition
    {
        get => cursorPosition;
        set => SetProperty(ref cursorPosition, value);
    }

    public bool PreviewSize
    {
        get => previewSize;
        set => SetProperty(ref previewSize, value);
    }

    public int SelectionEnd
    {
        get => selectionEnd;
        set => SetProperty(ref selectionEnd, value);
    }

    public void SetCursorPosition(VecD closestToPosition)
    {
        VecD mapped = Matrix.Invert().MapPoint(closestToPosition);
        RichText richText = new(Text);
        if (Font == null)
        {
            return;
        }

        var positions = richText.GetGlyphPositions(Font);
        if (positions == null || positions.Length == 0)
        {
            return;
        }

        int indexOfClosest = positions.Select((pos, index) => (pos, index))
            .OrderBy(pos => ((pos.pos + Position - new VecD(0, Font.Size / 2f)) - mapped).LengthSquared)
            .First().index;

        CursorPosition = indexOfClosest;
        SelectionEnd = indexOfClosest;
    }

    public TextOverlayViewModel()
    {
        RequestEditTextTrigger = new ExecutionTrigger<string>();
    }


    public void Show(string text, VecD position, Font font, Matrix3X3 matrix, double? spacing = null)
    {
        Font = font;
        Position = position;
        Text = text;
        Matrix = matrix;
        Spacing = spacing;
        IsActive = true;
        PreviewSize = false;
        RequestEditTextTrigger.Execute(this, text);
    }

    public void Hide()
    {
        IsActive = false;
        Font = null!;
        Position = default;
        Text = string.Empty;
        Matrix = Matrix3X3.Identity;
        Spacing = null;
    }
}
