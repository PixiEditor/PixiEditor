using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using PixiEditor.AvaloniaUI.Views.Animations;

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
    
    public void NotifyCollectionChanged(NotifyCollectionChangedAction action, List<KeyFrameViewModel> fames)
    {
        foreach (var frame in fames)
        {
            NotifyCollectionChanged(action, frame);
        }
    }

    public int FrameCount
    {
        get => Items.Count == 0 ? 0 : Items.Max(x => x.StartFrameBindable + x.DurationBindable);
    }

    public List<T> SelectChildrenBy<T>(Predicate<T> selector)
    {
       List<T> result = new List<T>();
       foreach (var group in Items)
       {
           foreach (var child in group.Children)
           {
               if (child is T target && selector(target))
               {
                   result.Add(target);
               }
           }
       }
       
       return result;
    }
}
