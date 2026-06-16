using System.Collections.ObjectModel;
using Avalonia.Threading;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.Controllers;
using PixiEditor.ViewModels.BrushSystem;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Settings;

internal class BrushSettingViewModel : Setting<BrushViewModel>
{
    private static BrushLibrary library;

    private static BrushLibrary Library
    {
        get
        {
            if (library == null)
            {
                library = ViewModelMain.Current.BrushesSubViewModel.BrushLibrary;
            }

            return library;
        }
    }

    public ObservableCollection<BrushViewModel> AllBrushes => viewModels;

    private ObservableCollection<BrushViewModel> viewModels = new ObservableCollection<BrushViewModel>();

    public BrushSettingViewModel(string name, string label) : base(name)
    {
        Label = label;
        Library.BrushesChanged += () =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                List<BrushViewModel> existingVMs = viewModels.ToList();
                foreach (var vm in existingVMs)
                {
                    if (Library.Brushes.Values.All(b => b.OutputNodeId != vm.Brush.OutputNodeId))
                    {
                        viewModels.Remove(vm);
                    }
                }

                foreach (var brush in Library.Brushes.Values)
                {
                    if (existingVMs.All(vm => vm.Brush.OutputNodeId != brush.OutputNodeId))
                    {
                        viewModels.Add(new BrushViewModel(brush));
                    }
                }

                OnPropertyChanged(nameof(AllBrushes));
            });
        };

        viewModels =
            new ObservableCollection<BrushViewModel>(Library.Brushes.Values.Select(b => new BrushViewModel(b)));
        OnPropertyChanged(nameof(AllBrushes));
    }

    protected override object AdjustValue(object value)
    {
        if (value is string str)
        {
            var foundVm = viewModels.FirstOrDefault(b => b.Name == str);
            if (foundVm != null)
                return foundVm;

            var found = Library.Brushes.Values.FirstOrDefault(b => b.Name == str);
            if (found != null)
            {
                var vm = new BrushViewModel(found);
                viewModels.Add(vm);
            }

            if(viewModels.Count > 0)
                return viewModels[0];

            var firstFromLib = Library.Brushes.Values.FirstOrDefault();
            if (firstFromLib != null)
            {
                var vm = new BrushViewModel(firstFromLib);
                viewModels.Add(vm);
                return vm;
            }

            return null;
        }

        return base.AdjustValue(value);
    }
}
