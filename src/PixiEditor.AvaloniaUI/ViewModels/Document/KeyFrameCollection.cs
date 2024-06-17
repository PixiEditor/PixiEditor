using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class KeyFrameCollection : ObservableCollection<KeyFrameGroupViewModel>
{
    public KeyFrameCollection()
    {
        
    }

    public void NotifyCollectionChanged()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(FrameCount)));
    }

    public int FrameCount
    {
        get => Items.Count == 0 ? 0 : Items.Max(x => x.StartFrameBindable + x.DurationBindable);
    }
}
