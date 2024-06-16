using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using PixiEditor.AvaloniaUI.Models.Handlers;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class KeyFrameCollection : ObservableCollection<KeyFrameViewModel>
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
