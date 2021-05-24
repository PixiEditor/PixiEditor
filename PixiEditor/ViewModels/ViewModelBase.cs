using PixiEditor.Helpers;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.ViewModels
{
    public class ViewModelBase : NotifyableObject
    {
        protected void CloseButton(object parameter)
        {
            ((Window)parameter).Close();
        }

        protected void DragMove(object parameter)
        {
            Window popup = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (Mouse.LeftButton == MouseButtonState.Pressed) popup.DragMove();
        }
    }
}