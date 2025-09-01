using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings;
using PixiEditor.Models.Commands;

namespace PixiEditor.ViewModels;

public class PixiObservableObject : ObservableObject
{
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        CommandController.Current.NotifyPropertyChanged(e.PropertyName);
    }

    protected void SubscribeSettingsValueChanged<T>(Setting<T> settingStore, string propertyName) =>
        settingStore.ValueChanged += (_, _) => OnPropertyChanged(propertyName);
}
