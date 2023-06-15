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
using PixiEditor.Models.DataHolders.Guides;

namespace PixiEditor.Views.UserControls.Guides;
/// <summary>
/// Interaction logic for LineGuideSettings.xaml
/// </summary>
public partial class LineGuideSettings : UserControl
{
    internal LineGuideSettings(LineGuide guide)
    {
        DataContext = guide;
        InitializeComponent();
    }
}
