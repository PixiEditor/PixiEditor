using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PixiEditor.Helpers;

namespace PixiEditor.Views
{
    /// <summary>
    ///     Interaction logic for LayerItem.xaml.
    /// </summary>
    public partial class LayerItem : UserControl
    {
        public static readonly DependencyProperty IsRenamingProperty = DependencyProperty.Register(
            "IsRenaming", typeof(bool), typeof(LayerItem), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            "IsActive", typeof(bool), typeof(LayerItem), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty SetActiveLayerCommandProperty = DependencyProperty.Register(
            "SetActiveLayerCommand", typeof(RelayCommand), typeof(LayerItem), new PropertyMetadata(default(RelayCommand)));

        public static readonly DependencyProperty LayerIndexProperty = DependencyProperty.Register(
            "LayerIndex", typeof(int), typeof(LayerItem), new PropertyMetadata(default(int)));

        public static readonly DependencyProperty LayerNameProperty = DependencyProperty.Register(
            "LayerName", typeof(string), typeof(LayerItem), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ControlButtonsVisibleProperty = DependencyProperty.Register(
            "ControlButtonsVisible", typeof(Visibility), typeof(LayerItem), new PropertyMetadata(Visibility.Hidden));

        // Using a DependencyProperty as the backing store for MoveToBackCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveToBackCommandProperty =
            DependencyProperty.Register("MoveToBackCommand", typeof(RelayCommand), typeof(LayerItem), new PropertyMetadata(default(RelayCommand)));

        public static readonly DependencyProperty MoveToFrontCommandProperty = DependencyProperty.Register(
            "MoveToFrontCommand", typeof(RelayCommand), typeof(LayerItem), new PropertyMetadata(default(RelayCommand)));

        public LayerItem()
        {
            InitializeComponent();
        }

        public bool IsRenaming
        {
            get => (bool)GetValue(IsRenamingProperty);
            set => SetValue(IsRenamingProperty, value);
        }

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public RelayCommand SetActiveLayerCommand
        {
            get => (RelayCommand)GetValue(SetActiveLayerCommandProperty);
            set => SetValue(SetActiveLayerCommandProperty, value);
        }

        public int LayerIndex
        {
            get => (int)GetValue(LayerIndexProperty);
            set => SetValue(LayerIndexProperty, value);
        }

        public string LayerName
        {
            get => (string)GetValue(LayerNameProperty);
            set => SetValue(LayerNameProperty, value);
        }

        public Visibility ControlButtonsVisible
        {
            get => (Visibility)GetValue(ControlButtonsVisibleProperty);
            set => SetValue(ControlButtonsVisibleProperty, value);
        }

        public RelayCommand MoveToBackCommand
        {
            get => (RelayCommand)GetValue(MoveToBackCommandProperty);
            set => SetValue(MoveToBackCommandProperty, value);
        }

        public RelayCommand MoveToFrontCommand
        {
            get => (RelayCommand)GetValue(MoveToFrontCommandProperty);
            set => SetValue(MoveToFrontCommandProperty, value);
        }

        private void LayerItem_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ControlButtonsVisible = Visibility.Visible;
        }

        private void LayerItem_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ControlButtonsVisible = Visibility.Hidden;
        }
    }
}