using Avalonia.Controls;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.Dialogs;

namespace PixiEditor.Views.Dialogs;

internal class NewFileDialog : CustomDialog
{
    private int width = PixiEditorSettings.File.DefaultNewFileWidth.Value;
    
    private int height = PixiEditorSettings.File.DefaultNewFileHeight.Value;

    private MeasurementUnit measurementUnit = MeasurementUnit.Pixels;

    private double dpi = 300;

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

    public MeasurementUnit MeasurementUnit
    {
        get => measurementUnit;
        set => SetProperty(ref measurementUnit, value);
    }

    public double Dpi
    {
        get => dpi;
        set => SetProperty(ref dpi, value);
    }

    public NewFileDialog(Window owner) : base(owner)
    {

    }

    public override async Task<bool> ShowDialog()
    {
        NewFilePopup popup = new()
        {
            FileWidth = Width,
            FileHeight = Height,
        };

        var result = await popup.ShowDialog<bool>(OwnerWindow);

        Height = popup.FileHeight;
        Width = popup.FileWidth;
        MeasurementUnit = popup.SelectedMeasurementUnit;
        Dpi = popup.Dpi;

        return result;
    }
}
