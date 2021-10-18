using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels;
using System.Linq;
using System.Windows;

namespace PixiEditor
{
    /// <summary>
    ///     Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);

            if (ViewModelMain.Current.BitmapManager.Documents.Any(x => !x.ChangesSaved))
            {
                ConfirmationType confirmation = ConfirmationDialog.Show($"{e.ReasonSessionEnding} with unsaved data. Are you sure?", $"{e.ReasonSessionEnding}");
                e.Cancel = confirmation != ConfirmationType.Yes;
            }
        }
    }
}
