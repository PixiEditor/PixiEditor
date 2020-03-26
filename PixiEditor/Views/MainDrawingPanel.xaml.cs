using PixiEditor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Xceed.Wpf.Toolkit.Zoombox;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for MainDrawingPanel.xaml
    /// </summary>
    public partial class MainDrawingPanel : UserControl
    {
        public MainDrawingPanel()
        {
            InitializeComponent();
        }



        public double MouseX
        {
            get { return (double)GetValue(MouseXProperty); }
            set { SetValue(MouseXProperty, value);}
        }

        // Using a DependencyProperty as the backing store for MouseX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseXProperty =
            DependencyProperty.Register("MouseX", typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(null));

        public double MouseY
        {
            get { return (double)GetValue(MouseYProperty); }
            set { SetValue(MouseYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseX.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseYProperty =
            DependencyProperty.Register("MouseY", typeof(double), typeof(MainDrawingPanel), new PropertyMetadata(null));


        public ICommand MouseMoveCommand
        {
            get { return (ICommand)GetValue(MouseMoveCommandProperty); }
            set { SetValue(MouseMoveCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseMoveCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseMoveCommandProperty =
            DependencyProperty.Register("MouseMoveCommand", typeof(ICommand), typeof(MainDrawingPanel), new PropertyMetadata(null));



        public bool CenterOnStart
        {
            get { return (bool)GetValue(CenterOnStartProperty); }
            set { SetValue(CenterOnStartProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CenterOnStart.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterOnStartProperty =
            DependencyProperty.Register("CenterOnStart", typeof(bool), typeof(MainDrawingPanel), new PropertyMetadata(false));



        public object Item
        {
            get { return (object)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(object), typeof(MainDrawingPanel), new PropertyMetadata(0));




        private void Zoombox_Loaded(object sender, RoutedEventArgs e)
        {
            if(CenterOnStart == true)
            {
                ((Zoombox)sender).CenterContent();
            }
        }
    }
}
