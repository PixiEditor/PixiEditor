using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LayerFolder.xaml.
    /// </summary>
    public partial class LayerFolder : UserControl
    {
        public Guid FolderGuid
        {
            get { return (Guid)GetValue(FolderGuidProperty); }
            set { SetValue(FolderGuidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FolderGuidProperty =
            DependencyProperty.Register("FolderGuid", typeof(Guid), typeof(LayerFolder), new PropertyMetadata(Guid.NewGuid()));

        public LayersViewModel LayersViewModel
        {
            get { return (LayersViewModel)GetValue(LayersViewModelProperty); }
            set { SetValue(LayersViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayersViewModelProperty =
            DependencyProperty.Register("LayersViewModel", typeof(LayersViewModel), typeof(LayerFolder), new PropertyMetadata(default(LayersViewModel)));

        public string FolderName
        {
            get { return (string)GetValue(FolderNameProperty); }
            set { SetValue(FolderNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FolderName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FolderNameProperty =
            DependencyProperty.Register("FolderName", typeof(string), typeof(LayerFolder), new PropertyMetadata(""));

        public LayerFolder()
        {
            InitializeComponent();
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void Grid_Drop_Top(object sender, DragEventArgs e)
        {

        }

        private void Grid_Drop_Bottom(object sender, DragEventArgs e)
        {

        }

        private void Grid_Drop_Middle(object sender, DragEventArgs e)
        {

        }

        private void FolderControl_MouseMove(object sender, MouseEventArgs e)
        {

        }
    }
}