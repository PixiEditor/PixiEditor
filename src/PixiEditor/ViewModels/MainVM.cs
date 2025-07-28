using System.Collections.Generic;
using Avalonia.Markup.Xaml;

namespace PixiEditor.ViewModels;
internal class MainVM : MarkupExtension
{
    private MainVmEnum? vm;
    private static Dictionary<MainVmEnum, object> subVms = new();

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return vm != null ? subVms[vm.Value] : ViewModelMain.Current;
    }

    static MainVM()
    {
        var type = typeof(ViewModelMain);
        var vm = ViewModelMain.Current;
        
        foreach (var value in Enum.GetValues<MainVmEnum>())
        {
            subVms.Add(value, type.GetProperty(value.ToString().Replace("SVM", "SubViewModel").Replace("VM", "ViewModel"))?.GetValue(vm));
        }
    }
    
    public MainVM()
    {
        vm = null;
    }

    public MainVM(MainVmEnum vm)
    {
        this.vm = vm;
    }
}
