using System.Diagnostics;
using PixiEditor.Helpers;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class DebugViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand BreakCommand { get; set; }

        public DebugViewModel(ViewModelMain owner)
            : base(owner)
        {
            BreakCommand = new RelayCommand(Break, CanBreak);
        }

        public static void Break(object parameter)
        {
            Debugger.Break();
        }

        public static bool CanBreak(object parameter) => Debugger.IsAttached;
    }
}