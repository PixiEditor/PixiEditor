using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.Models.Layers;
using PixiEditor.ViewModels.SubViewModels.Main;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for LayerFolder.xaml.
    /// </summary>
    public partial class LayerGroupControl : UserControl
    {
        public Guid GroupGuid
        {
            get { return (Guid)GetValue(GroupGuidProperty); }
            set { SetValue(GroupGuidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupGuidProperty =
            DependencyProperty.Register("GroupGuid", typeof(Guid), typeof(LayerGroupControl), new PropertyMetadata(Guid.NewGuid()));

        public LayersViewModel LayersViewModel
        {
            get { return (LayersViewModel)GetValue(LayersViewModelProperty); }
            set { SetValue(LayersViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LayersViewModelProperty =
            DependencyProperty.Register("LayersViewModel", typeof(LayersViewModel), typeof(LayerGroupControl), new PropertyMetadata(default(LayersViewModel)));

        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FolderName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(LayerGroupControl), new PropertyMetadata(default(string)));

        public GuidStructureItem GroupData
        {
            get { return (GuidStructureItem)GetValue(GroupDataProperty); }
            set { SetValue(GroupDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GroupDataProperty =
            DependencyProperty.Register("GroupData", typeof(GuidStructureItem), typeof(LayerGroupControl), new PropertyMetadata(default(GuidStructureItem)));

        public LayerGroupControl()
        {
            InitializeComponent();
        }

        private void Grid_DragEnter(object sender, DragEventArgs e)
        {
            Grid item = sender as Grid;

            item.Background = LayerItem.HighlightColor;
        }

        private void Grid_DragLeave(object sender, DragEventArgs e)
        {
            Grid grid = (Grid)sender;

            LayerItem.RemoveDragEffect(grid);
        }

        private void HandleDrop(IDataObject dataObj, bool above)
        {
            Guid referenceLayer = above ? (Guid)GroupData.EndLayerGuid : (Guid)GroupData.StartLayerGuid;

            if (dataObj.GetDataPresent("PixiEditor.Views.UserControls.LayerStructureItemContainer"))
            {
                var data = (LayerStructureItemContainer)dataObj.GetData("PixiEditor.Views.UserControls.LayerStructureItemContainer");
                Guid group = data.Layer.LayerGuid;

                data.LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.MoveLayerInStructure(group, referenceLayer, above);
                data.LayerCommandsViewModel.Owner.BitmapManager.ActiveDocument.LayerStructure.AssignParent(group, GroupData.Parent);
            }

            if (dataObj.GetDataPresent("PixiEditor.Views.UserControls.LayerGroupControl"))
            {
                var data = (LayerGroupControl)dataObj.GetData("PixiEditor.Views.UserControls.LayerGroupControl");
                Guid group = data.GroupGuid;
                var document = data.LayersViewModel.Owner.BitmapManager.ActiveDocument;

                Layer layer = document.Layers.First(x => x.LayerGuid == referenceLayer);
                int indexOfReferenceLayer = document.Layers.IndexOf(layer) + 1;

                Layer tempLayer = new("_temp");
                document.Layers.Insert(indexOfReferenceLayer, tempLayer);
                document.MoveFolderInStructure(group, tempLayer.LayerGuid, above);
                document.Layers.Remove(tempLayer);
            }
        }

        private void Grid_Drop_Top(object sender, DragEventArgs e)
        {
            HandleDrop(e.Data, true);
        }

        private void Grid_Drop_Bottom(object sender, DragEventArgs e)
        {
            HandleDrop(e.Data, false);
        }
    }
}