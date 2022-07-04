using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PixiEditor.Views.UserControls
{
    /// <summary>
    /// Interaction logic for PrependTextBlock.xaml
    /// </summary>
    public partial class PrependTextBlock : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(PrependTextBlock));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty PrependProperty =
            DependencyProperty.Register(nameof(Prepend), typeof(string), typeof(PrependTextBlock));

        public string Prepend
        {
            get => (string)GetValue(PrependProperty);
            set => SetValue(PrependProperty, value);
        }

        public static readonly DependencyProperty AppendProperty =
            DependencyProperty.Register(nameof(Append), typeof(string), typeof(PrependTextBlock));

        public string Append
        {
            get => (string)GetValue(AppendProperty);
            set => SetValue(AppendProperty, value);
        }

        public static readonly DependencyProperty PrependColorProperty =
            DependencyProperty.Register(nameof(PrependColor), typeof(Brush), typeof(PrependTextBlock), new PropertyMetadata(Brushes.White));

        public Brush PrependColor
        {
            get => (Brush)GetValue(PrependColorProperty);
            set => SetValue(PrependColorProperty, value);
        }

        public static readonly DependencyProperty AppendColorProperty =
            DependencyProperty.Register(nameof(AppendColor), typeof(Brush), typeof(PrependTextBlock), new PropertyMetadata(Brushes.White));

        public Brush AppendColor
        {
            get => (Brush)GetValue(AppendColorProperty);
            set => SetValue(AppendColorProperty, value);
        }

        public bool HidePrepend
        {
            get { return (bool)GetValue(HidePrependProperty); }
            set { SetValue(HidePrependProperty, value); }
        }


        public static readonly DependencyProperty HidePrependProperty =
            DependencyProperty.Register(nameof(HidePrepend), typeof(bool), typeof(PrependTextBlock), new PropertyMetadata(false));



        public PrependTextBlock()
        {
            InitializeComponent();
        }
    }
}
