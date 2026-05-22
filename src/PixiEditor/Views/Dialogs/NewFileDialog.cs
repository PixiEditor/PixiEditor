using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.Models;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.Dialogs;

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

    public bool UseCustomBackground { get; private set; }

    public IBrush BackgroundBrush { get; private set; } = Brushes.White;

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
        UseCustomBackground = popup.UseCustomBackground;
        BackgroundBrush = popup.BackgroundBrush;

        return result;
    }
}
