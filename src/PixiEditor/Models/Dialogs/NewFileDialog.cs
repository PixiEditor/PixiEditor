using PixiEditor.Extensions.Common.UserPreferences;
using PixiEditor.Views;
using PixiEditor.Views.Dialogs;

namespace PixiEditor.Models.Dialogs;

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

    public override bool ShowDialog()
    {
        NewFilePopup popup = new()
        {
            FileWidth = Width,
            FileHeight = Height
        };

        popup.ShowDialog();

        Height = popup.FileHeight;
        Width = popup.FileWidth;

        return popup.DialogResult.GetValueOrDefault();
    }
}
