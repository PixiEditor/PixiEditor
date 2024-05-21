using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.AvaloniaUI.Models.Commands;

namespace PixiEditor.AvaloniaUI.ViewModels;

public class PixiObservableObject : ObservableObject
{
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        CommandController.Current.NotifyPropertyChanged(e.PropertyName);
    }
}
