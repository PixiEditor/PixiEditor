using System.Windows.Controls;
using PixiEditor.Models.DataHolders.Guides;

namespace PixiEditor.Views.UserControls.Guides;

public partial class GridGuideSettings : UserControl
{
    internal GridGuideSettings(GridGuide guide)
    {
        DataContext = guide;
        InitializeComponent();
    }
}

