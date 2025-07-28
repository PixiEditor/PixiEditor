using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using PixiEditor.Models.Handlers;
using PixiEditor.Views.Animations;

namespace PixiEditor.ViewModels.Document;

internal class KeyFrameCollection : ObservableCollection<CelGroupViewModel>
{
    public KeyFrameCollection()
    {
        
    }

    public KeyFrameCollection(IEnumerable<CelGroupViewModel> source)
    {
        foreach (var handler in source)
        {
            Add(handler);
        }
    }

    public event Action<CelViewModel> KeyFrameAdded; 
    public event Action<CelViewModel> KeyFrameRemoved; 
    
    public void NotifyCollectionChanged()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(FrameCount)));
    }

    public void NotifyCollectionChanged(NotifyCollectionChangedAction action, CelViewModel cel)
    {
        NotifyCollectionChanged();
        if (action == NotifyCollectionChangedAction.Add)
        {
            KeyFrameAdded?.Invoke(cel);
        }
        else if (action == NotifyCollectionChangedAction.Remove)
        {
            KeyFrameRemoved?.Invoke(cel);
        }
    }
    
    public void NotifyCollectionChanged(NotifyCollectionChangedAction action, List<CelViewModel> fames)
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
