using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Dialogs;
using BackendColor = Drawie.Backend.Core.ColorsImpl.Color;

namespace PixiEditor.Views.Dialogs;

internal class NewFileDialog : CustomDialog
{
    private int width = PixiEditorSettings.File.DefaultNewFileWidth.Value;

    private int height = PixiEditorSettings.File.DefaultNewFileHeight.Value;

    public int Width
    {
        get => width;
        set => SetProperty(ref width, value);
    }

    public int Height
    {
        get => height;
        set => SetProperty(ref height, value);
    }

    public BackendColor? BackgroundColor { get; private set; }

    public NewFileDialog(Window owner) : base(owner)
    {

    }

    public override async Task<bool> ShowDialog()
    {
        NewFilePopup popup = new()
        {
            FileWidth = Width,
            FileHeight = Height
        };

        var result = await popup.ShowDialog<bool>(OwnerWindow);

        Height = popup.FileHeight;
        Width = popup.FileWidth;
        BackgroundColor = popup.UseCustomBackground && popup.BackgroundBrush is ISolidColorBrush solidBrush
            ? solidBrush.Color.ToColor()
            : null;

        return result;
    }
}
