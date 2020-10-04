using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PixiEditor.UpdateInstaller
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected void RaisePropertyChanged(string property)
        {
            if (property != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}