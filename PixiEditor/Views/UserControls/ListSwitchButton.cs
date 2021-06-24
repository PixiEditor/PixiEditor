using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for ListSwitchButton.xaml
    /// </summary>
    public class ListSwitchButton : Button
    {
        public ObservableCollection<SwitchItem> Items
        {
            get { return (ObservableCollection<SwitchItem>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<SwitchItem>), typeof(ListSwitchButton), new PropertyMetadata(default(ObservableCollection<SwitchItem>), CollChanged));

        private static void CollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ListSwitchButton button = (ListSwitchButton)d;

            ObservableCollection<SwitchItem> oldVal = (ObservableCollection<SwitchItem>)e.OldValue;
            ObservableCollection<SwitchItem> newVal = (ObservableCollection<SwitchItem>)e.NewValue;
            if ((oldVal == null || oldVal.Count == 0) && newVal != null && newVal.Count > 0)
            {
                button.ActiveItem = newVal[0];
            }
        }

        public SwitchItem ActiveItem
        {
            get { return (SwitchItem)GetValue(ActiveItemProperty); }
            set { SetValue(ActiveItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveItemProperty =
            DependencyProperty.Register("ActiveItem", typeof(SwitchItem), typeof(ListSwitchButton), new PropertyMetadata(new SwitchItem(Brushes.Transparent, "", null)));


        static ListSwitchButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListSwitchButton), new FrameworkPropertyMetadata(typeof(ListSwitchButton)));
        }

        public ListSwitchButton()
        {
            Click += ListSwitchButton_Click;
        }

        private void ListSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Items.Contains(ActiveItem))
            {
                throw new ArgumentException("Items doesn't contain specified Item.");
            }

            int index = Items.IndexOf(ActiveItem) + 1;
            if (index > Items.Count - 1)
            {
                index = 0;
            }
            ActiveItem = Items[Math.Clamp(index, 0, Items.Count - 1)];
        }
    }
}
