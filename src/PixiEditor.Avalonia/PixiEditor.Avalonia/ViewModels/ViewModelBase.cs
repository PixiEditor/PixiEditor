using CommunityToolkit.Mvvm.ComponentModel;

namespace PixiEditor.Avalonia.ViewModels;

public class ViewModelBase : ObservableObject
{
    public void AddPropertyChangedCallback(string propertyName, Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            PropertyChanged += (_, _) => action();
            return;
        }

        PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == propertyName)
            {
                action();
            }
        };
    }
}
