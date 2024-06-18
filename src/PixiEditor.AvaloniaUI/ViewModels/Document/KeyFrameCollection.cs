using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

internal class KeyFrameCollection : ObservableCollection<KeyFrameGroupViewModel>
{
    public KeyFrameCollection()
    {
        
    }
    
    public event Action<KeyFrameViewModel> KeyFrameAdded; 
    public event Action<KeyFrameViewModel> KeyFrameRemoved; 
    
    public void NotifyCollectionChanged()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(FrameCount)));
    }

    public void NotifyCollectionChanged(NotifyCollectionChangedAction action, KeyFrameViewModel keyFrame)
    {
        NotifyCollectionChanged();
        if (action == NotifyCollectionChangedAction.Add)
        {
            KeyFrameAdded?.Invoke(keyFrame);
        }
        else if (action == NotifyCollectionChangedAction.Remove)
        {
            KeyFrameRemoved?.Invoke(keyFrame);
        }
    }

    public int FrameCount
    {
        get => Items.Count == 0 ? 0 : Items.Max(x => x.StartFrameBindable + x.DurationBindable);
    }
}
