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
            DependencyProperty.Register(nameof(PrependColor), typeof(Brush), typeof(PrependTextBlock));

        public Brush PrependColor
        {
            get => (Brush)GetValue(PrependColorProperty);
            set => SetValue(PrependColorProperty, value);
        }

        public static readonly DependencyProperty AppendColorProperty =
            DependencyProperty.Register(nameof(AppendColor), typeof(Brush), typeof(PrependTextBlock));

        public Brush AppendColor
        {
            get => (Brush)GetValue(AppendColorProperty);
            set => SetValue(AppendColorProperty, value);
        }

        public PrependTextBlock()
        {
            InitializeComponent();
        }
    }
}
