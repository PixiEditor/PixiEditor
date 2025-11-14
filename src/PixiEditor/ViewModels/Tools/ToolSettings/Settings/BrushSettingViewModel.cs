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

    public ObservableCollection<Brush> AllBrushes => new ObservableCollection<Brush>(Library.Brushes.Values);
    public BrushSettingViewModel(string name, string label) : base(name)
    {
        Label = label;
        Library.BrushesChanged += () => OnPropertyChanged(nameof(AllBrushes));
    }

    protected override object AdjustValue(object value)
    {
        if (value is string str)
        {
            var found = Library.Brushes.Values.FirstOrDefault(b => b.Name == str);
            if (found != null)
                return found;

            return Library.Brushes.First().Value;
        }

        return base.AdjustValue(value);
    }
}
