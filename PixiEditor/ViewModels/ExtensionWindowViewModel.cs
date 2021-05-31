using PixiEditor.Helpers;
using PixiEditor.SDK;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.ViewModels
{
    public class ExtensionWindowViewModel : ViewModelBase
    {
        private Extension selectedExtension;

        public ExtensionViewModel MainWindowExtensionsViewModel { get; }

        public Extension SelectedExtension { get => selectedExtension; set => SetProperty(ref selectedExtension, value); }

        public RelayCommand SelectExtensionCommand { get; }

        public ExtensionWindowViewModel(ExtensionViewModel extensionViewModel)
        {
            MainWindowExtensionsViewModel = extensionViewModel;
            SelectedExtension = MainWindowExtensionsViewModel.Extensions[0];
            SelectExtensionCommand = new RelayCommand(SelectExtension);
        }

        private void SelectExtension(object param)
        {
            SelectedExtension = (Extension)param;
        }
    }
}
