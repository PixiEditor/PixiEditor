using SkiaSharp;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for SwatchesView.xaml
    /// </summary>
    public partial class SwatchesView : UserControl
    {
        public static readonly DependencyProperty SwatchesProperty =
            DependencyProperty.Register(nameof(Swatches), typeof(ObservableCollection<SKColor>), typeof(SwatchesView));

        public ObservableCollection<SKColor> Swatches
        {
            get => (ObservableCollection<SKColor>)GetValue(SwatchesProperty);
            set => SetValue(SwatchesProperty, value);
        }

        public static readonly DependencyProperty MouseDownCommandProperty =
            DependencyProperty.Register(nameof(MouseDownCommand), typeof(ICommand), typeof(SwatchesView));

        public ICommand MouseDownCommand
        {
            get => (ICommand)GetValue(MouseDownCommandProperty);
            set => SetValue(MouseDownCommandProperty, value);
        }

        public static readonly DependencyProperty SelectSwatchCommandProperty =
            DependencyProperty.Register(nameof(SelectSwatchCommand), typeof(ICommand), typeof(SwatchesView));

        public ICommand SelectSwatchCommand
        {
            get => (ICommand)GetValue(SelectSwatchCommandProperty);
            set => SetValue(SelectSwatchCommandProperty, value);
        }

        public SwatchesView()
        {
            InitializeComponent();
        }
    }
}
