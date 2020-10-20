using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.ViewModels
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void RaisePropertyChanged(string property)
        {
            if (property != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        protected void CloseButton(object parameter)
        {
            ((Window) parameter).Close();
        }

        protected void DragMove(object parameter)
        {
            var popup = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            if (Mouse.LeftButton == MouseButtonState.Pressed) popup.DragMove();
        }
    }
}