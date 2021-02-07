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
    /// Interaction logic for DiscordRPPreview.xaml
    /// </summary>
    public partial class DiscordRPPreview : UserControl
    {
        public static readonly DependencyProperty StateProperty =
               DependencyProperty.Register(nameof(State), typeof(string), typeof(DiscordRPPreview), new PropertyMetadata("nothing"));

        public string State
        {
            get => (string)GetValue(StateProperty);
            set => SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty DetailProperty =
                DependencyProperty.Register(nameof(Detail), typeof(string), typeof(DiscordRPPreview), new PropertyMetadata("Staring at absolutely"));

        public string Detail
        {
            get => (string)GetValue(DetailProperty);
            set => SetValue(DetailProperty, value);
        }

        public static readonly DependencyProperty UserSourceProperty =
                DependencyProperty.Register(nameof(UserSource), typeof(string), typeof(DiscordRPPreview), new PropertyMetadata("../../Images/pixiBotLogo.png"));

        public string UserSource
        {
            get => (string)GetValue(UserSourceProperty);
            set => SetValue(UserSourceProperty, value);
        }

        public static readonly DependencyProperty IsPlayingProperty =
                DependencyProperty.Register(nameof(IsPlaying), typeof(bool), typeof(DiscordRPPreview), new PropertyMetadata(true));

        public bool IsPlaying
        {
            get => (bool)GetValue(IsPlayingProperty);
            set
            {
                SetValue(IsPlayingProperty, value);
            }
        }

        public DiscordRPPreview()
        {
            InitializeComponent();
        }
    }
}