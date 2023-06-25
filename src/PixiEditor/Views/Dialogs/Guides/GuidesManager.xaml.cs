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
using System.Windows.Shapes;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DataHolders.Guides;

namespace PixiEditor.Views.Dialogs.Guides;
/// <summary>
/// Interaction logic for GuideManager.xaml
/// </summary>
public partial class GuidesManager : Window
{
    public GuidesManager()
    {
        Owner = Application.Current.MainWindow;
        InitializeComponent();
        PreviewKeyDown += OnKeyDown;
        KeyUp += OnKeyUp;
    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        Hide();
    }

    public void SelectGuide(Index guideIndex)
    {
        var guides = (WpfObservableRangeCollection<Guide>)guideList.ItemsSource;
        guideList.SelectedIndex = guideIndex.GetOffset(ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument.Guides.Count);
    }

    private void GuideSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.RemovedItems.Count == 1)
        {
            var oldGuide = (Guide)e.RemovedItems[0];
            oldGuide.IsEditing = false;
        }


        if (e.AddedItems.Count == 1)
        {
            var newGuide = (Guide)e.AddedItems[0];
            newGuide.IsEditing = true;
        }
    }

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        guideList.SelectedIndex = -1;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsSupportedKey(e.Key))
        {
            return;
        }

        ShortcutController.BlockShortcutExecution("GuidesManager");
        e.Handled = true;
        var document = ViewModelMain.Current.DocumentManagerSubViewModel.ActiveDocument;

        switch (e.Key)
        {
            case Key.Delete when guideList.SelectedItem != null:
                document.Guides.Remove((Guide)guideList.SelectedItem);
                guideList.SelectedIndex = document.Guides.Count - 1;
                break;
            case Key.Up:
                var iU = guideList.SelectedIndex - 1;
                if (iU < 0)
                {
                    iU = document.Guides.Count - 1;
                }
                guideList.SelectedIndex = iU;
                break;
            case Key.Down:
                var iD = guideList.SelectedIndex + 1;
                if (iD >= guideList.Items.Count)
                {
                    iD = 0;
                }
                guideList.SelectedIndex = iD;
                break;
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (!IsSupportedKey(e.Key))
        {
            return;
        }

        ShortcutController.UnblockShortcutExecution("GuidesManager");
        e.Handled = true;
    }

    private bool IsSupportedKey(Key key) => key == Key.Delete || key == Key.Up || key == Key.Down;

    private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
    {
        var listItem = (ListViewItem)sender;
        var guide = (Guide)listItem.DataContext;

        guide.ShowExtended = true;
    }

    private void ListViewItem_MouseLeave(object sender, MouseEventArgs e)
    {
        var listItem = (ListViewItem)sender;
        if (listItem.DataContext is Guide guide)
        {
            guide.ShowExtended = false;
        }
    }
}
