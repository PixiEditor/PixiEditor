using System.Threading.Tasks;
using Avalonia.Controls;
using PixiEditor.AvaloniaUI.Models;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.Extensions.CommonApi.UserPreferences;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

internal class NewFileDialog : CustomDialog
{
    private int height = IPreferences.Current.GetPreference("DefaultNewFileHeight", Constants.DefaultCanvasSize);

    private int width = IPreferences.Current.GetPreference("DefaultNewFileWidth", Constants.DefaultCanvasSize);

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

        return result;
    }
}
