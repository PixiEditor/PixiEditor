﻿using Microsoft.Win32;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PixiEditor.Views.UserControls.Layers
{
    /// <summary>
    /// Interaction logic for ReferenceLayer.xaml
    /// </summary>
    public partial class ReferenceLayer : UserControl
    {
        public Layer Layer
        {
            get { return (Layer)GetValue(ReferenceLayerProperty); }
            set { SetValue(ReferenceLayerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReferenceLayer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReferenceLayerProperty =
            DependencyProperty.Register("Layer", typeof(Layer), typeof(ReferenceLayer), new PropertyMetadata(default(Layer)));


        public ReferenceLayer()
        {
            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string path = OpenFilePicker();
            if (path != null)
            {
                var bitmap = Importer.ImportImage(path);
                Layer = new Layer("_Reference Layer", bitmap);
            }
        }

        private string OpenFilePicker()
        {

            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Reference layer path",
                CheckPathExists = true,
                Filter = "Image Files|*.png;*.jpeg;*.jpg|PNG Files|*.png|JPG Files|*.jpeg;*.jpg"
            };

            return (bool)dialog.ShowDialog() ? dialog.FileName : null;
        }

        private void TrashButton_Click(object sender, RoutedEventArgs e)
        {
            Layer = null;
        }
    }
}
