using System.Windows;
using System.Windows.Controls;
using PixiEditor.ViewModels;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for MenuButton.xaml.
    /// </summary>
    public partial class MenuButton : UserControl
    {
        public static readonly DependencyProperty MenuButtonTextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(MenuButton), new UIPropertyMetadata(string.Empty));

        // Using a DependencyProperty as the backing store for Item.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProperty =
            DependencyProperty.Register("Item", typeof(object), typeof(MenuButton), new PropertyMetadata(null));

        private readonly MenuButtonViewModel dc = new MenuButtonViewModel();

        public MenuButton()
        {
            InitializeComponent();
            DataContext = dc;
        }

        public string Text
        {
            get => (string)GetValue(MenuButtonTextProperty);
            set => SetValue(MenuButtonTextProperty, value);
        }

        public object Item
        {
            get => GetValue(ItemProperty);
            set => SetValue(ItemProperty, value);
        }

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            dc.CloseListViewCommand.Execute(null);
        }
    }
}