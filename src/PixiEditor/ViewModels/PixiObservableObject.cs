using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Models.Commands;

namespace PixiEditor.ViewModels;

public class PixiObservableObject : ObservableObject
{
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        CommandController.Current.NotifyPropertyChanged(e.PropertyName);
    }
}
