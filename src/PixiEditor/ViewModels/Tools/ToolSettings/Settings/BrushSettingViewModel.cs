using System.Collections.ObjectModel;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Controllers;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal class BrushSettingViewModel : Setting<Brush>
{
    private static BrushLibrary library;

    private static BrushLibrary Library
    {
        get
        {
            if (library == null)
            {
                library = (ViewModelMain.Current.ToolsSubViewModel as ToolsViewModel).BrushLibrary;
            }

            return library;
        }
    }

    public ObservableCollection<Brush> AllBrushes => new ObservableCollection<Brush>(Library.Brushes);
    public BrushSettingViewModel(string name, string label) : base(name)
    {
        Label = label;
    }
}
