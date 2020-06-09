using PixiEditor.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for ResizeCanvasPopup.xaml
    /// </summary>
    public partial class ResizeCanvasPopup : Window
    {
        public ResizeCanvasPopup()
        {
            InitializeComponent();
        }





        public AnchorPoint SelectedAnchorPoint
        {
            get { return (AnchorPoint)GetValue(SelectedAnchorPointProperty); }
            set { SetValue(SelectedAnchorPointProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedAnchorPoint.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedAnchorPointProperty =
            DependencyProperty.Register("SelectedAnchorPoint", typeof(AnchorPoint), typeof(ResizeCanvasPopup), new PropertyMetadata(AnchorPoint.Top | AnchorPoint.Left));





        public int NewHeight
        {
            get { return (int)GetValue(NewHeightProperty); }
            set { SetValue(NewHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewHeightProperty =
            DependencyProperty.Register("NewHeight", typeof(int), typeof(ResizeCanvasPopup), new PropertyMetadata(0));



        public int NewWidth
        {
            get { return (int)GetValue(NewWidthProperty); }
            set { SetValue(NewWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewWidthProperty =
            DependencyProperty.Register("NewWidth", typeof(int), typeof(ResizeCanvasPopup), new PropertyMetadata(0));



        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
