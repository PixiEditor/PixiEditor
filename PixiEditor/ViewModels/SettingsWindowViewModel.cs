using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixiEditor.Helpers;
using PixiEditor.ViewModels.SubViewModels.UserPreferences;

namespace PixiEditor.ViewModels
{
    public class SettingsWindowViewModel : ViewModelBase
    {
        public RelayCommand SelectCategoryCommand { get; set; }

        private string selectedCategory = "General";

        public string SelectedCategory
        {
            get => selectedCategory;
            set
            {
                selectedCategory = value;
                RaisePropertyChanged(nameof(SelectedCategory));
            }
        }

        public SettingsViewModel SettingsSubViewModel { get; set; }

        public SettingsWindowViewModel()
        {
            SettingsSubViewModel = new SettingsViewModel(this);
            SelectCategoryCommand = new RelayCommand(SelectCategory);
        }

        private void SelectCategory(object parameter)
        {
            if (parameter is not null && parameter is string value)
            {
                SelectedCategory = value;
            }
        }
    }
}