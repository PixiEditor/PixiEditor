using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace PixiEditor.Models.DataHolders;

public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    public ObservableRangeCollection()
    {
    }

    public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    public void AddRange(IEnumerable<T> collection)
    {
        foreach (var i in collection)
        {
            Items.Add(i);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection.ToList()));
    }

    public void ReplaceRange(IEnumerable<T> collection)
    {
        Items.Clear();
        foreach (var i in collection)
        {
            Items.Add(i);
        }

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
