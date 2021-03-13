using System.Windows;
using System.Windows.Controls;
using PixiEditor.Models.Controllers;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LayersManager.xaml.
    /// </summary>
    public partial class LayersManager : UserControl
    {
        public LayersViewModel LayersViewModel
        {
            get { return (LayersViewModel)GetValue(LayersViewModelProperty); }
            set { SetValue(LayersViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LayersViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayersViewModelProperty =
            DependencyProperty.Register("LayersViewModel", typeof(LayersViewModel), typeof(LayersManager), new PropertyMetadata(default(LayersViewModel)));

        public BitmapManager BitmapManager
        {
            get { return (BitmapManager)GetValue(BitmapManagerProperty); }
            set { SetValue(BitmapManagerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BitmapManager.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BitmapManagerProperty =
            DependencyProperty.Register("BitmapManager", typeof(BitmapManager), typeof(LayersManager), new PropertyMetadata(default(BitmapManager)));

        public LayersManager()
        {
            InitializeComponent();
        }
    }
}