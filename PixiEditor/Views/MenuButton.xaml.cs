using PixiEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;

namespace PixiEditor.Views
{
    /// <summary>
    /// Interaction logic for MenuButton.xaml
    /// </summary>
    public partial class MenuButton : UserControl
    {
        MenuButtonViewModel dc = new MenuButtonViewModel();
        public MenuButton()
        {
            InitializeComponent();
            this.DataContext = dc;
        }

        public static readonly DependencyProperty MenuButtonTextProperty = DependencyProperty.Register("Text", typeof(String), typeof(MenuButton), new UIPropertyMetadata(string.Empty));
        public String Text
        {
            get { return (string)GetValue(MenuButtonTextProperty); }
            set { SetValue(MenuButtonTextProperty, value); }
        }



        public object Item
        {
            get { return GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(object), typeof(MenuButton), new PropertyMetadata(null));

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            dc.CloseListViewCommand.Execute(null);
        }
    }
}
